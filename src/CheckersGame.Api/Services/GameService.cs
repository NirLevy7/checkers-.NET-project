using CheckersGame.Api.GameEngine;
using CheckersGame.Data.Context;
using CheckersGame.Data.Entities;
using CheckersGame.Shared;
using CheckersGame.Shared.Dtos;
using CheckersGame.Shared.Enums;
using CheckersGame.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace CheckersGame.Api.Services;

// Handles all game logic: start a game, validate a human move, pick the server's move.
public class GameService
{
    private readonly CheckersCentralDb _db;
    private readonly Random _rng = new();

    public GameService(CheckersCentralDb db) => _db = db;

    // ── Start a new game ────────────────────────────────────────────────────
    public async Task<StartGameResponse> StartGameAsync(StartGameRequest req)
    {
        var board = new BoardModel();  // fresh board — pieces in starting positions

        var game = new Game
        {
            StartTime        = DateTime.Now,
            TimePerMoveSec   = req.TimePerMoveSec,
            NumberOfPlayers  = req.PlayerIds.Count,
            BoardJson        = BoardSerializer.Serialize(board),
            Status           = GameStatus.InProgress,
            CurrentTurnIndex = 0
        };

        // Add each player to the game with their turn order
        int order = 1;
        foreach (int pid in req.PlayerIds)
            game.GamePlayers.Add(new GamePlayer { PlayerId = pid, TurnOrder = order++ });

        _db.Games.Add(game);
        await _db.SaveChangesAsync();

        return new StartGameResponse
        {
            GameId             = game.Id,
            InitialBoard       = game.BoardJson,
            FirstHumanPlayerId = req.PlayerIds[0],
            StartedAt          = game.StartTime
        };
    }

    // ── Human submits a move ────────────────────────────────────────────────
    public async Task<MakeMoveResponse> MakeMoveAsync(int gameId, MakeMoveRequest req)
    {
        // Load game and its players from the database
        var game = await _db.Games
            .Include(g => g.GamePlayers.OrderBy(gp => gp.TurnOrder))
            .FirstOrDefaultAsync(g => g.Id == gameId);

        if (game == null || game.Status != GameStatus.InProgress)
            return Reject("Game not found or already finished.");

        // Deserialize the saved board state
        var board = BoardSerializer.Deserialize(game.BoardJson);

        // 1. Validate the human's move against the game rules
        var (ok, error) = MoveValidator.Validate(board, req.Move, Side.Human);
        if (!ok) return Reject(error!);

        // 2. Apply the human's move to the board
        board = MoveValidator.Apply(board, req.Move, Side.Human);
        game.MovesCount++;

        // 3. Check if human won
        if (MoveValidator.CheckWin(board, Side.Human) == GameResult.Win)
            return await FinishGame(game, board, GameStatus.HumanWon, null);

        // 4. Server picks a random legal move
        var legalMoves = MoveValidator.GetAllLegalMoves(board, Side.Server);
        if (legalMoves.Count == 0)
            return await FinishGame(game, board, GameStatus.HumanWon, null);

        var serverMove = legalMoves[_rng.Next(legalMoves.Count)];
        board = MoveValidator.Apply(board, serverMove, Side.Server);
        game.MovesCount++;

        // 5. Check if server won
        if (MoveValidator.CheckWin(board, Side.Server) == GameResult.Win)
            return await FinishGame(game, board, GameStatus.ServerWon, serverMove);

        // 6. Advance to next human player (round-robin rotation)
        game.CurrentTurnIndex = (game.CurrentTurnIndex + 1) % game.NumberOfPlayers;
        int nextPlayerId = game.GamePlayers.ElementAt(game.CurrentTurnIndex).PlayerId;

        // 7. Save updated board back to database
        game.BoardJson = BoardSerializer.Serialize(board);
        await _db.SaveChangesAsync();

        return new MakeMoveResponse
        {
            Accepted     = true,
            ServerMove   = serverMove,
            BoardJson    = game.BoardJson,
            GameOver     = false,
            NextPlayerId = nextPlayerId
        };
    }

    // ── Human ran out of time → server wins ─────────────────────────────────
    public async Task<MakeMoveResponse> TimeoutAsync(int gameId, int playerId)
    {
        var game = await _db.Games.FindAsync(gameId);
        if (game == null || game.Status != GameStatus.InProgress)
            return Reject("Game not found or already finished.");

        game.Status  = GameStatus.ServerWon;
        game.EndTime = DateTime.Now;
        await _db.SaveChangesAsync();

        return new MakeMoveResponse
        {
            Accepted  = true,
            GameOver  = true,
            Winner    = GameStatus.ServerWon,
            BoardJson = game.BoardJson
        };
    }

    // ── Helpers ──────────────────────────────────────────────────────────────
    private static MakeMoveResponse Reject(string reason) =>
        new() { Accepted = false, RejectReason = reason };

    private async Task<MakeMoveResponse> FinishGame(
        Game game, BoardModel board, GameStatus winner, MoveDto? serverMove)
    {
        game.Status    = winner;
        game.EndTime   = DateTime.Now;
        game.BoardJson = BoardSerializer.Serialize(board);
        await _db.SaveChangesAsync();

        return new MakeMoveResponse
        {
            Accepted   = true,
            ServerMove = serverMove,
            BoardJson  = game.BoardJson,
            GameOver   = true,
            Winner     = winner
        };
    }
}

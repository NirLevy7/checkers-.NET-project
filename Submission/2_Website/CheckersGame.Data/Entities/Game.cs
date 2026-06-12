using CheckersGame.Shared.Enums;

namespace CheckersGame.Data.Entities;

// One row = one complete game session.
// A game can have 1-10 human players sharing the same client (spec §10).
// They all play against the Server, rotating turns.
public class Game
{
    public int        Id               { get; set; }
    public DateTime   StartTime        { get; set; }
    public DateTime?  EndTime          { get; set; }
    public int        TimePerMoveSec   { get; set; }  // chosen at registration: 2/5/10/15
    public GameStatus Status           { get; set; } = GameStatus.InProgress;
    public int        NumberOfPlayers  { get; set; }
    public int        MovesCount       { get; set; } = 0;

    // The current board state stored as JSON.
    // We save this so the server always knows the exact board position.
    public string     BoardJson        { get; set; } = "";

    // Which slot in the rotation plays next (0 = first player, 1 = second, etc.)
    public int        CurrentTurnIndex { get; set; } = 0;

    // Navigation property — EF Core uses this to load the players in this game
    public ICollection<GamePlayer> GamePlayers { get; set; } = new List<GamePlayer>();
}

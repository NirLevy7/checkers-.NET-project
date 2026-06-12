using CheckersGame.Shared.Enums;
using CheckersGame.Shared.Models;
using CheckersGame.Shared.Dtos;

namespace CheckersGame.Api.GameEngine;

/// <summary>
/// All game-rule logic lives here. Pure functions — no DB access.
/// Board layout: row 0 = Server side (top), row 7 = Human side (bottom).
/// Human pieces move UP (decreasing row). Server pieces move DOWN (increasing row).
///
/// Move geometry:
///   Normal step : 1 row forward, 1 column sideways  (colDelta = 1)
///   Capture     : 2 rows forward, 2 columns sideways (colDelta = 2) — jumps diagonally OVER the enemy
///   Backward    : 1 row backward, 1 column sideways  (colDelta = 1, one-time only)
/// </summary>
public static class MoveValidator
{
    // Returns true + null error if the move is legal for the given side
    public static (bool ok, string? error) Validate(BoardModel board, MoveDto move, Side side)
    {
        int fr = move.FromRow, fc = move.FromCol;
        int tr = move.ToRow,   tc = move.ToCol;

        if (!InBounds(fr, fc) || !InBounds(tr, tc))
            return (false, "Position out of bounds.");

        if (board.Cells[fr, fc] != side)
            return (false, "No piece of yours at that position.");

        if (board.Cells[tr, tc] != null)
            return (false, "Target square is not empty.");

        int rowDelta = tr - fr;
        int colDelta = Math.Abs(tc - fc);

        // Forward direction: Human moves up (rowDelta = -1), Server moves down (rowDelta = +1)
        int forwardDir = side == Side.Human ? -1 : 1;

        if (move.IsBackward)
        {
            // One-time backward step — no capture, exactly 1 square diagonally backward
            if (colDelta != 1)
                return (false, "Move must be exactly one column diagonally.");
            if (board.UsedBackward[fr, fc])
                return (false, "This piece has already used its backward move.");
            if (rowDelta != -forwardDir)
                return (false, "Backward move must go exactly one row backward.");
            if (move.IsCapture)
                return (false, "Capture is not allowed during a backward move.");
            return (true, null);
        }

        if (move.IsCapture)
        {
            // Capture: jump 2 rows forward, 2 columns diagonally — land on the far side of the enemy.
            // The captured piece is at the midpoint: (fr + forwardDir, (fc + tc) / 2).
            if (rowDelta != forwardDir * 2)
                return (false, "Capture must jump exactly 2 rows forward.");
            if (colDelta != 2)
                return (false, "Capture must jump exactly 2 columns diagonally.");

            int midRow = fr + forwardDir;
            int midCol = (fc + tc) / 2;  // exact midpoint — works because colDelta == 2

            if (!InBounds(midRow, midCol))
                return (false, "Captured position out of bounds.");

            Side enemy = side == Side.Human ? Side.Server : Side.Human;
            if (board.Cells[midRow, midCol] != enemy)
                return (false, "No enemy piece to capture.");

            return (true, null);
        }

        // Normal forward step: exactly 1 row forward, 1 column diagonally
        if (colDelta != 1)
            return (false, "Move must be exactly one column diagonally.");
        if (rowDelta != forwardDir)
            return (false, "Normal move must go exactly one row forward.");

        return (true, null);
    }

    // Apply a validated move to a cloned board and return it
    public static BoardModel Apply(BoardModel board, MoveDto move, Side side)
    {
        var next = board.Clone();
        int fr = move.FromRow, fc = move.FromCol;
        int tr = move.ToRow,   tc = move.ToCol;

        next.Cells[tr, tc] = next.Cells[fr, fc];
        next.Cells[fr, fc] = null;

        // Carry over the backward-used flag to the new position
        next.UsedBackward[tr, tc] = next.UsedBackward[fr, fc];
        next.UsedBackward[fr, fc] = false;

        if (move.IsBackward)
            next.UsedBackward[tr, tc] = true;

        if (move.IsCapture && move.CapturedRow.HasValue && move.CapturedCol.HasValue)
            next.Cells[move.CapturedRow.Value, move.CapturedCol.Value] = null;

        return next;
    }

    // Check win conditions after a move by `movedSide`
    public static GameResult CheckWin(BoardModel board, Side movedSide)
    {
        // Reach opponent's back row
        int goalRow = movedSide == Side.Human ? 0 : BoardModel.Rows - 1;
        for (int c = 0; c < BoardModel.Cols; c++)
            if (board.Cells[goalRow, c] == movedSide)
                return GameResult.Win;

        // Check if the OTHER side has any legal move left
        Side other = movedSide == Side.Human ? Side.Server : Side.Human;
        if (!HasAnyLegalMove(board, other))
            return GameResult.Win;  // other side is blocked

        return GameResult.Continue;
    }

    public static List<MoveDto> GetAllLegalMoves(BoardModel board, Side side)
    {
        var moves = new List<MoveDto>();
        for (int r = 0; r < BoardModel.Rows; r++)
            for (int c = 0; c < BoardModel.Cols; c++)
                if (board.Cells[r, c] == side)
                    moves.AddRange(GetMovesForPiece(board, r, c, side));
        return moves;
    }

    private static List<MoveDto> GetMovesForPiece(BoardModel board, int r, int c, Side side)
    {
        var result = new List<MoveDto>();
        int fwd = side == Side.Human ? -1 : 1;
        int[] colOffsets = { -1, 1 };

        foreach (int dc in colOffsets)
        {
            int nc = c + dc;      // adjacent column (enemy lives here on a capture)
            int lc = c + dc * 2;  // far column     (landing square on a capture)

            // Normal forward step: 1 row, 1 col
            int nr = r + fwd;
            if (InBounds(nr, nc) && board.Cells[nr, nc] == null)
                result.Add(new MoveDto { FromRow = r, FromCol = c, ToRow = nr, ToCol = nc });

            // Capture: jump 2 rows, 2 cols — enemy at (cr, nc), land at (tr, lc)
            int cr = r + fwd;       // enemy row (1 step forward)
            int tr = r + fwd * 2;   // landing row (2 steps forward)
            if (InBounds(cr, nc) && InBounds(tr, lc))
            {
                Side enemy = side == Side.Human ? Side.Server : Side.Human;
                if (board.Cells[cr, nc] == enemy && board.Cells[tr, lc] == null)
                    result.Add(new MoveDto
                    {
                        FromRow = r, FromCol = c, ToRow = tr, ToCol = lc,
                        IsCapture = true, CapturedRow = cr, CapturedCol = nc
                    });
            }

            // One-time backward step: 1 row backward, 1 col — no capture
            if (!board.UsedBackward[r, c])
            {
                int br = r - fwd;
                if (InBounds(br, nc) && board.Cells[br, nc] == null)
                    result.Add(new MoveDto
                    {
                        FromRow = r, FromCol = c, ToRow = br, ToCol = nc,
                        IsBackward = true
                    });
            }
        }

        return result;
    }

    private static bool HasAnyLegalMove(BoardModel board, Side side) =>
        GetAllLegalMoves(board, side).Count > 0;

    private static bool InBounds(int r, int c) =>
        r >= 0 && r < BoardModel.Rows && c >= 0 && c < BoardModel.Cols;
}

public enum GameResult { Continue, Win }

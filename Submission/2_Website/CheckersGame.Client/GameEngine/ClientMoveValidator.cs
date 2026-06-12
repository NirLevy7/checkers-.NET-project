using CheckersGame.Shared.Enums;
using CheckersGame.Shared.Models;

namespace CheckersGame.Client.GameEngine;

/// <summary>
/// Client-side move detection — mirrors the server rules so we can highlight
/// legal squares before the player submits anything to the server.
///
/// Capture geometry (same as server):
///   Enemy at  (row + fwd,     col + dc)      — 1 step diagonal
///   Land  at  (row + fwd * 2, col + dc * 2)  — 2 steps diagonal (far side of enemy)
/// </summary>
public static class ClientMoveValidator
{
    // Returns every square the selected piece can legally move to.
    public static List<(int Row, int Col, bool IsCapture, bool IsBackward,
                         int? CapRow, int? CapCol)>
        GetLegalTargets(BoardModel board, int row, int col)
    {
        var results = new List<(int, int, bool, bool, int?, int?)>();
        var side = board.Cells[row, col];
        if (side == null) return results;

        int fwd = side == Side.Human ? -1 : 1;
        int[] colOffsets = { -1, 1 };

        foreach (int dc in colOffsets)
        {
            int nc = col + dc;      // adjacent column
            int lc = col + dc * 2;  // far column (capture landing)

            // Normal forward step
            int nr = row + fwd;
            if (InBounds(nr, nc) && board.Cells[nr, nc] == null)
                results.Add((nr, nc, false, false, null, null));

            // Capture: jump over enemy at (cr, nc), land at (tr, lc)
            int cr = row + fwd;
            int tr = row + fwd * 2;
            if (InBounds(cr, nc) && InBounds(tr, lc))
            {
                Side enemy = side == Side.Human ? Side.Server : Side.Human;
                if (board.Cells[cr, nc] == enemy && board.Cells[tr, lc] == null)
                    results.Add((tr, lc, true, false, cr, nc));
            }

            // One-time backward step (no capture)
            if (!board.UsedBackward[row, col])
            {
                int br = row - fwd;
                if (InBounds(br, nc) && board.Cells[br, nc] == null)
                    results.Add((br, nc, false, true, null, null));
            }
        }

        return results;
    }

    private static bool InBounds(int r, int c) =>
        r >= 0 && r < BoardModel.Rows && c >= 0 && c < BoardModel.Cols;
}

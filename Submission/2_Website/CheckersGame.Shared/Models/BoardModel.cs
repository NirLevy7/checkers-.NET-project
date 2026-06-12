using CheckersGame.Shared.Enums;

namespace CheckersGame.Shared.Models;

/// <summary>
/// Board is 8 rows x 4 columns.
/// Row 0 = top (Server's starting side), Row 7 = bottom (Human's starting side).
/// Pieces on dark squares only — column index is already the "dark square" index per row.
/// </summary>
public class BoardModel
{
    public const int Rows = 8;
    public const int Cols = 4;

    // null = empty, Side.Human = human piece, Side.Server = server piece
    public Side?[,] Cells { get; set; } = new Side?[Rows, Cols];

    // Tracks which pieces have already used their one backward move
    public bool[,] UsedBackward { get; set; } = new bool[Rows, Cols];

    public BoardModel() => Reset();

    public void Reset()
    {
        Cells = new Side?[Rows, Cols];
        UsedBackward = new bool[Rows, Cols];

        // Server pieces start at rows 0-1 (top)
        for (int r = 0; r < 2; r++)
            for (int c = 0; c < Cols; c++)
                Cells[r, c] = Side.Server;

        // Human pieces start at rows 6-7 (bottom)
        for (int r = 6; r < Rows; r++)
            for (int c = 0; c < Cols; c++)
                Cells[r, c] = Side.Human;
    }

    public BoardModel Clone()
    {
        var clone = new BoardModel();
        Array.Copy(Cells, clone.Cells, Cells.Length);
        Array.Copy(UsedBackward, clone.UsedBackward, UsedBackward.Length);
        return clone;
    }
}

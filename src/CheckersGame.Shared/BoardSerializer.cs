using System.Text.Json;
using CheckersGame.Shared.Enums;
using CheckersGame.Shared.Models;

namespace CheckersGame.Shared;

/// <summary>
/// Converts BoardModel to/from a JSON string.
/// Lives in Shared so both the API (stores board in DB) and the Client (reads board from API response) can use it.
///
/// Format: a flat int[] of length 64 (8 rows × 4 cols × 2 values per cell).
///   For each cell: first int = piece (0=empty, 1=Human, 2=Server)
///                  second int = usedBackward flag (0=false, 1=true)
/// </summary>
public static class BoardSerializer
{
    public static string Serialize(BoardModel board)
    {
        var flat = new int[BoardModel.Rows * BoardModel.Cols * 2];
        int i = 0;
        for (int r = 0; r < BoardModel.Rows; r++)
            for (int c = 0; c < BoardModel.Cols; c++)
            {
                flat[i++] = board.Cells[r, c] == null ? 0
                           : board.Cells[r, c] == Side.Human ? 1 : 2;
                flat[i++] = board.UsedBackward[r, c] ? 1 : 0;
            }
        return JsonSerializer.Serialize(flat);
    }

    public static BoardModel Deserialize(string json)
    {
        var flat = JsonSerializer.Deserialize<int[]>(json)!;
        var board = new BoardModel();
        board.Cells        = new Side?[BoardModel.Rows, BoardModel.Cols];
        board.UsedBackward = new bool[BoardModel.Rows, BoardModel.Cols];
        int i = 0;
        for (int r = 0; r < BoardModel.Rows; r++)
            for (int c = 0; c < BoardModel.Cols; c++)
            {
                int cell = flat[i++];
                board.Cells[r, c]        = cell == 0 ? null : cell == 1 ? Side.Human : Side.Server;
                board.UsedBackward[r, c] = flat[i++] == 1;
            }
        return board;
    }
}

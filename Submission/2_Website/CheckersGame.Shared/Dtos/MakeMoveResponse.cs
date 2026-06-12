using CheckersGame.Shared.Enums;

namespace CheckersGame.Shared.Dtos;

public class MakeMoveResponse
{
    public bool       Accepted      { get; set; }
    public string?    RejectReason  { get; set; }

    // Server's counter-move (null if game over on human's move)
    public MoveDto?   ServerMove    { get; set; }

    // The updated board after both moves
    public string     BoardJson     { get; set; } = "";

    public bool       GameOver      { get; set; }
    public GameStatus? Winner       { get; set; }   // HumanWon or ServerWon when GameOver

    // Next human player's ID (for round-robin rotation)
    public int?       NextPlayerId  { get; set; }
}

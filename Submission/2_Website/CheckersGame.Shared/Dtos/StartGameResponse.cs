namespace CheckersGame.Shared.Dtos;

public class StartGameResponse
{
    public int    GameId              { get; set; }
    public string InitialBoard        { get; set; } = "";  // JSON-serialized board cells
    public int    FirstHumanPlayerId  { get; set; }
    public DateTime StartedAt         { get; set; }
}

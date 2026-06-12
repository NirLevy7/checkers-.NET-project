namespace CheckersGame.Client.ReplayDb;

public class ReplayMove
{
    public int    Id           { get; set; }
    public int    ReplayGameId { get; set; }
    public ReplayGame Game     { get; set; } = null!;
    public int    MoveNumber   { get; set; }
    public string Side         { get; set; } = "";  // "Human" or "Server"
    public int    FromRow      { get; set; }
    public int    FromCol      { get; set; }
    public int    ToRow        { get; set; }
    public int    ToCol        { get; set; }
    public int?   CapturedRow  { get; set; }
    public int?   CapturedCol  { get; set; }
    public bool   IsBackward   { get; set; }
}

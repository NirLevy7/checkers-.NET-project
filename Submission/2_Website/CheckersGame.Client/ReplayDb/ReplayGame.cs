namespace CheckersGame.Client.ReplayDb;

public class ReplayGame
{
    public int      Id            { get; set; }
    public int      ServerGameId  { get; set; }
    public string   PlayersJson   { get; set; } = ""; // JSON array of player names
    public DateTime StartTime     { get; set; }
    public int      DurationSec   { get; set; }
    public int      TimePerMoveSec{ get; set; }
    public string   WinnerSide    { get; set; } = ""; // "Human" or "Server"

    public ICollection<ReplayMove> Moves { get; set; } = new List<ReplayMove>();
}

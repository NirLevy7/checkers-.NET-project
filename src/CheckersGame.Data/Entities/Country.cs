namespace CheckersGame.Data.Entities;

public class Country
{
    public int    Id      { get; set; }
    public string Name    { get; set; } = "";

    public ICollection<Player> Players { get; set; } = new List<Player>();
}

namespace CheckersGame.Data.Entities;

// Join table: one game has 1-10 players
public class GamePlayer
{
    public int    Id         { get; set; }
    public int    GameId     { get; set; }
    public Game   Game       { get; set; } = null!;
    public int    PlayerId   { get; set; }
    public Player Player     { get; set; } = null!;
    public int    TurnOrder  { get; set; }  // 1 = first to move
}

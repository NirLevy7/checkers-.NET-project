namespace CheckersGame.Shared.Dtos;

public class StartGameRequest
{
    public List<int> PlayerIds       { get; set; } = new();
    public int       TimePerMoveSec  { get; set; } = 10;
}

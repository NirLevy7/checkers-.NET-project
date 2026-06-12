namespace CheckersGame.Shared.Dtos;

public class MakeMoveRequest
{
    public int     PlayerId   { get; set; }
    public MoveDto Move       { get; set; } = new();
}

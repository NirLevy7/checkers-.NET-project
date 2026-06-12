namespace CheckersGame.Shared.Dtos;

public class MoveDto
{
    public int FromRow { get; set; }
    public int FromCol { get; set; }
    public int ToRow   { get; set; }
    public int ToCol   { get; set; }
    public bool IsBackward { get; set; }  // used the one-time backward step
    public bool IsCapture  { get; set; }
    public int? CapturedRow { get; set; }
    public int? CapturedCol { get; set; }
}

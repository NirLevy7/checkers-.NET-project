namespace CheckersGame.Shared.Dtos;

public class PlayerDto
{
    public int    Id          { get; set; }
    public string FirstName   { get; set; } = "";
    public string Phone       { get; set; } = "";
    public string CountryName { get; set; } = "";
    public DateTime RegisteredAt { get; set; }
}

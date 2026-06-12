using System.ComponentModel.DataAnnotations;

namespace CheckersGame.Data.Entities;

public class Player
{
    // User-chosen ID: 1-1000, validated by DB CHECK constraint
    [Key]
    public int    Id           { get; set; }

    [Required, MinLength(2), MaxLength(50)]
    public string FirstName    { get; set; } = "";

    [Required, StringLength(10, MinimumLength = 10)]
    public string Phone        { get; set; } = "";

    public int    CountryId    { get; set; }
    public Country Country     { get; set; } = null!;

    public DateTime RegisteredAt { get; set; }

    public ICollection<GamePlayer> GamePlayers { get; set; } = new List<GamePlayer>();
}

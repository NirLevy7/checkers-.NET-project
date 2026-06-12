// Query 25: First player ever to play from each country
using CheckersGame.Data.Context;
using CheckersGame.Shared.Enums;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CheckersGame.Web.Pages.Queries;

public class FirstByCountryModel : PageModel
{
    private readonly CheckersCentralDb _db;
    public FirstByCountryModel(CheckersCentralDb db) => _db = db;

    public List<(string Country, string PlayerName, int PlayerId, DateTime FirstGame)> Results { get; set; } = new();

    public async Task OnGetAsync()
    {
        var finished = new[] { GameStatus.HumanWon, GameStatus.ServerWon };

        // Step 1: fetch one flat row per (player, finished game) from the DB.
        // EF Core can translate this Select with navigation properties just fine.
        var rows = await _db.GamePlayers
            .Where(gp => finished.Contains(gp.Game.Status))
            .Select(gp => new
            {
                CountryId = gp.Player.CountryId,
                Country   = gp.Player.Country.Name,
                Name      = gp.Player.FirstName,
                PlayerId  = gp.PlayerId,
                GameStart = gp.Game.StartTime
            })
            .ToListAsync();

        // Step 2: group by country in memory, pick the row with the earliest game
        // (= the first player ever to play from that country), sort A-Z by country.
        Results = rows
            .GroupBy(r => r.CountryId)
            .Select(g => g.OrderBy(r => r.GameStart).First())
            .OrderBy(r => r.Country)
            .Select(r => (Country: r.Country, PlayerName: r.Name,
                          PlayerId: r.PlayerId, FirstGame: r.GameStart))
            .ToList();
    }
}

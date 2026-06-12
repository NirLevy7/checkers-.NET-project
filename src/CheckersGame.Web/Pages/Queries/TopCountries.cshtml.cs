// Query 30: Top 2 countries by number of games, 2 columns only
using CheckersGame.Data.Context;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CheckersGame.Web.Pages.Queries;

public class TopCountriesModel : PageModel
{
    private readonly CheckersCentralDb _db;
    public TopCountriesModel(CheckersCentralDb db) => _db = db;

    public List<(string Country, int GameCount)> Results { get; set; } = new();

    public async Task OnGetAsync()
    {
        Results = await _db.Games
            .SelectMany(g => g.GamePlayers)
            .GroupBy(gp => gp.Player.Country.Name)
            .OrderByDescending(g => g.Count())
            .Take(2)
            .Select(g => ValueTuple.Create(g.Key, g.Count()))
            .ToListAsync();
    }
}

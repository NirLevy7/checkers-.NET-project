// Query 29: Players grouped by country
using CheckersGame.Data.Context;
using CheckersGame.Data.Entities;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CheckersGame.Web.Pages.Queries;

public class ByCountryModel : PageModel
{
    private readonly CheckersCentralDb _db;
    public ByCountryModel(CheckersCentralDb db) => _db = db;

    public List<(string Country, List<Player> Players)> Groups { get; set; } = new();

    public async Task OnGetAsync()
    {
        var all = await _db.Players
            .Include(p => p.Country)
            .OrderBy(p => p.Country.Name)
            .ThenBy(p => p.FirstName)
            .ToListAsync();

        Groups = all
            .GroupBy(p => p.Country.Name)
            .Select(g => (g.Key, g.ToList()))
            .ToList();
    }
}

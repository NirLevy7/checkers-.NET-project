// Query 27: Each player and how many games they played (2 columns only)
using CheckersGame.Data.Context;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CheckersGame.Web.Pages.Queries;

public class GamesPerPlayerModel : PageModel
{
    private readonly CheckersCentralDb _db;
    public GamesPerPlayerModel(CheckersCentralDb db) => _db = db;

    public List<(string Name, int Count)> Results { get; set; } = new();

    public async Task OnGetAsync()
    {
        // EF Core cannot translate ValueTuple.Create to SQL.
        // Step 1: run the DB query with an anonymous type (EF Core handles this fine).
        // Step 2: convert to named tuples in memory.
        var rows = await _db.Players
            .Select(p => new { p.FirstName, Count = p.GamePlayers.Count() })
            .OrderBy(p => p.FirstName.ToLower())
            .ToListAsync();

        Results = rows
            .Select(x => (Name: x.FirstName, Count: x.Count))
            .ToList();
    }
}

// Query 28: Players grouped by number of games, sorted desc, including "0 games" group
using CheckersGame.Data.Context;
using CheckersGame.Data.Entities;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CheckersGame.Web.Pages.Queries;

public class GroupedByCountModel : PageModel
{
    private readonly CheckersCentralDb _db;
    public GroupedByCountModel(CheckersCentralDb db) => _db = db;

    // Key = number of games played, Value = list of players in that group
    public List<(int GameCount, List<Player> Players)> Groups { get; set; } = new();

    public async Task OnGetAsync()
    {
        var playersWithCount = await _db.Players
            .Include(p => p.Country)
            .Select(p => new { Player = p, Count = p.GamePlayers.Count() })
            .ToListAsync();

        Groups = playersWithCount
            .GroupBy(x => x.Count)
            .OrderByDescending(g => g.Key)
            .Select(g => (g.Key, g.Select(x => x.Player).ToList()))
            .ToList();
    }
}

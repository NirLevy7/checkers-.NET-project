// Query 23: All players sorted desc by name (case-insensitive), show name + last game date only
using CheckersGame.Data.Context;
using CheckersGame.Shared.Enums;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CheckersGame.Web.Pages.Queries;

public class PlayersByNameModel : PageModel
{
    private readonly CheckersCentralDb _db;
    public PlayersByNameModel(CheckersCentralDb db) => _db = db;

    public List<(string Name, DateTime? LastGame)> Results { get; set; } = new();

    public async Task OnGetAsync()
    {
        Results = await _db.Players
            .Where(p => p.GamePlayers.Any(gp =>
                gp.Game.Status == GameStatus.HumanWon ||
                gp.Game.Status == GameStatus.ServerWon))
            .OrderByDescending(p => p.FirstName.ToLower())
            .Select(p => ValueTuple.Create(
                p.FirstName,
                p.GamePlayers
                    .Where(gp => gp.Game.Status == GameStatus.HumanWon || gp.Game.Status == GameStatus.ServerWon)
                    .Max(gp => (DateTime?)gp.Game.StartTime)
            ))
            .ToListAsync();
    }
}

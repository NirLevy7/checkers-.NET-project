// Query 24: All games with all details
using CheckersGame.Data.Context;
using CheckersGame.Data.Entities;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CheckersGame.Web.Pages.Queries;

public class AllGamesModel : PageModel
{
    private readonly CheckersCentralDb _db;
    public AllGamesModel(CheckersCentralDb db) => _db = db;

    public List<Game> Games { get; set; } = new();

    public async Task OnGetAsync()
    {
        Games = await _db.Games
            .Include(g => g.GamePlayers)
                .ThenInclude(gp => gp.Player)
            .OrderByDescending(g => g.StartTime)
            .ToListAsync();
    }
}

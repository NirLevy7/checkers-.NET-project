// Query 26: Pick a player from combo (case-insensitive dedup), get all their games
using CheckersGame.Data.Context;
using CheckersGame.Data.Entities;
using CheckersGame.Shared.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CheckersGame.Web.Pages.Queries;

public class GamesByPlayerModel : PageModel
{
    private readonly CheckersCentralDb _db;
    public GamesByPlayerModel(CheckersCentralDb db) => _db = db;

    [BindProperty(SupportsGet = true)]
    public string? SelectedName { get; set; }

    public List<SelectListItem> PlayerNames { get; set; } = new();
    public List<Game> Games { get; set; } = new();

    public async Task OnGetAsync()
    {
        // Distinct names (case-insensitive) sorted asc — using GroupBy on lowercase
        PlayerNames = await _db.Players
            .GroupBy(p => p.FirstName.ToLower())
            .OrderBy(g => g.Key)
            .Select(g => new SelectListItem
            {
                Value = g.First().FirstName,
                Text  = g.First().FirstName
            })
            .ToListAsync();

        if (!string.IsNullOrEmpty(SelectedName))
        {
            Games = await _db.Games
                .Include(g => g.GamePlayers)
                    .ThenInclude(gp => gp.Player)
                .Where(g => g.GamePlayers.Any(gp =>
                    gp.Player.FirstName.ToLower() == SelectedName.ToLower()))
                .OrderByDescending(g => g.StartTime)
                .ToListAsync();
        }
    }
}

// Query 22: All players with at least one finished game, sortable by name (case-insensitive)
using CheckersGame.Data.Context;
using CheckersGame.Data.Entities;
using CheckersGame.Shared.Enums;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CheckersGame.Web.Pages.Queries;

public class AllPlayersModel : PageModel
{
    private readonly CheckersCentralDb _db;
    public AllPlayersModel(CheckersCentralDb db) => _db = db;

    public List<Player> Players { get; set; } = new();
    public string SortOrder { get; set; } = "asc";

    public async Task OnGetAsync(string sort = "asc")
    {
        SortOrder = sort;
        Players = await BuildQuery(sort);
    }

    private Task<List<Player>> BuildQuery(string sort)
    {
        // LINQ only, no loops inside
        var query = _db.Players
            .Include(p => p.Country)
            .Include(p => p.GamePlayers)
                .ThenInclude(gp => gp.Game)
            .Where(p => p.GamePlayers.Any(gp =>
                gp.Game.Status == GameStatus.HumanWon ||
                gp.Game.Status == GameStatus.ServerWon));

        query = sort == "desc"
            ? query.OrderByDescending(p => p.FirstName.ToLower())
            : query.OrderBy(p => p.FirstName.ToLower());

        return query.ToListAsync();
    }
}

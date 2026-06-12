using CheckersGame.Data.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CheckersGame.Web.Pages;

public class DeleteModel : PageModel
{
    private readonly CheckersCentralDb _db;

    // ASP.NET injects the database automatically via the constructor
    public DeleteModel(CheckersCentralDb db) => _db = db;

    public string? Message { get; set; }

    // [BindProperty] = when the form is submitted, ASP.NET fills this from the form field
    [BindProperty] public int? DeleteGameId   { get; set; }
    [BindProperty] public int? DeletePlayerId { get; set; }

    public void OnGet() { }  // nothing to load on page open

    // ── Delete a single game ──────────────────────────────────────────────
    public async Task<IActionResult> OnPostDeleteGameAsync()
    {
        if (!DeleteGameId.HasValue)
        {
            Message = "Please enter a game ID.";
            return Page();
        }

        var game = await _db.Games.FindAsync(DeleteGameId.Value);
        if (game == null)
        {
            Message = $"Game {DeleteGameId} not found.";
            return Page();
        }

        // EF Core CASCADE: deleting the Game also deletes its GamePlayers and Moves rows
        _db.Games.Remove(game);
        await _db.SaveChangesAsync();

        Message = $"Game {DeleteGameId} deleted.";
        return Page();
    }

    // ── Delete a player (and all their games) ─────────────────────────────
    public async Task<IActionResult> OnPostDeletePlayerAsync()
    {
        if (!DeletePlayerId.HasValue)
        {
            Message = "Please enter a player ID.";
            return Page();
        }

        var player = await _db.Players.FindAsync(DeletePlayerId.Value);
        if (player == null)
        {
            Message = $"Player {DeletePlayerId} not found.";
            return Page();
        }

        // Step 1: find all games this player participated in
        var gameIds = await _db.GamePlayers
            .Where(gp => gp.PlayerId == DeletePlayerId.Value)
            .Select(gp => gp.GameId)
            .Distinct()
            .ToListAsync();

        // Step 2: delete those games
        // EF Core CASCADE will automatically delete the GamePlayers and Moves rows
        var games = await _db.Games
            .Where(g => gameIds.Contains(g.Id))
            .ToListAsync();
        _db.Games.RemoveRange(games);

        // Step 3: delete the player
        _db.Players.Remove(player);

        await _db.SaveChangesAsync();

        Message = $"Player {DeletePlayerId} and all {games.Count} of their games deleted.";
        return Page();
    }
}

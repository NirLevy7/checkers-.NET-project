using CheckersGame.Data.Context;
using CheckersGame.Data.Entities;
using CheckersGame.Shared.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace CheckersGame.Web.Pages;

public class RegisterModel : PageModel
{
    private readonly CheckersCentralDb _db;
    public RegisterModel(CheckersCentralDb db) => _db = db;

    // ── How many players are registering this session ──────────────────────
    [BindProperty]
    public int PlayerCount { get; set; } = 1;

    // ── The list of player forms (one per player) ──────────────────────────
    [BindProperty]
    public List<PlayerInput> Players { get; set; } = new();

    public List<SelectListItem> CountryOptions { get; set; } = new();

    public bool RegistrationSuccess { get; set; } = false;
    public int  NewGameId           { get; set; }

    public async Task OnGetAsync()
    {
        await LoadCountriesAsync();
        // Build empty forms for the default count
        Players = Enumerable.Range(0, PlayerCount)
                            .Select(_ => new PlayerInput()).ToList();
    }

    // ── User chose a player count → rebuild the form ───────────────────────
    public async Task<IActionResult> OnPostSetCountAsync()
    {
        await LoadCountriesAsync();
        Players = Enumerable.Range(0, PlayerCount)
                            .Select(_ => new PlayerInput()).ToList();
        ModelState.Clear();
        return Page();
    }

    // ── Final registration submission ──────────────────────────────────────
    public async Task<IActionResult> OnPostRegisterAsync()
    {
        await LoadCountriesAsync();

        // Extra cross-field validation: check ID uniqueness against DB
        for (int i = 0; i < Players.Count; i++)
        {
            var p = Players[i];
            if (p.Id < 1 || p.Id > 1000)
            {
                ModelState.AddModelError($"Players[{i}].Id", "ID must be between 1 and 1000.");
            }
            else if (await _db.Players.AnyAsync(x => x.Id == p.Id))
            {
                ModelState.AddModelError($"Players[{i}].Id",
                    $"ID {p.Id} already exists in the system. Please choose a different one.");
            }

            // Ensure IDs are unique within the current registration form too
            for (int j = 0; j < i; j++)
            {
                if (Players[j].Id == p.Id)
                {
                    ModelState.AddModelError($"Players[{i}].Id",
                        $"ID {p.Id} is used more than once in this form.");
                    break;
                }
            }
        }

        if (!ModelState.IsValid)
            return Page();

        // Create the game and add all players
        var game = new Game
        {
            StartTime       = DateTime.UtcNow,
            TimePerMoveSec  = 10,
            NumberOfPlayers = Players.Count,
            Status          = GameStatus.InProgress,
            BoardJson       = "" // will be set when game actually starts via API
        };

        int order = 1;
        foreach (var input in Players)
        {
            var player = new Player
            {
                Id           = input.Id,
                FirstName    = input.FirstName.Trim(),
                Phone        = input.Phone.Trim(),
                CountryId    = input.CountryId,
                RegisteredAt = DateTime.UtcNow
            };
            _db.Players.Add(player);
            game.GamePlayers.Add(new GamePlayer { Player = player, TurnOrder = order++ });
        }

        _db.Games.Add(game);
        await _db.SaveChangesAsync();

        RegistrationSuccess = true;
        NewGameId = game.Id;
        return Page();
    }

    private async Task LoadCountriesAsync()
    {
        CountryOptions = await _db.Countries
            .OrderBy(c => c.Name)
            .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
            .ToListAsync();
    }

    // ── Input model for one player row ─────────────────────────────────────
    public class PlayerInput
    {
        [Required(ErrorMessage = "First name is required.")]
        [MinLength(2, ErrorMessage = "First name must be at least 2 letters.")]
        [MaxLength(50)]
        public string FirstName { get; set; } = "";

        [Required(ErrorMessage = "ID is required.")]
        [Range(1, 1000, ErrorMessage = "ID must be a whole number between 1 and 1000.")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Phone is required.")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone must be exactly 10 digits.")]
        public string Phone { get; set; } = "";

        [Required(ErrorMessage = "Please select a country.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a country.")]
        public int CountryId { get; set; }
    }
}

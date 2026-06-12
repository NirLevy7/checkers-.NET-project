// §31: Update player details (name, phone, country). ID is NOT changeable.
using CheckersGame.Data.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace CheckersGame.Web.Pages;

public class EditPlayerModel : PageModel
{
    private readonly CheckersCentralDb _db;
    public EditPlayerModel(CheckersCentralDb db) => _db = db;

    public List<SelectListItem> CountryOptions { get; set; } = new();
    public string? SuccessMessage { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? PlayerId { get; set; }

    [BindProperty]
    public EditInput Input { get; set; } = new();

    public async Task OnGetAsync()
    {
        await LoadCountriesAsync();
        if (PlayerId.HasValue)
        {
            var player = await _db.Players.FindAsync(PlayerId.Value);
            if (player != null)
            {
                Input.FirstName = player.FirstName;
                Input.Phone     = player.Phone;
                Input.CountryId = player.CountryId;
            }
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadCountriesAsync();
        if (!PlayerId.HasValue) return Page();

        if (!ModelState.IsValid) return Page();

        var player = await _db.Players.FindAsync(PlayerId.Value);
        if (player == null)
        {
            ModelState.AddModelError("", "Player not found.");
            return Page();
        }

        player.FirstName = Input.FirstName.Trim();
        player.Phone     = Input.Phone.Trim();
        player.CountryId = Input.CountryId;
        await _db.SaveChangesAsync();

        SuccessMessage = $"Player {PlayerId} updated successfully.";
        return Page();
    }

    private async Task LoadCountriesAsync()
    {
        CountryOptions = await _db.Countries
            .OrderBy(c => c.Name)
            .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
            .ToListAsync();
    }

    public class EditInput
    {
        [Required(ErrorMessage = "First name is required.")]
        [MinLength(2, ErrorMessage = "First name must be at least 2 letters.")]
        [MaxLength(50)]
        public string FirstName { get; set; } = "";

        [Required(ErrorMessage = "Phone is required.")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone must be exactly 10 digits.")]
        public string Phone { get; set; } = "";

        [Range(1, int.MaxValue, ErrorMessage = "Please select a country.")]
        public int CountryId { get; set; }
    }
}

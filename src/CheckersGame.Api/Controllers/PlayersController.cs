using CheckersGame.Data.Context;
using CheckersGame.Shared.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CheckersGame.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlayersController : ControllerBase
{
    private readonly CheckersCentralDb _db;
    public PlayersController(CheckersCentralDb db) => _db = db;

    // GET api/players/{id}  — returns player info, 404 if not registered
    [HttpGet("{id}")]
    public async Task<IActionResult> GetPlayer(int id)
    {
        var player = await _db.Players
            .Include(p => p.Country)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (player == null) return NotFound();

        return Ok(new PlayerDto
        {
            Id           = player.Id,
            FirstName    = player.FirstName,
            Phone        = player.Phone,
            CountryName  = player.Country.Name,
            RegisteredAt = player.RegisteredAt
        });
    }
}

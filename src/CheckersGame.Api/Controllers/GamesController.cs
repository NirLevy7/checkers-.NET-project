using CheckersGame.Api.Services;
using CheckersGame.Shared.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace CheckersGame.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GamesController : ControllerBase
{
    private readonly GameService _svc;
    public GamesController(GameService svc) => _svc = svc;

    // POST api/games  — start a new game
    [HttpPost]
    public async Task<IActionResult> StartGame([FromBody] StartGameRequest req)
    {
        if (req.PlayerIds == null || req.PlayerIds.Count == 0)
            return BadRequest("At least one player is required.");

        var response = await _svc.StartGameAsync(req);
        return Ok(response);
    }

    // POST api/games/{id}/moves  — human submits a move
    [HttpPost("{gameId}/moves")]
    public async Task<IActionResult> MakeMove(int gameId, [FromBody] MakeMoveRequest req)
    {
        var response = await _svc.MakeMoveAsync(gameId, req);
        if (!response.Accepted)
            return BadRequest(response);
        return Ok(response);
    }

    // POST api/games/{id}/timeout  — human ran out of time → server wins
    [HttpPost("{gameId}/timeout")]
    public async Task<IActionResult> Timeout(int gameId, [FromBody] TimeoutRequest req)
    {
        var response = await _svc.TimeoutAsync(gameId, req.PlayerId);
        return Ok(response);
    }
}

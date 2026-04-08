using MaklerWebApp.API.Extensions;
using MaklerWebApp.BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MaklerWebApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FavoritesController : ControllerBase
{
    private readonly IFavoriteService _favoriteService;

    public FavoritesController(IFavoriteService favoriteService)
    {
        _favoriteService = favoriteService;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyFavorites(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _favoriteService.GetMyFavoritesAsync(userId.Value, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{listingId:int}")]
    public async Task<IActionResult> Add(int listingId, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var added = await _favoriteService.AddAsync(userId.Value, listingId, cancellationToken);
        return added ? NoContent() : NotFound();
    }

    [HttpDelete("{listingId:int}")]
    public async Task<IActionResult> Remove(int listingId, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var removed = await _favoriteService.RemoveAsync(userId.Value, listingId, cancellationToken);
        return removed ? NoContent() : NotFound();
    }
}

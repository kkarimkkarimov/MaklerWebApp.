using MaklerWebApp.API.Extensions;
using MaklerWebApp.API.Models;
using MaklerWebApp.API.Services;
using MaklerWebApp.BLL.Models;
using MaklerWebApp.BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MaklerWebApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ListingsController : ControllerBase
{
    private readonly IListingService _listingService;
    private readonly IImageStorageService _imageStorageService;

    public ListingsController(IListingService listingService, IImageStorageService imageStorageService)
    {
        _listingService = listingService;
        _imageStorageService = imageStorageService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Search([FromQuery] ListingSearchRequest request, CancellationToken cancellationToken)
    {
        var result = await _listingService.SearchAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id, [FromQuery] string? languageCode, CancellationToken cancellationToken)
    {
        var result = await _listingService.GetByIdAsync(id, languageCode, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMyListings([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? languageCode = null, CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _listingService.GetByUserIdAsync(userId.Value, page, pageSize, languageCode, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateListingRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _listingService.CreateAsync(request, userId.Value, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id, languageCode = request.Translations.FirstOrDefault()?.LanguageCode }, result);
    }

    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateListingRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var updated = await _listingService.UpdateAsync(id, request, userId.Value, cancellationToken);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var deleted = await _listingService.DeleteAsync(id, userId.Value, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    [HttpPatch("{id:int}/moderate")]
    [Authorize]
    public async Task<IActionResult> Moderate(int id, [FromBody] ModerateListingRequest request, CancellationToken cancellationToken)
    {
        var updated = await _listingService.ModerateAsync(id, request, cancellationToken);
        return updated ? NoContent() : NotFound();
    }

    [HttpPatch("{id:int}/featured")]
    [Authorize]
    public async Task<IActionResult> SetFeatured(int id, [FromBody] SetFeaturedRequest request, CancellationToken cancellationToken)
    {
        var updated = await _listingService.SetFeaturedAsync(id, request, cancellationToken);
        return updated ? NoContent() : NotFound();
    }

    [HttpPatch("{id:int}/ad-status")]
    [Authorize]
    public async Task<IActionResult> SetAdStatus(int id, [FromBody] PatchListingAdStatusRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var updated = await _listingService.SetAdStatusAsync(id, request, userId.Value, cancellationToken);
        return updated ? NoContent() : NotFound();
    }

    [HttpPost("{id:int}/views")]
    [AllowAnonymous]
    public async Task<IActionResult> AddView(int id, CancellationToken cancellationToken)
    {
        await _listingService.AddViewAsync(id, User.GetUserId(), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:int}/images")]
    [Authorize]
    public async Task<IActionResult> AddImages(int id, [FromBody] ListingImageUrlsRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var updated = await _listingService.AddImagesAsync(id, request.ImageUrls, userId.Value, cancellationToken);
        return updated ? NoContent() : NotFound();
    }

    [HttpPost("{id:int}/images/upload")]
    [Authorize]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> AddImagesUpload(int id, List<IFormFile> files, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        if (files.Count == 0)
        {
            return BadRequest(new { message = "At least one image file is required." });
        }

        var uploadedUrls = new List<string>(files.Count);
        foreach (var file in files)
        {
            var imageUrl = await _imageStorageService.SaveAsync(file, "listings", cancellationToken);
            uploadedUrls.Add(imageUrl);
        }

        var updated = await _listingService.AddImagesAsync(id, uploadedUrls, userId.Value, cancellationToken);
        if (!updated)
        {
            foreach (var uploadedUrl in uploadedUrls)
            {
                await _imageStorageService.DeleteByUrlAsync(uploadedUrl, cancellationToken);
            }

            return NotFound();
        }

        return Ok(new { imageUrls = uploadedUrls });
    }

    [HttpDelete("{listingId:int}/images/{imageId:int}")]
    [Authorize]
    public async Task<IActionResult> DeleteImage(int listingId, int imageId, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var removedUrl = await _listingService.DeleteImageAsync(listingId, imageId, userId.Value, cancellationToken);
        if (removedUrl is null)
        {
            return NotFound();
        }

        await _imageStorageService.DeleteByUrlAsync(removedUrl, cancellationToken);
        return Ok(new { removedUrl });
    }

    [HttpPut("{listingId:int}/images/{imageId:int}")]
    [Authorize]
    public async Task<IActionResult> ReplaceImage(int listingId, int imageId, [FromBody] ReplaceListingImageRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var oldUrl = await _listingService.ReplaceImageAsync(listingId, imageId, request.NewImageUrl, userId.Value, cancellationToken);
        if (oldUrl is null)
        {
            return NotFound();
        }

        await _imageStorageService.DeleteByUrlAsync(oldUrl, cancellationToken);
        return Ok(new { oldUrl, newUrl = request.NewImageUrl });
    }

    [HttpPut("{listingId:int}/images/{imageId:int}/upload")]
    [Authorize]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> ReplaceImageUpload(int listingId, int imageId, IFormFile file, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var newUrl = await _imageStorageService.SaveAsync(file, "listings", cancellationToken);
        var oldUrl = await _listingService.ReplaceImageAsync(listingId, imageId, newUrl, userId.Value, cancellationToken);
        if (oldUrl is null)
        {
            await _imageStorageService.DeleteByUrlAsync(newUrl, cancellationToken);
            return NotFound();
        }

        await _imageStorageService.DeleteByUrlAsync(oldUrl, cancellationToken);
        return Ok(new { oldUrl, newUrl });
    }

    [HttpPut("{listingId:int}/images/reorder")]
    [Authorize]
    public async Task<IActionResult> ReorderImages(int listingId, [FromBody] ListingImageReorderRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var updated = await _listingService.ReorderImagesAsync(listingId, request, userId.Value, cancellationToken);
        return updated ? NoContent() : NotFound();
    }
}

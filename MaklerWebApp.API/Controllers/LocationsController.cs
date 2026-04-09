using MaklerWebApp.BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MaklerWebApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LocationsController : ControllerBase
{
    private readonly ILocationService _locationService;

    public LocationsController(ILocationService locationService)
    {
        _locationService = locationService;
    }

    [HttpGet("az")]
    [AllowAnonymous]
    public IActionResult GetAzerbaijanLocations()
    {
        var result = _locationService.GetAzerbaijanCities();
        return Ok(result);
    }
}

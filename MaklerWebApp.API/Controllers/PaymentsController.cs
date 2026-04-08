using MaklerWebApp.API.Extensions;
using MaklerWebApp.BLL.Models;
using MaklerWebApp.BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MaklerWebApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost("boost")]
    public async Task<IActionResult> StartBoost([FromBody] BoostPaymentRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _paymentService.StartBoostAsync(userId.Value, request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _paymentService.GetHistoryAsync(userId.Value, cancellationToken);
        return Ok(result);
    }
}

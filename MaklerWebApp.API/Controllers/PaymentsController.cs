using MaklerWebApp.API.Extensions;
using MaklerWebApp.API.Authorization;
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

    [HttpPost("boost/confirm")]
    [Authorize(Policy = AuthorizationPolicies.ListingsFeatureManagement)]
    public async Task<IActionResult> ConfirmBoost([FromBody] ConfirmBoostPaymentRequest request, CancellationToken cancellationToken)
    {
        var result = await _paymentService.ConfirmBoostAsync(request, cancellationToken);
        return result is null ? NotFound(new { message = "Pending payment transaction not found." }) : Ok(result);
    }
}

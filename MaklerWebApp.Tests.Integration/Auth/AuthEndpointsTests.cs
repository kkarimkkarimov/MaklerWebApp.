using MaklerWebApp.Tests.Integration.Infrastructure;
using MaklerWebApp.BLL.Models;
using MaklerWebApp.API.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using System.Text.Json;

namespace MaklerWebApp.Tests.Integration.Auth;

public class AuthEndpointsTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;

    public AuthEndpointsTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task SwaggerEndpoint_ReturnsSuccess()
    {
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });

        var response = await client.GetAsync("/swagger/index.html");

        Assert.True(response.IsSuccessStatusCode);
    }

    [Fact]
    public async Task VerifyOtp_InvalidPayload_ReturnsUnifiedValidationError()
    {
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });

        var response = await client.PostAsJsonAsync("/api/auth/verify-otp", new VerifyOtpRequest
        {
            EmailOrPhone = "test@example.com",
            Code = "12"
        });

        var payload = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal("validation_failed", payload!.Code);
    }
}

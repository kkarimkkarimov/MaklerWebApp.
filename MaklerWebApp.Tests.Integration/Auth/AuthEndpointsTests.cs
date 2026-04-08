using MaklerWebApp.Tests.Integration.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;

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
}

using MaklerWebApp.Tests.Integration.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;

namespace MaklerWebApp.Tests.Integration.Listing;

public class ListingsEndpointsTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;

    public ListingsEndpointsTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetMyListings_UnauthorizedWithoutToken()
    {
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });

        var response = await client.GetAsync("/api/listings/me");
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }
}

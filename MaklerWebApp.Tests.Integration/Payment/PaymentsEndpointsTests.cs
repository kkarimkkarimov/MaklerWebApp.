using MaklerWebApp.Tests.Integration.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;

namespace MaklerWebApp.Tests.Integration.Payment;

public class PaymentsEndpointsTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;

    public PaymentsEndpointsTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ConfirmBoost_UnauthorizedWithoutToken()
    {
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });

        var response = await client.PostAsJsonAsync("/api/payments/boost/confirm", new
        {
            reference = "ref-1234567890123456",
            paidAmount = 9,
            succeeded = true
        });

        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }
}

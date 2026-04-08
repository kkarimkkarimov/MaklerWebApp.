using MaklerWebApp.Tests.Integration.Infrastructure;

namespace MaklerWebApp.Tests.Integration.Listing;

public class ListingsEndpointsTests : IClassFixture<ApiWebApplicationFactory>
{
    [Fact(Skip = "Skeleton test. Add authenticated listing integration scenarios.")]
    public Task SearchEndpoint_ReturnsPagedResult()
    {
        return Task.CompletedTask;
    }
}

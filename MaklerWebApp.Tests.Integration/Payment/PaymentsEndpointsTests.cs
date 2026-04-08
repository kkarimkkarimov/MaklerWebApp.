using MaklerWebApp.Tests.Integration.Infrastructure;

namespace MaklerWebApp.Tests.Integration.Payment;

public class PaymentsEndpointsTests : IClassFixture<ApiWebApplicationFactory>
{
    [Fact(Skip = "Skeleton test. Add authenticated payment start/confirm scenarios.")]
    public Task ConfirmBoostEndpoint_UpdatesPendingPayment()
    {
        return Task.CompletedTask;
    }
}

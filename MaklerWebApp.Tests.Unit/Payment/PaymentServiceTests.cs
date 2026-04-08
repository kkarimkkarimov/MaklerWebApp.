namespace MaklerWebApp.Tests.Unit.Payment;

public class PaymentServiceTests
{
    [Fact]
    public async Task StartBoostAsync_UsesServerSideAmountAndCreatesPendingTransaction()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Listings.Add(new DAL.Entities.Listing
        {
            Id = 101,
            OwnerUserId = 7,
            Title = "Listing",
            Description = "Description",
            Price = 150000,
            CurrencyType = DAL.Enums.CurrencyType.AZN,
            Area = 80,
            Rooms = 3,
            PropertyType = DAL.Enums.PropertyType.Apartment,
            ListingType = DAL.Enums.ListingType.Sale,
            City = "Baku",
            District = "Yasamal",
            Address = "Addr",
            ContactName = "Owner",
            ContactPhone = "+994501112233"
        });
        await dbContext.SaveChangesAsync();

        var service = new PaymentService(dbContext);
        var result = await service.StartBoostAsync(7, new BoostPaymentRequest
        {
            ListingId = 101,
            ServiceType = MaklerWebApp.BLL.Contracts.Enums.PaymentServiceType.Vip
        });

        var transaction = await dbContext.PaymentTransactions.SingleAsync();
        Assert.Equal(49m, result.Amount);
        Assert.Equal(MaklerWebApp.BLL.Contracts.Enums.PaymentStatus.Pending, result.Status);
        Assert.Equal(DAL.Enums.PaymentStatus.Pending, transaction.Status);
    }

    [Fact]
    public async Task ConfirmBoostAsync_OnMatchingAmount_MarksSuccessAndFeaturesListing()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Listings.Add(new DAL.Entities.Listing
        {
            Id = 102,
            OwnerUserId = 8,
            Title = "Listing",
            Description = "Description",
            Price = 200000,
            CurrencyType = DAL.Enums.CurrencyType.AZN,
            Area = 100,
            Rooms = 4,
            PropertyType = DAL.Enums.PropertyType.House,
            ListingType = DAL.Enums.ListingType.Sale,
            City = "Baku",
            District = "Sabail",
            Address = "Addr",
            ContactName = "Owner",
            ContactPhone = "+994501112233"
        });
        await dbContext.SaveChangesAsync();

        var service = new PaymentService(dbContext);
        var started = await service.StartBoostAsync(8, new BoostPaymentRequest
        {
            ListingId = 102,
            ServiceType = MaklerWebApp.BLL.Contracts.Enums.PaymentServiceType.Boost
        });

        var confirmed = await service.ConfirmBoostAsync(new ConfirmBoostPaymentRequest
        {
            Reference = started.Reference,
            PaidAmount = started.Amount,
            Succeeded = true
        });

        var listing = await dbContext.Listings.SingleAsync(x => x.Id == 102);
        Assert.NotNull(confirmed);
        Assert.Equal(MaklerWebApp.BLL.Contracts.Enums.PaymentStatus.Success, confirmed!.Status);
        Assert.True(listing.IsFeatured);
        Assert.NotNull(listing.FeaturedUntil);
    }

    private static MaklerDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<MaklerDbContext>()
            .UseInMemoryDatabase($"payment-tests-{Guid.NewGuid()}")
            .Options;

        return new MaklerDbContext(options);
    }
}

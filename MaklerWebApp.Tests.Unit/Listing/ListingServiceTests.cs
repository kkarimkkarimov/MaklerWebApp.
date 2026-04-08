namespace MaklerWebApp.Tests.Unit.Listing;

public class ListingServiceTests
{
    [Fact]
    public async Task SearchAsync_EnforcesApprovedAndNotDeletedFilters()
    {
        var repository = new CapturingListingRepository();
        await using var dbContext = CreateDbContext();
        var service = new ListingService(repository, dbContext);

        await service.SearchAsync(new ListingSearchRequest
        {
            Status = MaklerWebApp.BLL.Contracts.Enums.ListingStatus.Rejected,
            IncludeDeleted = true,
            Page = 1,
            PageSize = 20
        });

        Assert.NotNull(repository.LastCriteria);
        Assert.Equal(DAL.Enums.ListingStatus.Approved, repository.LastCriteria!.Status);
        Assert.False(repository.LastCriteria.IncludeDeleted);
    }

    [Fact]
    public async Task SetAdStatusAsync_OnlyOwnerCanUpdate()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Listings.Add(new DAL.Entities.Listing
        {
            Id = 1,
            OwnerUserId = 10,
            Title = "T",
            Description = "D",
            Price = 100,
            CurrencyType = DAL.Enums.CurrencyType.AZN,
            Area = 50,
            Rooms = 2,
            PropertyType = DAL.Enums.PropertyType.Apartment,
            ListingType = DAL.Enums.ListingType.Sale,
            City = "Baku",
            District = "Nizami",
            Address = "Addr",
            ContactName = "Owner",
            ContactPhone = "+994501112233"
        });
        await dbContext.SaveChangesAsync();

        var service = new ListingService(new CapturingListingRepository(), dbContext);

        var wrongOwnerResult = await service.SetAdStatusAsync(1, new PatchListingAdStatusRequest
        {
            AdStatus = MaklerWebApp.BLL.Contracts.Enums.AdStatus.Sold
        }, ownerUserId: 99);

        var rightOwnerResult = await service.SetAdStatusAsync(1, new PatchListingAdStatusRequest
        {
            AdStatus = MaklerWebApp.BLL.Contracts.Enums.AdStatus.Sold
        }, ownerUserId: 10);

        var listing = await dbContext.Listings.SingleAsync(x => x.Id == 1);
        Assert.False(wrongOwnerResult);
        Assert.True(rightOwnerResult);
        Assert.Equal(DAL.Enums.AdStatus.Sold, listing.AdStatus);
    }

    private static MaklerDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<MaklerDbContext>()
            .UseInMemoryDatabase($"listing-tests-{Guid.NewGuid()}")
            .Options;

        return new MaklerDbContext(options);
    }

    private sealed class CapturingListingRepository : DAL.Repositories.IListingRepository
    {
        public DAL.Models.ListingSearchCriteria? LastCriteria { get; private set; }

        public Task<DAL.Models.ListingSearchResult> SearchAsync(DAL.Models.ListingSearchCriteria criteria, CancellationToken cancellationToken = default)
        {
            LastCriteria = criteria;
            return Task.FromResult(new DAL.Models.ListingSearchResult());
        }

        public Task<DAL.Entities.Listing?> GetByIdAsync(int id, CancellationToken cancellationToken = default) => Task.FromResult<DAL.Entities.Listing?>(null);
        public Task<DAL.Entities.Listing> AddAsync(DAL.Entities.Listing listing, CancellationToken cancellationToken = default) => Task.FromResult(listing);
        public Task<bool> UpdateAsync(DAL.Entities.Listing listing, CancellationToken cancellationToken = default) => Task.FromResult(true);
        public Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken = default) => Task.FromResult(true);
        public Task<bool> SetStatusAsync(int id, DAL.Enums.ListingStatus status, string? moderationNote, CancellationToken cancellationToken = default) => Task.FromResult(true);
        public Task<bool> SetFeaturedAsync(int id, bool isFeatured, DateTime? featuredUntil, CancellationToken cancellationToken = default) => Task.FromResult(true);
    }
}

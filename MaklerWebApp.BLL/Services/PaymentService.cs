using MaklerWebApp.BLL.Models;
using MaklerWebApp.DAL.Data;
using MaklerWebApp.DAL.Entities;
using MaklerWebApp.DAL.Enums;
using Microsoft.EntityFrameworkCore;

namespace MaklerWebApp.BLL.Services;

public class PaymentService : IPaymentService
{
    private readonly MaklerDbContext _dbContext;

    public PaymentService(MaklerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PaymentHistoryDto> StartBoostAsync(int userId, BoostPaymentRequest request, CancellationToken cancellationToken = default)
    {
        var listing = await _dbContext.Listings.FirstOrDefaultAsync(x => x.Id == request.ListingId && !x.IsDeleted, cancellationToken);
        if (listing is null || listing.OwnerUserId != userId)
        {
            throw new ArgumentException("Listing not found for current user.");
        }

        var transaction = new PaymentTransaction
        {
            UserId = userId,
            ListingId = request.ListingId,
            ServiceType = request.ServiceType,
            Amount = request.Amount,
            Status = PaymentStatus.Success,
            Reference = Guid.NewGuid().ToString("N")
        };

        _dbContext.PaymentTransactions.Add(transaction);

        if (request.ServiceType is PaymentServiceType.Vip or PaymentServiceType.Premium or PaymentServiceType.Boost)
        {
            listing.IsFeatured = true;
            listing.FeaturedUntil = DateTime.UtcNow.AddDays(request.ServiceType == PaymentServiceType.Vip ? 30 : 14);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new PaymentHistoryDto
        {
            Id = transaction.Id,
            ListingId = transaction.ListingId,
            ServiceType = transaction.ServiceType,
            Amount = transaction.Amount,
            Status = transaction.Status,
            Reference = transaction.Reference,
            CreatedAt = transaction.CreatedAt
        };
    }

    public async Task<IReadOnlyList<PaymentHistoryDto>> GetHistoryAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.PaymentTransactions
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new PaymentHistoryDto
            {
                Id = x.Id,
                ListingId = x.ListingId,
                ServiceType = x.ServiceType,
                Amount = x.Amount,
                Status = x.Status,
                Reference = x.Reference,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }
}

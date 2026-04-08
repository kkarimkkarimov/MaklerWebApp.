using MaklerWebApp.BLL.Contracts.Enums;
using MaklerWebApp.BLL.Models;
using MaklerWebApp.DAL.Data;
using MaklerWebApp.DAL.Entities;
using Microsoft.EntityFrameworkCore;

using DalEnums = MaklerWebApp.DAL.Enums;

namespace MaklerWebApp.BLL.Services;

public class PaymentService : IPaymentService
{
    private static readonly IReadOnlyDictionary<PaymentServiceType, decimal> ServicePrices = new Dictionary<PaymentServiceType, decimal>
    {
        [PaymentServiceType.Vip] = 49.00m,
        [PaymentServiceType.Premium] = 29.00m,
        [PaymentServiceType.Boost] = 9.00m
    };

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

        var amount = GetServicePrice(request.ServiceType);

        var transaction = new PaymentTransaction
        {
            UserId = userId,
            ListingId = request.ListingId,
            ServiceType = (DalEnums.PaymentServiceType)request.ServiceType,
            Amount = amount,
            Status = DalEnums.PaymentStatus.Pending,
            Reference = Guid.NewGuid().ToString("N")
        };

        _dbContext.PaymentTransactions.Add(transaction);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new PaymentHistoryDto
        {
            Id = transaction.Id,
            ListingId = transaction.ListingId,
            ServiceType = (PaymentServiceType)transaction.ServiceType,
            Amount = transaction.Amount,
            Status = (PaymentStatus)transaction.Status,
            Reference = transaction.Reference,
            CreatedAt = transaction.CreatedAt
        };
    }

    public async Task<PaymentHistoryDto?> ConfirmBoostAsync(ConfirmBoostPaymentRequest request, CancellationToken cancellationToken = default)
    {
        var reference = request.Reference.Trim();
        var transaction = await _dbContext.PaymentTransactions
            .Include(x => x.Listing)
            .FirstOrDefaultAsync(x => x.Reference == reference, cancellationToken);

        if (transaction is null || transaction.Status != DalEnums.PaymentStatus.Pending)
        {
            return null;
        }

        if (!request.Succeeded)
        {
            transaction.Status = DalEnums.PaymentStatus.Failed;
            await _dbContext.SaveChangesAsync(cancellationToken);
            return MapToHistoryDto(transaction);
        }

        if (request.PaidAmount != transaction.Amount)
        {
            transaction.Status = DalEnums.PaymentStatus.Failed;
            await _dbContext.SaveChangesAsync(cancellationToken);
            throw new ArgumentException("Paid amount does not match expected amount.");
        }

        transaction.Status = DalEnums.PaymentStatus.Success;
        ApplyFeatureWindow(transaction.Listing, (PaymentServiceType)transaction.ServiceType);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToHistoryDto(transaction);
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
                ServiceType = (PaymentServiceType)x.ServiceType,
                Amount = x.Amount,
                Status = (PaymentStatus)x.Status,
                Reference = x.Reference,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }

    private static decimal GetServicePrice(PaymentServiceType serviceType)
    {
        if (!ServicePrices.TryGetValue(serviceType, out var amount))
        {
            throw new ArgumentOutOfRangeException(nameof(serviceType), serviceType, "Unsupported payment service type.");
        }

        return amount;
    }

    private static void ApplyFeatureWindow(Listing listing, PaymentServiceType serviceType)
    {
        listing.IsFeatured = true;
        listing.FeaturedUntil = DateTime.UtcNow.AddDays(serviceType == PaymentServiceType.Vip ? 30 : 14);
        listing.UpdatedAt = DateTime.UtcNow;
    }

    private static PaymentHistoryDto MapToHistoryDto(PaymentTransaction transaction)
    {
        return new PaymentHistoryDto
        {
            Id = transaction.Id,
            ListingId = transaction.ListingId,
            ServiceType = (PaymentServiceType)transaction.ServiceType,
            Amount = transaction.Amount,
            Status = (PaymentStatus)transaction.Status,
            Reference = transaction.Reference,
            CreatedAt = transaction.CreatedAt
        };
    }
}

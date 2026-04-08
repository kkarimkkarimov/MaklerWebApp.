using MaklerWebApp.BLL.Models;

namespace MaklerWebApp.BLL.Services;

public interface IPaymentService
{
    Task<PaymentHistoryDto> StartBoostAsync(int userId, BoostPaymentRequest request, CancellationToken cancellationToken = default);
    Task<PaymentHistoryDto?> ConfirmBoostAsync(ConfirmBoostPaymentRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PaymentHistoryDto>> GetHistoryAsync(int userId, CancellationToken cancellationToken = default);
}

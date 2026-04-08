namespace MaklerWebApp.API.Services;

public interface IImageStorageService
{
    Task<string> SaveAsync(IFormFile file, string folder, CancellationToken cancellationToken = default);
    Task<bool> DeleteByUrlAsync(string? fileUrl, CancellationToken cancellationToken = default);
}

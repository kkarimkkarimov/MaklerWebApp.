namespace MaklerWebApp.API.Services;

public sealed class LocalImageStorageService : IImageStorageService
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp", ".gif"
    };

    private const long MaxFileSizeBytes = 10 * 1024 * 1024;
    private readonly IWebHostEnvironment _environment;

    public LocalImageStorageService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<string> SaveAsync(IFormFile file, string folder, CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length == 0)
        {
            throw new ArgumentException("Image file is required.");
        }

        if (file.Length > MaxFileSizeBytes)
        {
            throw new ArgumentException("Image size cannot exceed 10 MB.");
        }

        var extension = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
        {
            throw new ArgumentException("Unsupported image format. Allowed: .jpg, .jpeg, .png, .webp, .gif");
        }

        var webRootPath = _environment.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRootPath))
        {
            webRootPath = Path.Combine(_environment.ContentRootPath, "wwwroot");
        }

        var folderPath = Path.Combine(webRootPath, "uploads", folder);
        Directory.CreateDirectory(folderPath);

        var fileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var physicalPath = Path.Combine(folderPath, fileName);

        await using var stream = new FileStream(physicalPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        await file.CopyToAsync(stream, cancellationToken);

        return $"/uploads/{folder}/{fileName}";
    }

    public Task<bool> DeleteByUrlAsync(string? fileUrl, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fileUrl))
        {
            return Task.FromResult(false);
        }

        var localPath = fileUrl;
        if (Uri.TryCreate(fileUrl, UriKind.Absolute, out var absoluteUri))
        {
            localPath = absoluteUri.LocalPath;
        }

        localPath = localPath.Replace('\\', '/').Trim();
        if (localPath.StartsWith('/'))
        {
            localPath = localPath[1..];
        }

        if (!localPath.StartsWith("uploads/", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(false);
        }

        var webRootPath = _environment.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRootPath))
        {
            webRootPath = Path.Combine(_environment.ContentRootPath, "wwwroot");
        }

        var physicalPath = Path.Combine(webRootPath, localPath.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(physicalPath))
        {
            return Task.FromResult(false);
        }

        File.Delete(physicalPath);
        return Task.FromResult(true);
    }
}

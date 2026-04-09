using MaklerWebApp.API.Options;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;

namespace MaklerWebApp.API.Services;

public sealed class LocalImageStorageService : IImageStorageService
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp", ".gif"
    };

    private readonly IWebHostEnvironment _environment;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ImageStorageOptions _options;
    private readonly FileExtensionContentTypeProvider _contentTypeProvider = new();

    public LocalImageStorageService(
        IWebHostEnvironment environment,
        IHttpContextAccessor httpContextAccessor,
        IOptions<ImageStorageOptions> options)
    {
        _environment = environment;
        _httpContextAccessor = httpContextAccessor;
        _options = options.Value;
    }

    public async Task<string> SaveAsync(IFormFile file, string folder, CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length == 0)
        {
            throw new ArgumentException("Image file is required.");
        }

        var maxFileSizeBytes = Math.Max(1, _options.MaxFileSizeMb) * 1024L * 1024L;
        if (file.Length > maxFileSizeBytes)
        {
            throw new ArgumentException($"Image size cannot exceed {_options.MaxFileSizeMb} MB.");
        }

        var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
        {
            throw new ArgumentException("Unsupported image format. Allowed: .jpg, .jpeg, .png, .webp, .gif");
        }

        if (!_contentTypeProvider.TryGetContentType($"file{extension}", out var expectedContentType)
            || string.IsNullOrWhiteSpace(file.ContentType)
            || !file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase)
            || !string.Equals(file.ContentType, expectedContentType, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Invalid image content type.");
        }

        var normalizedFolder = NormalizeFolder(folder);
        var datedPath = Path.Combine(DateTime.UtcNow.ToString("yyyy"), DateTime.UtcNow.ToString("MM"));

        var storageRootPath = GetStorageRootPath();
        var folderPath = Path.Combine(storageRootPath, normalizedFolder, datedPath);
        Directory.CreateDirectory(folderPath);

        var fileName = $"{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}-{RandomNumberGenerator.GetHexString(10).ToLowerInvariant()}{extension}";
        var physicalPath = Path.Combine(folderPath, fileName);

        await using var stream = new FileStream(physicalPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        await file.CopyToAsync(stream, cancellationToken);

        var relativePublicPath = $"/{_options.PublicPathPrefix.Trim('/').Trim()}" +
                                 $"/{normalizedFolder}/{datedPath.Replace('\\', '/')}/{fileName}";

        return BuildAbsoluteUrl(relativePublicPath);
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
            localPath = absoluteUri.AbsolutePath;
        }

        localPath = localPath.Replace('\\', '/').Trim();
        if (!localPath.StartsWith('/'))
        {
            localPath = "/" + localPath;
        }

        var publicPrefix = "/" + _options.PublicPathPrefix.Trim('/').Trim();
        if (!localPath.StartsWith(publicPrefix + "/", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(false);
        }

        var relativeStoragePath = localPath[(publicPrefix.Length + 1)..];
        var physicalPath = Path.Combine(GetStorageRootPath(), relativeStoragePath.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(physicalPath))
        {
            return Task.FromResult(false);
        }

        File.Delete(physicalPath);
        return Task.FromResult(true);
    }

    private string GetStorageRootPath()
    {
        var configured = _options.StorageRootPath?.Trim();
        if (string.IsNullOrWhiteSpace(configured))
        {
            configured = "storage";
        }

        return Path.IsPathRooted(configured)
            ? configured
            : Path.Combine(_environment.ContentRootPath, configured);
    }

    private string BuildAbsoluteUrl(string relativePath)
    {
        if (!string.IsNullOrWhiteSpace(_options.PublicBaseUrl))
        {
            return new Uri(new Uri(_options.PublicBaseUrl!.TrimEnd('/') + "/"), relativePath.TrimStart('/')).ToString();
        }

        var request = _httpContextAccessor.HttpContext?.Request;
        if (request is not null)
        {
            return $"{request.Scheme}://{request.Host}{relativePath}";
        }

        return relativePath;
    }

    private static string NormalizeFolder(string folder)
    {
        if (string.IsNullOrWhiteSpace(folder))
        {
            return "general";
        }

        return new string(folder
            .Trim()
            .ToLowerInvariant()
            .Select(ch => char.IsLetterOrDigit(ch) || ch is '-' or '_' ? ch : '-')
            .ToArray())
            .Trim('-');
    }
}

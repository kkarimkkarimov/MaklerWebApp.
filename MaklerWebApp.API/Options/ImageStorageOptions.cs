namespace MaklerWebApp.API.Options;

public class ImageStorageOptions
{
    public const string SectionName = "ImageStorage";

    public string StorageRootPath { get; set; } = "storage";
    public string PublicPathPrefix { get; set; } = "media";
    public string? PublicBaseUrl { get; set; }
    public int MaxFileSizeMb { get; set; } = 10;
}

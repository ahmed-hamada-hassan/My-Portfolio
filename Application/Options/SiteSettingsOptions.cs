namespace Application.Options;

public sealed class SiteSettingsOptions
{
    public const string SectionName = "SiteSettings";
    public DocumentUploadOptions CV { get; set; } = new();
    public ImageUploadOptions Images { get; set; } = new();
}

public sealed class DocumentUploadOptions
{
    public long MaxFileSizeInBytes { get; set; }

    public string[] AllowedContentTypes { get; set; } = [];
}

public sealed class ImageUploadOptions
{
    public long MaxFileSizeInBytes { get; set; }
    public string[] AllowedContentTypes { get; set; } = [];
    public string[] AllowedExtensions { get; set; } = [];
}
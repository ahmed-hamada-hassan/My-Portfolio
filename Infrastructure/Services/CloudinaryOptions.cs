namespace Infrastructure.Services;

public sealed class CloudinaryOptions
{
    public const string SectionName = "Cloudinary";

    public string ApiSecret { get; set; } = null!;
    public string CloudName { get; set; } = null!;
    public string ApiKey { get; set; } = null!;
}
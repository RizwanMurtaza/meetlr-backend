namespace Meetlr.Infrastructure.Services.FileStorage;

public class FileStorageSettings
{
    public const string SectionName = "FileStorage";

    /// <summary>
    /// Storage provider: "S3", "Local", or "Auto" (tries S3 first, falls back to Local)
    /// </summary>
    public string Provider { get; set; } = "Auto";

    /// <summary>
    /// Maximum file size in bytes (default: 10MB)
    /// </summary>
    public long MaxFileSizeBytes { get; set; } = 10 * 1024 * 1024;

    /// <summary>
    /// Allowed file extensions (empty = all allowed)
    /// </summary>
    public string[] AllowedExtensions { get; set; } = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg", ".ico" };

    /// <summary>
    /// S3 settings
    /// </summary>
    public S3Settings S3 { get; set; } = new();

    /// <summary>
    /// Local storage settings
    /// </summary>
    public LocalStorageSettings Local { get; set; } = new();
}

public class S3Settings
{
    /// <summary>
    /// AWS Access Key ID
    /// </summary>
    public string? AccessKeyId { get; set; }

    /// <summary>
    /// AWS Secret Access Key
    /// </summary>
    public string? SecretAccessKey { get; set; }

    /// <summary>
    /// S3 Bucket name
    /// </summary>
    public string? BucketName { get; set; }

    /// <summary>
    /// AWS Region (e.g., us-east-1)
    /// </summary>
    public string Region { get; set; } = "us-east-1";

    /// <summary>
    /// Custom endpoint URL for S3-compatible services (MinIO, DigitalOcean Spaces, etc.)
    /// </summary>
    public string? ServiceUrl { get; set; }

    /// <summary>
    /// Use path-style addressing (required for some S3-compatible services)
    /// </summary>
    public bool ForcePathStyle { get; set; } = false;

    /// <summary>
    /// CDN URL prefix for serving files (optional)
    /// </summary>
    public string? CdnUrl { get; set; }

    /// <summary>
    /// Whether S3 is properly configured
    /// </summary>
    public bool IsConfigured =>
        !string.IsNullOrEmpty(AccessKeyId) &&
        !string.IsNullOrEmpty(SecretAccessKey) &&
        !string.IsNullOrEmpty(BucketName);
}

public class LocalStorageSettings
{
    /// <summary>
    /// Root path for storing files (relative to wwwroot or absolute)
    /// </summary>
    public string StoragePath { get; set; } = "uploads";

    /// <summary>
    /// Base URL for serving files
    /// </summary>
    public string? BaseUrl { get; set; }
}

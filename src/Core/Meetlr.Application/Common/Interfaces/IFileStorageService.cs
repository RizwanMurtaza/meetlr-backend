namespace Meetlr.Application.Common.Interfaces;

public interface IFileStorageService
{
    /// <summary>
    /// Uploads a file and returns the public URL
    /// </summary>
    Task<FileUploadResult> UploadAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        string folder = "uploads",
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a file by its URL or key
    /// </summary>
    Task<bool> DeleteAsync(string fileUrl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a file exists
    /// </summary>
    Task<bool> ExistsAsync(string fileUrl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the storage provider name (S3, Local, etc.)
    /// </summary>
    string ProviderName { get; }
}

public record FileUploadResult
{
    public bool Success { get; init; }
    public string? Url { get; init; }
    public string? Key { get; init; }
    public string? Error { get; init; }
    public long FileSize { get; init; }
    public string? ContentType { get; init; }

    public static FileUploadResult Succeeded(string url, string key, long fileSize, string contentType) =>
        new() { Success = true, Url = url, Key = key, FileSize = fileSize, ContentType = contentType };

    public static FileUploadResult Failed(string error) =>
        new() { Success = false, Error = error };
}

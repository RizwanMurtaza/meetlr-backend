using Meetlr.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Meetlr.Infrastructure.Services.FileStorage;

public class LocalFileStorageService : IFileStorageService
{
    private readonly FileStorageSettings _settings;
    private readonly ILogger<LocalFileStorageService> _logger;
    private readonly string _storagePath;
    private readonly string _baseUrl;

    public string ProviderName => "Local";

    public LocalFileStorageService(
        IOptions<FileStorageSettings> settings,
        ILogger<LocalFileStorageService> logger,
        IWebHostEnvironmentAccessor environmentAccessor)
    {
        _settings = settings.Value;
        _logger = logger;

        // Determine storage path
        var localSettings = _settings.Local;
        if (Path.IsPathRooted(localSettings.StoragePath))
        {
            _storagePath = localSettings.StoragePath;
        }
        else
        {
            _storagePath = Path.Combine(environmentAccessor.WebRootPath ?? environmentAccessor.ContentRootPath, localSettings.StoragePath);
        }

        // Ensure storage directory exists
        if (!Directory.Exists(_storagePath))
        {
            Directory.CreateDirectory(_storagePath);
        }

        // Determine base URL
        _baseUrl = localSettings.BaseUrl?.TrimEnd('/') ?? $"/{localSettings.StoragePath}";
    }

    public async Task<FileUploadResult> UploadAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        string folder = "uploads",
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate file size
            if (fileStream.Length > _settings.MaxFileSizeBytes)
            {
                return FileUploadResult.Failed($"File size exceeds maximum allowed size of {_settings.MaxFileSizeBytes / 1024 / 1024}MB");
            }

            // Validate extension
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            if (_settings.AllowedExtensions.Length > 0 && !_settings.AllowedExtensions.Contains(extension))
            {
                return FileUploadResult.Failed($"File extension '{extension}' is not allowed");
            }

            // Generate unique file name
            var uniqueFileName = $"{Guid.NewGuid():N}{extension}";
            var folderPath = Path.Combine(_storagePath, folder);
            var filePath = Path.Combine(folderPath, uniqueFileName);

            // Ensure folder exists
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // Save file
            await using var fileStreamOut = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            await fileStream.CopyToAsync(fileStreamOut, cancellationToken);

            // Generate URL
            var key = $"{folder}/{uniqueFileName}";
            var url = $"{_baseUrl}/{key}";

            _logger.LogInformation("File uploaded locally: {FilePath}", filePath);

            return FileUploadResult.Succeeded(url, key, fileStream.Length, contentType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload file locally: {FileName}", fileName);
            return FileUploadResult.Failed($"Failed to upload file: {ex.Message}");
        }
    }

    public Task<bool> DeleteAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            // Extract file path from URL
            var relativePath = fileUrl.Replace(_baseUrl, "").TrimStart('/');
            var filePath = Path.Combine(_storagePath, relativePath);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _logger.LogInformation("File deleted locally: {FilePath}", filePath);
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete file locally: {FileUrl}", fileUrl);
            return Task.FromResult(false);
        }
    }

    public Task<bool> ExistsAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            var relativePath = fileUrl.Replace(_baseUrl, "").TrimStart('/');
            var filePath = Path.Combine(_storagePath, relativePath);
            return Task.FromResult(File.Exists(filePath));
        }
        catch
        {
            return Task.FromResult(false);
        }
    }
}

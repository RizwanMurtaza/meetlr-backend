using Meetlr.Api.Endpoints.Common;
using Meetlr.Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Api.Endpoints.Files.Upload;

[Route("api/files")]
public class UploadFile : BaseAuthenticatedEndpoint
{
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<UploadFile> _logger;

    public UploadFile(
        IFileStorageService fileStorageService,
        ILogger<UploadFile> logger)
    {
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    /// <summary>
    /// Upload a file (image) for use in homepage or profile.
    /// </summary>
    /// <param name="file">The file to upload</param>
    /// <param name="folder">Optional folder name (default: "uploads")</param>
    /// <returns>The uploaded file URL and metadata</returns>
    [HttpPost("upload")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(UploadFileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(UploadFileResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10MB limit
    public async Task<IActionResult> Handle(
        IFormFile file,
        [FromQuery] string folder = "uploads")
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new UploadFileResponse
            {
                Success = false,
                ErrorMessage = "No file provided"
            });
        }

        try
        {
            await using var stream = file.OpenReadStream();
            var result = await _fileStorageService.UploadAsync(
                stream,
                file.FileName,
                file.ContentType,
                folder);

            if (!result.Success)
            {
                return BadRequest(new UploadFileResponse
                {
                    Success = false,
                    ErrorMessage = result.Error
                });
            }

            _logger.LogInformation(
                "File uploaded by user {UserId}: {Url}",
                CurrentUserId,
                result.Url);

            return Ok(new UploadFileResponse
            {
                Success = true,
                Url = result.Url,
                Key = result.Key,
                Size = result.FileSize,
                ContentType = result.ContentType,
                Provider = _fileStorageService.ProviderName
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file for user {UserId}", CurrentUserId);
            return BadRequest(new UploadFileResponse
            {
                Success = false,
                ErrorMessage = "An error occurred while uploading the file"
            });
        }
    }

    /// <summary>
    /// Delete a previously uploaded file.
    /// </summary>
    /// <param name="request">The file URL to delete</param>
    [HttpDelete]
    [Produces("application/json")]
    [ProducesResponseType(typeof(DeleteFileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete([FromBody] DeleteFileRequest request)
    {
        if (string.IsNullOrEmpty(request.FileUrl))
        {
            return BadRequest(new DeleteFileResponse { Success = false, Message = "File URL is required" });
        }

        var deleted = await _fileStorageService.DeleteAsync(request.FileUrl);

        if (deleted)
        {
            _logger.LogInformation(
                "File deleted by user {UserId}: {Url}",
                CurrentUserId,
                request.FileUrl);
        }

        return Ok(new DeleteFileResponse
        {
            Success = deleted,
            Message = deleted ? "File deleted successfully" : "File not found or could not be deleted"
        });
    }
}

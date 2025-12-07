namespace Meetlr.Api.Endpoints.Files.Upload;

public class UploadFileResponse
{
    public bool Success { get; set; }
    public string? Url { get; set; }
    public string? Key { get; set; }
    public long Size { get; set; }
    public string? ContentType { get; set; }
    public string? Provider { get; set; }
    public string? ErrorMessage { get; set; }
}

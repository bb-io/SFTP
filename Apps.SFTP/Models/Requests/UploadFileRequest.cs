using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.SFTP.Models.Requests;

public class UploadFileRequest
{
    public FileReference File { get; set; }

    public string? FileName { get; set; }

    public string Path { get; set; }
}
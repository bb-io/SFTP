using File = Blackbird.Applications.Sdk.Common.Files.File;

namespace Apps.SFTP.Models.Requests;

public class UploadFileRequest
{
    public File File { get; set; }

    public string? FileName { get; set; }

    public string Path { get; set; }
}
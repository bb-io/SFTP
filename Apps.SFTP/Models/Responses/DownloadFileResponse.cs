using File = Blackbird.Applications.Sdk.Common.Files.File;

namespace Apps.SFTP.Models.Responses;

public class DownloadFileResponse
{
    public File File { get; set; }
}
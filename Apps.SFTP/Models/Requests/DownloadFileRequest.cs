using Blackbird.Applications.Sdk.Common;

namespace Apps.SFTP.Models.Requests;

public class DownloadFileRequest
{
    [Display("Full path")]
    public string Path { get; set; }
}
using Blackbird.Applications.Sdk.Common;

namespace Apps.SFTP.Models.Requests;

public class DeleteFileRequest
{
    [Display("Path")]
    public string FilePath { get; set; }
}
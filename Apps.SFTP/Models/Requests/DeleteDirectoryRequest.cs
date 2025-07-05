
using Blackbird.Applications.Sdk.Common;

namespace Apps.SFTP.Models.Requests;

public class DeleteDirectoryRequest
{
    [Display("Folder path")]
    public string Path { get; set; }
}
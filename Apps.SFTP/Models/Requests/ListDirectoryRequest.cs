
using Blackbird.Applications.Sdk.Common;

namespace Apps.SFTP.Models.Requests;

public class ListDirectoryRequest
{
    [Display("Folder path")]
    public string Path { get; set; }

    [Display("Updated from")]
    public DateTime? UpdatedFrom { get; set; }

    [Display("Updated to")]
    public DateTime? UpdatedTo { get; set; }
}
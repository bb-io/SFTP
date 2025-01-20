
using Blackbird.Applications.Sdk.Common;

namespace Apps.SFTP.Models.Requests;

public class DeleteDirectoryRequest
{
    [Display("Directory path")]
    public string Path { get; set; }
}
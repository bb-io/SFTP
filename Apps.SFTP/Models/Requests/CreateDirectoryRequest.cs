
using Blackbird.Applications.Sdk.Common;

namespace Apps.SFTP.Models.Requests;

public class CreateDirectoryRequest
{
    [Display("Name")]
    public string DirectoryName { get; set; }

    public string Path { get; set; }
}
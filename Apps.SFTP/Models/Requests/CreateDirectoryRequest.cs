
using Blackbird.Applications.Sdk.Common;

namespace Apps.SFTP.Models.Requests;

public class CreateDirectoryRequest
{
    [Display("Directory name")]
    public string DirectoryName { get; set; }

    [Display("Parent directory path", Description = "The path, '/' being the root directory (default).")]
    public string? Path { get; set; }
}
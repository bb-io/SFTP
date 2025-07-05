
using Blackbird.Applications.Sdk.Common;

namespace Apps.SFTP.Models.Requests;

public class CreateDirectoryRequest
{
    [Display("Folder name")]
    public string DirectoryName { get; set; }

    [Display("Parent folder path", Description = "The path, '/' being the root folder (default).")]
    public string? Path { get; set; }
}
using Blackbird.Applications.Sdk.Common;

namespace Apps.SFTP.Models.Requests;

public class RenameFileRequest
{
    [Display("Old path")]
    public string OldPath { get; set; }

    [Display("New path")]
    public string NewPath { get; set; }
}
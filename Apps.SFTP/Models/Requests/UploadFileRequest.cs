using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.SDK.Blueprints.Interfaces.FileStorage;

namespace Apps.SFTP.Models.Requests;

public class UploadFileRequest : IUploadFileInput
{
    public FileReference File { get; set; }

    [Display("File name")]
    public string? FileName { get; set; }

    [Display("Directory path")]
    public string? Path { get; set; }
}
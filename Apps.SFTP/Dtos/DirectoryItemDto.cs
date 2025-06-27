using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.SDK.Blueprints.Interfaces.FileStorage;

namespace Apps.SFTP.Dtos;

public class DirectoryItemDto : IDownloadFileInput
{
    public string Name { get; set; }

    [Display("File path")]
    public string FileId { get; set; }
}
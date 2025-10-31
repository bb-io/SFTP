using Apps.SFTP.DataHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.SDK.Blueprints.Interfaces.FileStorage;
using Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems;

namespace Apps.SFTP.Models.Requests;

public class UploadFileRequest : IUploadFileInput
{
    public FileReference File { get; set; }

    [Display("File name")]
    public string? FileName { get; set; }

    [Display("Folder path")]
    [FileDataSource(typeof(FolderDataHandler))]
    public string? Path { get; set; }
}
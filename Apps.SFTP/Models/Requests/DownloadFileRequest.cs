using Apps.SFTP.DataHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.SDK.Blueprints.Interfaces.FileStorage;
using Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems;

namespace Apps.SFTP.Models.Requests;

public class DownloadFileRequest : IDownloadFileInput
{
    [Display("File path")]
    [FileDataSource(typeof(FileDataHandler))]
    public string FileId { get; set; }
}
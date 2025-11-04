using Apps.SFTP.DataHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems;

namespace Apps.SFTP.Models.Requests;

public class DeleteFileRequest
{
    [Display("File path")]
    [FileDataSource(typeof(FileDataHandler))]
    public string FilePath { get; set; }
}
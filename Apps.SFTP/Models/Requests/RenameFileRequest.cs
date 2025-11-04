using Apps.SFTP.DataHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems;

namespace Apps.SFTP.Models.Requests;

public class RenameFileRequest
{
    [Display("File path")]
    [FileDataSource(typeof(FileDataHandler))]
    public string OldPath { get; set; }

    [Display("New file name")]
    public string NewFileName { get; set; }
}
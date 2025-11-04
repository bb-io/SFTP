
using Apps.SFTP.DataHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems;

namespace Apps.SFTP.Models.Requests;

public class DeleteDirectoryRequest
{
    [Display("Folder path")]
    [FileDataSource(typeof(FolderDataHandler))]
    public string FolderPath { get; set; }
}
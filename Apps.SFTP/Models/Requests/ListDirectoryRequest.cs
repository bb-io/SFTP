
using Apps.SFTP.DataHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems;

namespace Apps.SFTP.Models.Requests;

public class ListDirectoryRequest
{
    [Display("Folder path")]
    [FileDataSource(typeof(FolderDataHandler))]
    public string FolderPath { get; set; }

    [Display("Updated from")]
    public DateTime? UpdatedFrom { get; set; }

    [Display("Updated to")]
    public DateTime? UpdatedTo { get; set; }
}
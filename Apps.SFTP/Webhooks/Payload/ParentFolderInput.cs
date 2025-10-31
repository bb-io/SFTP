
using Apps.SFTP.DataHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems;

namespace Apps.SFTP.Webhooks.Payload
{
    public class ParentFolderInput
    {
        [Display("Folder path")]
        [FileDataSource(typeof(FolderDataHandler))]
        public string? Folder { get; set; }

        [Display("Include subfolders")]
        public bool? IncludeSubfolders { get; set; }
    }
}

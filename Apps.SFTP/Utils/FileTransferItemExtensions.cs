using Apps.SFTP.Models;
using Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems;
using File = Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems.File;

namespace Apps.SFTP.Utils;

public static class FileTransferItemExtensions
{
    public static Folder ToFolderObject(this FileTransferItem item, bool isSelectable = false)
    {
        return new Folder
        {
            Id = item.FullName,
            Date = item.LastModified,
            DisplayName = item.Name,
            IsSelectable = isSelectable,
        };
    }
    
    public static File ToFileObject(this FileTransferItem item, bool isSelectable = false)
    {
        return new File
        {
            Id = item.FullName,
            Date = item.LastModified,
            DisplayName = item.Name,
            IsSelectable = isSelectable,
            Size = item.Size ?? 0,
        };
    }
}
using Apps.SFTP.Invocables;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems;

namespace Apps.SFTP.DataHandlers;

public class FolderDataHandler(InvocationContext invocationContext) : FileTransferInvocable(invocationContext), IFileDataSourceItemHandler
{
    public IEnumerable<FileDataItem> GetFolderContent(FolderContentDataSourceContext context)
    {
        var path = string.IsNullOrEmpty(context.FolderId) ? "/" : context.FolderId;
        return UseClient(client => client.ListDirectoryAsync(path).GetAwaiter().GetResult())
            .Where(x => x.IsDirectory)
            .Where(x => x.Name.Any(y => y != '.'))
            .Select(x => new Folder
            {
                Id = x.FullName,
                Date = x.LastModified,
                DisplayName = x.Name,
                IsSelectable = true
            })
            .ToList<FileDataItem>();
    }

    public IEnumerable<FolderPathItem> GetFolderPath(FolderPathDataSourceContext context)
    {
        var folderPaths = new List<FolderPathItem> { new() { Id = "/", DisplayName = "/" } };

        var directoryPath = Path.GetDirectoryName(context.FileDataItemId ?? "/");
        if (string.IsNullOrEmpty(directoryPath))
        {
            return folderPaths;
        }

        var parts = directoryPath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        var currentPath = string.Empty;
        foreach (var part in parts)
        {
            if (string.IsNullOrWhiteSpace(part))
            {
                continue;
            }

            currentPath = string.IsNullOrEmpty(currentPath) ? part : $"{currentPath}/{part}";
            folderPaths.Add(new FolderPathItem { Id = currentPath, DisplayName = currentPath + "/" });
        }

        return folderPaths;
    }
}

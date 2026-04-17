using Apps.SFTP.Api;
using Apps.SFTP.Invocables;
using Apps.SFTP.Models;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems;
using File = Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems.File;

namespace Apps.SFTP.DataHandlers;

public class FileDataHandler(InvocationContext invocationContext) : FileTransferInvocable(invocationContext), IFileDataSourceItemHandler
{
    public IEnumerable<FileDataItem> GetFolderContent(FolderContentDataSourceContext context)
    {
        var path = string.IsNullOrEmpty(context.FolderId) ? "/" : context.FolderId;
        using var client = FileTransferClientFactory.Create(Creds);
        client.ConnectAsync().GetAwaiter().GetResult();

        return client.ExecuteAsync(() => Task.FromResult(GetFolderContentInternal(client, path)))
            .GetAwaiter()
            .GetResult();
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

    private static IEnumerable<FileDataItem> GetFolderContentInternal(FileTransferClient client, string path) =>
        client.ListDirectoryAsync(path).GetAwaiter().GetResult()
            .Where(x => x.Name.Any(y => y != '.'))
            .Where(x => x.IsDirectory || x.IsFile)
            .Select(Convert)
            .ToList<FileDataItem>();

    private static FileDataItem Convert(FileTransferItem file)
    {
        if (file.IsDirectory)
        {
            return new Folder
            {
                Id = file.FullName,
                Date = file.LastModified,
                DisplayName = file.Name,
                IsSelectable = false
            };
        }

        return new File
        {
            Id = file.FullName,
            Date = file.LastModified,
            DisplayName = file.Name,
            IsSelectable = true,
            Size = file.Size ?? 0
        };
    }
}

using Apps.SFTP.Api;
using Apps.SFTP.Invocables;
using Apps.SFTP.Models;
using Apps.SFTP.Utils;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems;
using File = Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems.File;

namespace Apps.SFTP.DataHandlers;

public class FileDataHandler(InvocationContext invocationContext)
    : FileTransferInvocable(invocationContext), IAsyncFileDataSourceItemHandler
{
    public async Task<IEnumerable<FileDataItem>> GetFolderContentAsync(FolderContentDataSourceContext context, CancellationToken cancellationToken)
    {
        var path = string.IsNullOrEmpty(context.FolderId) ? "/" : context.FolderId;
        using var client = FileTransferClientFactory.Create(Creds);
        await client.ConnectAsync(cancellationToken);

        return await client.ExecuteAsync(() => GetFolderContentInternal(client, path));
    }

    public Task<IEnumerable<FolderPathItem>> GetFolderPathAsync(FolderPathDataSourceContext context, CancellationToken cancellationToken)
    {
        var folderPaths = new List<FolderPathItem> { new() { Id = "/", DisplayName = "/" } };

        var directoryPath = Path.GetDirectoryName(context.FileDataItemId ?? "/");
        if (string.IsNullOrEmpty(directoryPath))
        {
            return Task.FromResult<IEnumerable<FolderPathItem>>(folderPaths);
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

        return Task.FromResult<IEnumerable<FolderPathItem>>(folderPaths);
    }

    private static async Task<IEnumerable<FileDataItem>> GetFolderContentInternal(FileTransferClient client, string path)
    {
        var items = await client.ListDirectoryAsync(path);
        return items
            .Where(x => x.Name.Any(y => y != '.'))
            .Where(x => x.IsDirectory || x.IsFile)
            .Select(Convert)
            .ToList<FileDataItem>();
    }

    private static FileDataItem Convert(FileTransferItem file)
    {
        if (file.IsDirectory)
        {
            return file.ToFolderObject(false);
        }

        return file.ToFileObject(true);
    }
}
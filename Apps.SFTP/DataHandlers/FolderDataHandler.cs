using Apps.SFTP.Api;
using Apps.SFTP.Invocables;
using Apps.SFTP.Utils;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems;

namespace Apps.SFTP.DataHandlers;

public class FolderDataHandler(InvocationContext invocationContext) : FileTransferInvocable(invocationContext), IAsyncFileDataSourceItemHandler
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
            .Where(x => x.IsDirectory)
            .Where(x => x.Name.Any(y => y != '.'))
            .Select(x => x.ToFolderObject(true))
            .ToList<FileDataItem>();
    }
}
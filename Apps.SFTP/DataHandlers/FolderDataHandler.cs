using Apps.SFTP.Invocables;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems;

namespace Apps.SFTP.DataHandlers;
public class FolderDataHandler(InvocationContext invocationContext) : SFTPInvocable(invocationContext), IFileDataSourceItemHandler
{
    public IEnumerable<FileDataItem> GetFolderContent(FolderContentDataSourceContext context)
    {
        return UseClient(client => client.ListDirectory(context.FolderId ?? "/"))
            .Where(x => x.IsDirectory)
            .Where(x => !x.Name.All(y => y == '.'))
            .Select(x => new Folder { Id = x.FullName, Date = x.LastWriteTime, DisplayName = x.Name, IsSelectable = true })
            .ToList<FileDataItem>();
    }

    public IEnumerable<FolderPathItem> GetFolderPath(FolderPathDataSourceContext context)
    {
        var folderPaths = new List<FolderPathItem>() { new FolderPathItem { Id = "/", DisplayName = "/"} };

        var directoryPath = Path.GetDirectoryName(context.FileDataItemId ?? "/");
        if (string.IsNullOrEmpty(directoryPath))
            return folderPaths;

        var parts = directoryPath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        string currentPath = "";
        foreach (var part in parts)
        {
            if (string.IsNullOrWhiteSpace(part)) continue;

            currentPath = string.IsNullOrEmpty(currentPath)
                ? part
                : Path.Combine(currentPath, part);

            folderPaths.Add(new FolderPathItem { Id = currentPath, DisplayName = currentPath + "/" });
        }

        return folderPaths;
    }
}

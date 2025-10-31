using Apps.SFTP.Invocables;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems;
using Renci.SshNet.Sftp;
using System.Web;
using File = Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems.File;

namespace Apps.SFTP.DataHandlers;
public class FileDataHandler(InvocationContext invocationContext) : SFTPInvocable(invocationContext), IFileDataSourceItemHandler
{
    public IEnumerable<FileDataItem> GetFolderContent(FolderContentDataSourceContext context)
    {
        var path = string.IsNullOrEmpty(context.FolderId) ? "/" : HttpUtility.HtmlDecode(context.FolderId);
        return UseClient(client => client.ListDirectory(path))
            .Where(x => !x.Name.All(y => y == '.'))
            .Where(x => x.IsDirectory || x.IsRegularFile)
            .Select(Convert)
            .Where(x => x is not null)
            .ToList<FileDataItem>();
    }

    private FileDataItem Convert(ISftpFile file)
    {
        if (file.IsDirectory)
        {
            return new Folder { Id = file.FullName, Date = file.LastWriteTime, DisplayName = file.Name, IsSelectable = false };
        }

        return new File { Id = file.FullName, Date = file.LastWriteTime, DisplayName = file.Name, IsSelectable = true, Size = file.Attributes.Size };
    }

    public IEnumerable<FolderPathItem> GetFolderPath(FolderPathDataSourceContext context)
    {
        var folderPaths = new List<FolderPathItem>() { new FolderPathItem { Id = HttpUtility.HtmlEncode("/"), DisplayName = "/"} };

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

using Apps.SFTP.Api;
using Apps.SFTP.Dtos;
using Apps.SFTP.Invocables;
using Apps.SFTP.Models.Requests;
using Apps.SFTP.Models.Responses;
using Blackbird.Applications.SDK.Blueprints;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Renci.SshNet.Common;
using RestSharp;

namespace Apps.SFTP;

[ActionList("Files")]
public class Actions(InvocationContext context, IFileManagementClient fileManagementClient)
    : FileTransferInvocable(context)
{
    [Action("Search files", Description = "Search all files in specified folder")]
    public async Task<ListDirectoryResponse> ListDirectory([ActionParameter] ListDirectoryRequest input)
    {
        var folderPath = string.IsNullOrWhiteSpace(input.FolderPath) ? "/" : input.FolderPath;
        using var client = FileTransferClientFactory.Create(Creds);
        await client.ConnectAsync();

        return await client.ExecuteAsync(() => ListDirectoryInternal(client, folderPath, input));
    }

    [Action("Rename file", Description = "Rename a path from old to new")]
    public async Task RenameFile([ActionParameter] RenameFileRequest input)
    {
        var newFullPath = BuildRenamedPath(input.OldPath, input.NewFileName);
        using var client = FileTransferClientFactory.Create(Creds);
        await client.ConnectAsync();
        var targetDirectory = GetDirectoryPath(newFullPath);
        await EnsureDirectoryExistsAsync(client, targetDirectory);
        await client.ExecuteAsync(() => client.RenameAsync(input.OldPath, newFullPath));
    }

    [BlueprintActionDefinition(BlueprintAction.DownloadFile)]
    [Action("Download file", Description = "Download file by path")]
    public async Task<DownloadFileResponse> DownloadFile([ActionParameter] DownloadFileRequest input)
    {
        using var client = FileTransferClientFactory.Create(Creds);
        await client.ConnectAsync();

        return await client.ExecuteAsync(async () =>
        {
            var fileName = Path.GetFileName(input.FileId);
            var mimeType = MimeTypes.GetMimeType(fileName);

            using var memoryStream = new MemoryStream();
            await client.DownloadAsync(input.FileId, memoryStream);
            if (memoryStream.Length == 0)
            {
                throw new PluginMisconfigurationException("The file cannot be found.");
            }

            memoryStream.Seek(0, SeekOrigin.Begin);
            var file = await fileManagementClient.UploadAsync(memoryStream, mimeType, fileName);
            return new DownloadFileResponse { File = file };
        });
    }

    [BlueprintActionDefinition(BlueprintAction.UploadFile)]
    [Action("Upload file", Description = "Upload file by path")]
    public async Task UploadFile([ActionParameter] UploadFileRequest input)
    {
        using var client = FileTransferClientFactory.Create(Creds);
        await client.ConnectAsync();

        await client.ExecuteAsync(async () =>
        {
            using var memoryStream = new MemoryStream();

            if (input.File.Url == null)
            {
                var fileStream = await fileManagementClient.DownloadAsync(input.File);
                await fileStream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;
            }
            else
            {
                var restClient = new RestClient(input.File.Url);
                using var responseStream = restClient.DownloadStream(new RestRequest());
                if (responseStream == null)
                {
                    throw new PluginApplicationException("Failed to download the file from the provided URL.");
                }

                await responseStream.CopyToAsync(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);
            }

            var fileName = input.FileName ?? input.File.Name;
            var path = input.Path ?? "/";
            await client.UploadAsync(memoryStream, $"{path.TrimEnd('/')}/{fileName}");
        });
    }

    [Action("Delete file", Description = "Delete file by path")]
    public async Task DeleteFile([ActionParameter] DeleteFileRequest input)
    {
        if (string.IsNullOrWhiteSpace(input.FilePath))
        {
            throw new PluginMisconfigurationException("Please enter a valid path.");
        }

        using var client = FileTransferClientFactory.Create(Creds);
        await client.ConnectAsync();
        await client.ExecuteAsync(() => client.DeleteFileAsync(input.FilePath));
    }

    private static async Task<ListDirectoryResponse> ListDirectoryInternal(
        FileTransferClient client,
        string folderPath,
        ListDirectoryRequest input)
    {
        var filesQuery = (await client.ListDirectoryAsync(folderPath))
            .Where(x => x.IsFile);

        if (input.UpdatedFrom.HasValue)
        {
            filesQuery = filesQuery.Where(x => x.LastModified >= input.UpdatedFrom.Value);
        }

        if (input.UpdatedTo.HasValue)
        {
            filesQuery = filesQuery.Where(x => x.LastModified <= input.UpdatedTo.Value);
        }

        var files = filesQuery.Select(item => new DirectoryItemDto
        {
            Name = item.Name,
            FileId = item.FullName,
        }).ToList();

        return new ListDirectoryResponse
        {
            DirectoriesItems = files,
            ItemNames = files.Select(x => x.Name)
        };
    }

    private static string BuildRenamedPath(string oldPath, string newFileName)
    {
        var normalizedNewFileName = newFileName.Replace('\\', '/');
        if (normalizedNewFileName.StartsWith('/'))
            return normalizedNewFileName;

        var normalizedOldPath = oldPath.Replace('\\', '/');
        var lastSlashIndex = normalizedOldPath.LastIndexOf('/');

        return lastSlashIndex switch
        {
            < 0 => normalizedNewFileName,
            0 => $"/{normalizedNewFileName}",
            _ => $"{normalizedOldPath[..lastSlashIndex]}/{normalizedNewFileName}"
        };
    }

    private static string? GetDirectoryPath(string fullPath)
    {
        var normalized = fullPath.Replace('\\', '/').TrimEnd('/');
        var lastSlashIndex = normalized.LastIndexOf('/');

        return lastSlashIndex switch
        {
            < 0 => null,
            0 => "/",
            _ => normalized[..lastSlashIndex]
        };
    }

    private static async Task EnsureDirectoryExistsAsync(FileTransferClient client, string? directoryPath)
    {
        if (string.IsNullOrWhiteSpace(directoryPath) || directoryPath == "/")
        {
            return;
        }

        var normalized = directoryPath.Replace('\\', '/').TrimEnd('/');
        if (string.IsNullOrWhiteSpace(normalized) || normalized == "/")
        {
            return;
        }

        var isAbsolute = normalized.StartsWith('/');
        var parts = normalized.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var currentPath = isAbsolute ? "/" : string.Empty;

        foreach (var part in parts)
        {
            currentPath = string.IsNullOrEmpty(currentPath) || currentPath == "/"
                ? $"{currentPath}{part}"
                : $"{currentPath}/{part}";

            if (await DirectoryExistsAsync(client, currentPath))
            {
                continue;
            }

            await client.ExecuteAsync(() => client.CreateDirectoryAsync(currentPath));
        }
    }

    private static async Task<bool> DirectoryExistsAsync(FileTransferClient client, string path)
    {
        try
        {
            var info = await client.ExecuteAsync(() => client.GetFileInfoAsync(path));
            if (!info.IsDirectory)
            {
                throw new PluginMisconfigurationException($"Path exists and is not a directory: {path}");
            }

            return true;
        }
        catch (PluginMisconfigurationException ex)
            when (ex.Message.StartsWith("File or path not found:", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }
        catch (SftpPathNotFoundException)
        {
            return false;
        }
        catch (AggregateException ex) when (ex.GetBaseException() is SftpPathNotFoundException)
        {
            return false;
        }
    }
}

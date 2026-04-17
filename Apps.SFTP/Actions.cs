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
using RestSharp;

namespace Apps.SFTP;

[ActionList("Files")]
public class Actions(InvocationContext context, IFileManagementClient fileManagementClient)
    : FileTransferInvocable(context)
{
    [Action("Search files", Description = "Search all files in specified folder")]
    public ListDirectoryResponse ListDirectory([ActionParameter] ListDirectoryRequest input)
    {
        var folderPath = string.IsNullOrWhiteSpace(input.FolderPath) ? "/" : input.FolderPath;

        return UseClient(client =>
        {
            var filesQuery = client.ListDirectoryAsync(folderPath).GetAwaiter().GetResult()
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
        });
    }

    [Action("Rename file", Description = "Rename a path from old to new")]
    public void RenameFile([ActionParameter] RenameFileRequest input)
    {
        var newFullPath = BuildRenamedPath(input.OldPath, input.NewFileName);

        UseClient(client =>
        {
            client.RenameAsync(input.OldPath, newFullPath).GetAwaiter().GetResult();
            return true;
        });
    }

    [BlueprintActionDefinition(BlueprintAction.DownloadFile)]
    [Action("Download file", Description = "Download file by path")]
    public async Task<DownloadFileResponse> DownloadFile([ActionParameter] DownloadFileRequest input)
    {
        return await UseClientAsync(async client =>
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
        await UseClientAsync(async client =>
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
    public void DeleteFile([ActionParameter] DeleteFileRequest input)
    {
        if (string.IsNullOrWhiteSpace(input.FilePath))
        {
            throw new PluginMisconfigurationException("Please enter a valid path.");
        }

        UseClient(client =>
        {
            client.DeleteFileAsync(input.FilePath).GetAwaiter().GetResult();
            return true;
        });
    }

    private static string BuildRenamedPath(string oldPath, string newFileName)
    {
        var normalizedOldPath = oldPath.Replace('\\', '/');
        var lastSlashIndex = normalizedOldPath.LastIndexOf('/');

        return lastSlashIndex switch
        {
            < 0 => newFileName,
            0 => $"/{newFileName}",
            _ => $"{normalizedOldPath[..lastSlashIndex]}/{newFileName}"
        };
    }
}

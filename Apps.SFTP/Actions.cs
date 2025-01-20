using System.Net.Mime;
using Blackbird.Applications.Sdk.Common;
using Apps.SFTP.Models.Requests;
using Apps.SFTP.Models.Responses;
using Apps.SFTP.Dtos;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Blackbird.Applications.Sdk.Common.Invocation;
using Apps.SFTP.Invocables;
using RestSharp;
using Blackbird.Applications.Sdk.Common.Exceptions;

namespace Apps.SFTP;

[ActionList]
public class Actions : SFTPInvocable
{
    private readonly IFileManagementClient _fileManagementClient;

    public Actions(InvocationContext context, IFileManagementClient fileManagementClient) : base(context)
    {
        _fileManagementClient = fileManagementClient;
    }

    [Action("List directory files", Description = "List all files in specified directory")]
    public ListDirectoryResponse ListDirectory([ActionParameter] ListDirectoryRequest input)
    {
        return UseClient(client =>
        {
            var files = client.ListDirectory(input.Path)
                .Where(x => x.IsRegularFile)
                .Select(i => new DirectoryItemDto()
                {
                    Name = i.Name,
                    Path = i.FullName,
                }).ToList();

            return new ListDirectoryResponse()
            {
                DirectoriesItems = files
            };
        });
    }

    [Action("Rename file", Description = "Rename a path from old to new")]
    public void RenameFile([ActionParameter] RenameFileRequest input)
    {
        UseClient(client =>
        {
            client.RenameFile(input.OldPath, input.NewPath);
            return true;
        });
    }

    [Action("Download file", Description = "Download file by path")]
    public async Task<DownloadFileResponse> DownloadFile([ActionParameter] DownloadFileRequest input)
    {
        return await UseClientAsync(async client =>
        {
            using var stream = new MemoryStream();

            client.DownloadFile(input.Path, stream);
            stream.Position = 0;

            var mimeType = MimeTypes.GetMimeType(input.Path);

            var file = await _fileManagementClient.UploadAsync(stream, mimeType, Path.GetFileName(input.Path));

            return new DownloadFileResponse { File=file };
        });
    }

    [Action("Upload file", Description = "Upload file by path")]
    public async Task UploadFile([ActionParameter] UploadFileRequest input)
    {
        await UseClientAsync(async client =>
        {
            var fileStream = await _fileManagementClient.DownloadAsync(input.File);

            var memoryStream = new MemoryStream();
            await fileStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            var fileName = input.FileName ?? input.File.Name;
            var path = input.Path ?? "/";
            client.UploadFile(memoryStream, $"{path.TrimEnd('/')}/{fileName}");
            return true;
        });
    }

    [Action("Delete file", Description = "Delete file by path")]
    public void DeleteFile([ActionParameter] DeleteFileRequest input)
    {
        UseClient(client => {
            client.DeleteFile(input.FilePath);
            return true;
        });
    }

    [Action("Create directory", Description = "Create new directory by path")]
    public void CreateDirectory([ActionParameter] CreateDirectoryRequest input)
    {
        UseClient(client =>
        {
            var path = input.Path ?? "/";
            client.CreateDirectory($"{path.TrimEnd('/')}/{input.DirectoryName}");
            return true;
        });
    }

    [Action("Delete directory", Description = "Delete directory by path")]
    public void DeleteDirectory([ActionParameter] DeleteDirectoryRequest input)
    {
        UseClient(client =>
        {
            client.DeleteDirectory(input.Path);
            return true;
        });
    }
}
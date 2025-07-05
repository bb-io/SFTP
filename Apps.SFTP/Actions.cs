﻿using Blackbird.Applications.Sdk.Common;
using Apps.SFTP.Models.Requests;
using Apps.SFTP.Models.Responses;
using Apps.SFTP.Dtos;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Blackbird.Applications.Sdk.Common.Invocation;
using Apps.SFTP.Invocables;
using RestSharp;
using Blackbird.Applications.SDK.Blueprints;

namespace Apps.SFTP;

[ActionList("Files")]
public class Actions : SFTPInvocable
{
    private readonly IFileManagementClient _fileManagementClient;

    public Actions(InvocationContext context, IFileManagementClient fileManagementClient) : base(context)
    {
        _fileManagementClient = fileManagementClient;
    }

    [Action("Search files", Description = "Search all files in specified folder")]
    public ListDirectoryResponse ListDirectory([ActionParameter] ListDirectoryRequest input)
    {
        return UseClient(client =>
        {
            var filesQuery = client.ListDirectory(input.Path)
                .Where(x => x.IsRegularFile);

            if (input.UpdatedFrom.HasValue)
            {
                filesQuery = filesQuery.Where(x => x.LastWriteTime >= input.UpdatedFrom.Value);
            }

            if (input.UpdatedTo.HasValue)
            {
                filesQuery = filesQuery.Where(x => x.LastWriteTime <= input.UpdatedTo.Value);
            }

            var files = filesQuery
                .Select(i => new DirectoryItemDto()
                {
                    Name = i.Name,
                    FileId = i.FullName,
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

    [BlueprintActionDefinition(BlueprintAction.DownloadFile)]
    [Action("Download file", Description = "Download file by path")]
    public async Task<DownloadFileResponse> DownloadFile([ActionParameter] DownloadFileRequest input)
    {
        return await UseClientAsync(async client =>
        {
            using var stream = new MemoryStream();

            client.DownloadFile(input.FileId, stream);
            stream.Position = 0;

            var mimeType = MimeTypes.GetMimeType(input.FileId);

            var file = await _fileManagementClient.UploadAsync(stream, mimeType, Path.GetFileName(input.FileId));

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
                var fileStream = await _fileManagementClient.DownloadAsync(input.File);
                await fileStream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;
            } else
            {
                var restClient = new RestClient(input.File.Url);
                using var responseStream = restClient.DownloadStream(new RestRequest());
                await responseStream.CopyToAsync(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);
            }

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
}
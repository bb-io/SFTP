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
        using var client = new BlackbirdSftpClient(Creds);
        var files = client.ListDirectory(input.Path).Where(x => x.IsRegularFile).Select(i => new DirectoryItemDto()
        {
            Name = i.Name,
            Path = i.FullName,
        }).ToList();
        client.Disconnect();
        return new ListDirectoryResponse()
        {
            DirectoriesItems = files
        }; 
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

            client.DownloadFile(input.Path,stream);

            var mimeType = MimeTypes.GetMimeType(input.Path);

            var file = await _fileManagementClient.UploadAsync(
                new MemoryStream(stream.GetBuffer()), mimeType,Path.GetFileName(input.Path));

            return new DownloadFileResponse { File=file };
        });
    }

    [Action("Upload file", Description = "Upload file by path")]
    public async void UploadFile([ActionParameter] UploadFileRequest input)
    {
        using var client = new BlackbirdSftpClient(Creds);
        if (input.File.Url == null) throw new Exception("For some unknown reason the file was not properly saved on Blackbird");
        var restClient = new RestClient(input.File.Url);
        using var stream = restClient.DownloadStream(new RestRequest());

        var fileName = input.FileName ?? input.File.Name;
        client.UploadFile(stream, $"{input.Path.TrimEnd('/')}/{fileName}");
        client.Disconnect();
    }

    [Action("Delete file", Description = "Delete file by path")]
    public void DeleteFile([ActionParameter] DeleteFileRequest input)
    {
        using var client = new BlackbirdSftpClient(Creds);
        client.DeleteFile(input.FilePath);
        client.Disconnect();
    }

    [Action("Create directory", Description = "Create new directory by path")]
    public void CreateDirectory([ActionParameter] CreateDirectoryRequest input)
    {
        using var client = new BlackbirdSftpClient(Creds);
        try
        {
            client.CreateDirectory($"{input.Path.TrimEnd('/')}/{input.DirectoryName}");
        }
        catch (Exception) { }
        client.Disconnect();
    }

    [Action("Delete directory", Description = "Delete directory by path")]
    public void DeleteDirectory([ActionParameter] DeleteDirectoryRequest input)
    {
        using var client = new BlackbirdSftpClient(Creds);
        client.DeleteDirectory(input.Path);
        client.Disconnect();
    }
}
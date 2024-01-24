using System.Net.Mime;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Apps.SFTP.Models.Requests;
using Apps.SFTP.Models.Responses;
using Apps.SFTP.Dtos;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Blackbird.Applications.Sdk.Utils.Extensions.Files;
using Blackbird.Applications.Sdk.Common.Invocation;
using System.Text;
using RestSharp;

namespace Apps.SFTP;

[ActionList]
public class Actions : BaseInvocable
{
    private readonly IFileManagementClient _fileManagementClient;
    private AuthenticationCredentialsProvider[] Creds =>
        InvocationContext.AuthenticationCredentialsProviders.ToArray();

    public Actions(InvocationContext context, IFileManagementClient fileManagementClient) : base(context)
    {
        _fileManagementClient = fileManagementClient;
    }

    [Action("List directory files", Description = "List all files in specified directory")]
    public ListDirectoryResponse ListDirectory([ActionParameter] ListDirectoryRequest input)
    {
        using (var client = new BlackbirdSftpClient(Creds))
        {
            var files = client.ListDirectory(input.Path).Where(x => x.IsRegularFile).Select(i => new DirectoryItemDto()
            {
                Name = i.Name,
                Path = i.FullName,
            }).ToList();

            return new ListDirectoryResponse()
            {
                DirectoriesItems = files
            };
        }
    }

    [Action("Rename file", Description = "Rename a path from old to new")]
    public void RenameFile([ActionParameter] RenameFileRequest input)
    {
        using (var client = new BlackbirdSftpClient(Creds))
        {
            client.RenameFile(input.OldPath, input.NewPath);
        }
    }

    [Action("Download file", Description = "Download file by path")]
    public async Task<DownloadFileResponse> DownloadFile([ActionParameter] DownloadFileRequest input)
    {
        using var client = new BlackbirdSftpClient(Creds);
        using var stream = new MemoryStream();
        
        client.DownloadFile(input.Path, stream);

        MimeTypes.FallbackMimeType = MediaTypeNames.Application.Octet;
        var mimeType = MimeTypes.GetMimeType(input.Path);

        var file = await _fileManagementClient.UploadAsync(new MemoryStream(stream.GetBuffer()), mimeType, Path.GetFileName(input.Path));
        return new() { File = file };
    }

    [Action("Upload file", Description = "Upload file by path")]
    public void UploadFile([ActionParameter] UploadFileRequest input)
    {
        using var client = new BlackbirdSftpClient(Creds);
        using var stream = _fileManagementClient.DownloadAsync(input.File).Result;

        var fileName = input.FileName ?? input.File.Name;
        client.UploadFile(stream, $"{input.Path.TrimEnd('/')}/{fileName}");
    }

    [Action("Delete file", Description = "Delete file by path")]
    public void DeleteFile([ActionParameter] DeleteFileRequest input)
    {
        using (var client = new BlackbirdSftpClient(Creds))
        {
            client.DeleteFile(input.FilePath);
        }
    }

    [Action("Create directory", Description = "Create anew directory inside of a path")]
    public void CreateDirectory([ActionParameter] CreateDirectoryRequest input)
    {
        using (var client = new BlackbirdSftpClient(Creds))
        {
            client.CreateDirectory($"{input.Path.TrimEnd('/')}/{input.DirectoryName}");
        }
    }

    [Action("Delete directory", Description = "Delete directory")]
    public void DeleteDirectory([ActionParameter] DeleteDirectoryRequest input)
    {
        using (var client = new BlackbirdSftpClient(Creds))
        {
            client.DeleteDirectory(input.Path);
        }
    }
}
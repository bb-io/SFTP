using System.Net.Mime;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Apps.SFTP.Models.Requests;
using Apps.SFTP.Models.Responses;
using Apps.SFTP.Dtos;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Blackbird.Applications.Sdk.Utils.Extensions.Files;

namespace Apps.SFTP;

[ActionList]
public class Actions
{
    private readonly IFileManagementClient _fileManagementClient;

    public Actions(IFileManagementClient fileManagementClient)
    {
        _fileManagementClient = fileManagementClient;
    }

    [Action("List directory", Description = "List specified directory")]
    public ListDirectoryResponse ListDirectory(
        IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
        [ActionParameter] ListDirectoryRequest input)
    {
        using (var client = new BlackbirdSftpClient(authenticationCredentialsProviders))
        {
            var files = client.ListDirectory(input.Path).Select(i => new DirectoryItemDto()
            {
                Name = i.Name
            }).ToList();

            return new ListDirectoryResponse()
            {
                DirectoriesItems = files
            };
        }
    }

    [Action("Get file information", Description = "Get file information")]
    public GetFileInformationResponse GetFileInformation(
        IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
        [ActionParameter] GetFileInformationRequest input)
    {
        using (var client = new BlackbirdSftpClient(authenticationCredentialsProviders))
        {
            var fileInfo = client.Get(input.FilePath);
            return new GetFileInformationResponse()
            {
                Size = fileInfo.Attributes.Size,
                Path = fileInfo.FullName
            };
        }
    }

    [Action("Rename file", Description = "Rename file")]
    public void RenameFile(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
        [ActionParameter] RenameFileRequest input)
    {
        using (var client = new BlackbirdSftpClient(authenticationCredentialsProviders))
        {
            client.RenameFile(input.OldPath, input.NewPath);
        }
    }

    [Action("Download file", Description = "Download file by path")]
    public async Task<DownloadFileResponse> DownloadFile(
        IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
        [ActionParameter] DownloadFileRequest input)
    {
        using var client = new BlackbirdSftpClient(authenticationCredentialsProviders);
        using var stream = new MemoryStream();
        
        client.DownloadFile(input.Path, stream);
        var file = await _fileManagementClient.UploadAsync(stream, MediaTypeNames.Application.Octet, Path.GetFileName(input.Path));
        return new() { File = file };
    }

    [Action("Upload file", Description = "Upload file by path")]
    public void UploadFile(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
        [ActionParameter] UploadFileRequest input)
    {
        using var client = new BlackbirdSftpClient(authenticationCredentialsProviders);
        using var stream = _fileManagementClient.DownloadAsync(input.File).Result;

        var fileName = input.FileName ?? input.File.Name;
        client.UploadFile(stream, $"{input.Path.TrimEnd('/')}/{fileName}");
    }

    [Action("Delete file", Description = "Delete file by path")]
    public void DeleteFile(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
        [ActionParameter] DeleteFileRequest input)
    {
        using (var client = new BlackbirdSftpClient(authenticationCredentialsProviders))
        {
            client.DeleteFile(input.FilePath);
        }
    }

    [Action("Create directory", Description = "Create directory by path")]
    public void CreateDirectory(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
        [ActionParameter] CreateDirectoryRequest input)
    {
        using (var client = new BlackbirdSftpClient(authenticationCredentialsProviders))
        {
            client.CreateDirectory($"{input.Path.TrimEnd('/')}/{input.DirectoryName}");
        }
    }

    [Action("Delete directory", Description = "Delete directory")]
    public void DeleteDirectory(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
        [ActionParameter] DeleteDirectoryRequest input)
    {
        using (var client = new BlackbirdSftpClient(authenticationCredentialsProviders))
        {
            client.DeleteDirectory(input.Path);
        }
    }
}
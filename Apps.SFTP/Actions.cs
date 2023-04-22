using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using System.IO;
using System.Collections.Generic;
using System;
using Renci.SshNet;
using Apps.SFTP.Models.Requests;
using Apps.SFTP.Models.Responses;
using Apps.SFTP.Dtos;
using static System.Net.WebRequestMethods;
using Blackbird.Applications.Sdk.Common.Actions;

namespace Apps.SFTP
{
    [ActionList]
    public class Actions
    {
        [Action("List directory", Description = "List specified directory")]
        public ListDirectoryResponse ListDirectory(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
           [ActionParameter] ListDirectoryRequest input)
        {
            using(var client = new BlackbirdSftpClient(authenticationCredentialsProviders)) {
                
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
        public GetFileInformationResponse GetFileInformation(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
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
        public DownloadFileResponse DownloadFile(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
           [ActionParameter] DownloadFileRequest input)
        {
            using (var client = new BlackbirdSftpClient(authenticationCredentialsProviders))
            {
                using (var stream = new MemoryStream())
                {
                    client.DownloadFile(input.Path, stream);
                    var fileData = stream.ToArray();
                    return new DownloadFileResponse()
                    {
                        File = fileData
                    };
                }   
            }
        }

        [Action("Upload file", Description = "Upload file by path")]
        public void UploadFile(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
           [ActionParameter] UploadFileRequest input)
        {
            using (var client = new BlackbirdSftpClient(authenticationCredentialsProviders))
            {
                using (var stream = new MemoryStream(input.File))
                {
                    client.UploadFile(stream, $"{input.Path.TrimEnd('/')}/{input.FileName}");
                }
            }
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
}

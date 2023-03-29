﻿using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using System.IO;
using System.Collections.Generic;
using System;
using Renci.SshNet;
using Apps.SFTP.Models.Requests;
using Apps.SFTP.Models.Responses;
using Apps.SFTP.Dtos;
using static System.Net.WebRequestMethods;

namespace Apps.SFTP
{
    [ActionList]
    public class Actions
    {
        [Action("List directory", Description = "List specified directory")]
        public ListDirectoryResponse ListDirectory(string host, string port, string login, AuthenticationCredentialsProvider authenticationCredentialsProvider,
           [ActionParameter] ListDirectoryRequest input)
        {
            using(var client = GetSftpClient(host, port, login, authenticationCredentialsProvider.Value)) {
                
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

        [Action("Download file", Description = "Download file by path")]
        public DownloadFileResponse DownloadFile(string host, string port, string login, AuthenticationCredentialsProvider authenticationCredentialsProvider,
           [ActionParameter] DownloadFileRequest input)
        {
            using (var client = GetSftpClient(host, port, login, authenticationCredentialsProvider.Value))
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
        public void UploadFile(string host, string port, string login, AuthenticationCredentialsProvider authenticationCredentialsProvider,
           [ActionParameter] UploadFileRequest input)
        {
            using (var client = GetSftpClient(host, port, login, authenticationCredentialsProvider.Value))
            {
                using (var stream = new MemoryStream(input.File))
                {
                    client.UploadFile(stream, $"{input.Path.TrimEnd('/')}/{input.FileName}");
                }
            }
        }

        [Action("Create directory", Description = "Create directory by path")]
        public void CreateDirectory(string host, string port, string login, AuthenticationCredentialsProvider authenticationCredentialsProvider,
           [ActionParameter] CreateDirectoryRequest input)
        {
            using (var client = GetSftpClient(host, port, login, authenticationCredentialsProvider.Value))
            {
                client.CreateDirectory($"{input.Path.TrimEnd('/')}/{input.DirectoryName}");
            }
        }

        [Action("Delete directory", Description = "Delete directory")]
        public void DeleteDirectory(string host, string port, string login, AuthenticationCredentialsProvider authenticationCredentialsProvider,
           [ActionParameter] DeleteDirectoryRequest input)
        {
            using (var client = GetSftpClient(host, port, login, authenticationCredentialsProvider.Value))
            {
                client.DeleteDirectory(input.Path);
            }
        }

        private SftpClient GetSftpClient(string host, string port, string login, string password)
        {
            var connectionInfo = new ConnectionInfo(host, Int32.Parse(port), login, 
                new PasswordAuthenticationMethod(login, password));
            var client = new SftpClient(connectionInfo);
            client.Connect();
            return client;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apps.SFTP;
using Apps.SFTP.Connections;
using Apps.SFTP.Models.Requests;
using Blackbird.Applications.Sdk.Common.Files;

namespace Tests.SFTP
{
    [TestClass]
    public class ActionsTests :TestBase
    {

        [TestMethod]
        public async Task CreateDirectory_IsOk()
        {
            var client = new Actions(InvocationContext, FileManager);
            var input = new CreateDirectoryRequest
            {
                Path="/newpath",
                DirectoryName="directory1"
            };

            client.CreateDirectory(input);
        }

        [TestMethod]
        public async Task DeleteDirectory_IsOk()
        {
            var client = new Actions(InvocationContext, FileManager);
            var input = new DeleteDirectoryRequest { Path= "/newpath/directory1" };

            client.DeleteDirectory(input);
        }

        [TestMethod]
        public async Task UploadFile_IsOk()
        {
            var client = new Actions(InvocationContext, FileManager);
            var fileName = "Translate.txt";
            var input = new UploadFileRequest
            {
                File = new FileReference
                {
                    Name = fileName
                },
                Path = "/upload",
                FileName = fileName
            };
            client.UploadFile(input);
        }

        [TestMethod]
        public async Task RenameFile_IsOk()
        {
            var client = new Actions(InvocationContext, FileManager);
            var input = new RenameFileRequest { NewPath = "/newpath/some223.txt", OldPath = "/newpath/some.txt" };
            client.RenameFile(input);
        }

        [TestMethod]
        public async Task DeleteFile_IsOk()
        {
            var client = new Actions(InvocationContext, FileManager);
            var input = new DeleteFileRequest { FilePath = "/newpath/some223.txt" };

            client.DeleteFile(input);
        }

        [TestMethod]
        public async Task ListDirectoryFiles_IsOk()
        {
            var client = new Actions(InvocationContext, FileManager);
            var input = new ListDirectoryRequest { Path = "/newpath" };
            client.ListDirectory(input);
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apps.SFTP;
using Apps.SFTP.Connections;
using Apps.SFTP.Models.Requests;
using Blackbird.Applications.Sdk.Common.Files;
using Microsoft.Extensions.Options;
using Tests.SFTP.Base;

namespace Tests.SFTP
{
    [TestClass]
    public class ActionsTests : TestBase
    {
        public const string fileName = "Translate.txt";
        public const string alternativeFileName = "Translate_renamed.txt";
        public const string directory = "test";

        public const string sizeTestFilename = "test.json";

        private bool DoesFileExist(string directory, string fileName)
        {
            var actions = new Actions(InvocationContext, FileManager);
            var directoryResponse = actions.ListDirectory(new ListDirectoryRequest { Path = directory });
            return directoryResponse.DirectoriesItems.Any(x => x.Name == fileName);
        }

        [TestMethod]
        public void CreateDirectory_IsOk()
        {
            var actions = new Actions(InvocationContext, FileManager);
            var input = new CreateDirectoryRequest
            {
                DirectoryName= "/test"
            };

            actions.CreateDirectory(input);
        }

        [TestMethod]
        public void DeleteDirectory_IsOk()
        {
            CreateDirectory_IsOk();
            var actions = new Actions(InvocationContext, FileManager);
            var input = new DeleteDirectoryRequest { Path = directory };

            actions.DeleteDirectory(input);
        }

        [TestMethod]
        public async Task UploadFile_IsOk()
        {
            var actions = new Actions(InvocationContext, FileManager);
            var input = new UploadFileRequest
            {
                File = new FileReference
                {
                    Name = "Translate.txt"
                },
                Path = directory,
            };
            await actions.UploadFile(input);
            //Assert.IsTrue(DoesFileExist(directory, fileName));
        }

        [TestMethod]
        public async Task DownloadFile_IsOk()
        {
            await UploadFile_IsOk();
            var actions = new Actions(InvocationContext, FileManager);
            var response = await actions.DownloadFile(new DownloadFileRequest { Path = directory + '/' + fileName});
            Assert.IsTrue(response.File.Name == fileName);
        }

        [TestMethod]
        public async Task DownloadFile_Throws_for_unknown_file()
        {
            var actions = new Actions(InvocationContext, FileManager);
            await Throws.MisconfigurationException(() => actions.DownloadFile(new DownloadFileRequest { Path = directory + '/' + "does_not_exist.txt" }));
        }

        [TestMethod]
        public async Task RenameFile_IsOk()
        {
            await UploadFile_IsOk();
            var actions = new Actions(InvocationContext, FileManager);
            var input = new RenameFileRequest { NewPath = directory + '/' + alternativeFileName, OldPath = directory + '/' + fileName };
            actions.RenameFile(input);

            Assert.IsTrue(DoesFileExist(directory, alternativeFileName));
        }

        [TestMethod]
        public async Task RenameFile_Throws_for_unknown_file()
        {
            await UploadFile_IsOk();
            var actions = new Actions(InvocationContext, FileManager);
            var input = new RenameFileRequest { NewPath = directory + '/' + alternativeFileName, OldPath = directory + '/' + "does_not_exist.txt" };

            Throws.MisconfigurationException(() => actions.RenameFile(input));
        }

        [TestMethod]
        public async Task DeleteFile_IsOk()
        {
            await UploadFile_IsOk();
            var actions = new Actions(InvocationContext, FileManager);
            var input = new DeleteFileRequest { FilePath = directory + '/' + fileName };

            actions.DeleteFile(input);
            Assert.IsFalse(DoesFileExist(directory, alternativeFileName));
        }

        [TestMethod]
        public void DeleteFile_Throws_for_unknown_file()
        {
            var actions = new Actions(InvocationContext, FileManager);
            var input = new DeleteFileRequest { FilePath = directory + '/' + "does_not_exists.txt" };

            Throws.MisconfigurationException(() => actions.DeleteFile(input));
        }

        [TestMethod]
        public async Task ListDirectoryFiles_IsOk()
        {
            await UploadFile_IsOk();
            var actions = new Actions(InvocationContext, FileManager);
            var input = new ListDirectoryRequest { Path = directory };
            var response = actions.ListDirectory(input);

            Assert.IsTrue(response.DirectoriesItems.Count() > 0);

            foreach(var item in response.DirectoriesItems)
            {
                Console.WriteLine(item.Name);
            }
        }


        [TestMethod]
        public async Task SearchFiles_IsOk()
        {
            await UploadFile_IsOk();
            var actions = new Actions(InvocationContext, FileManager);

            var updatedFrom = DateTime.Now.AddMinutes(-1);
            var updatedTo = DateTime.Now.AddMinutes(1);
            var input = new ListDirectoryRequest
            {
                Path = directory,
                UpdatedFrom = updatedFrom,
                UpdatedTo = updatedTo
            };

            var response = actions.ListDirectory(input);

            Assert.IsTrue(response.DirectoriesItems.Any());

            foreach (var item in response.DirectoriesItems)
            {
                Console.WriteLine(item.Name);
            }
        }

        [TestMethod]
        public async Task Uploaded_and_downloaded_file_are_equal_size()
        {
            var actions = new Actions(InvocationContext, FileManager);
            var input = new UploadFileRequest
            {
                File = new FileReference
                {
                    Name = sizeTestFilename
                },
                Path = directory,
            };
            await actions.UploadFile(input);
            await actions.DownloadFile(new DownloadFileRequest { Path = directory + '/' + sizeTestFilename });

            var uploadFileSize = GetInputFileSize(sizeTestFilename);
            var downloadFileSize = GetOutputFileSize(sizeTestFilename);

            Assert.AreEqual(uploadFileSize, downloadFileSize);
        }

        [TestMethod]
        public async Task DownloadAllFiles_IsOk()
        {
            var actions = new Actions(InvocationContext, FileManager);
            var response = await actions.DownloadAllFiles(new ListDirectoryRequest { Path = "/"+directory});
            Assert.IsNotNull(response);
        }
    }
}

﻿using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;

namespace Tests.SFTP
{
    public class FileManager : IFileManagementClient
    {
        public readonly string inputFolder;
        public readonly string outputFolder;

        public FileManager(string folderLocation)
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var projectDirectory = Directory.GetParent(baseDirectory).Parent.Parent.Parent.FullName;


            var testFilesPath = Path.Combine(projectDirectory, "TestFiles");
            inputFolder = Path.Combine(folderLocation, "Input");
            outputFolder = Path.Combine(folderLocation, "Output");

            Directory.CreateDirectory(inputFolder);
            Directory.CreateDirectory(outputFolder);
        }


        public Task<Stream> DownloadAsync(FileReference reference)
        {
            var path = Path.Combine(inputFolder, reference.Name);
            Assert.IsTrue(File.Exists(path), $"File not found at: {path}");
            var bytes = File.ReadAllBytes(path);

            var stream = new MemoryStream(bytes);
            return Task.FromResult((Stream)stream);
        }

        public Task<FileReference> UploadAsync(Stream stream, string contentType, string fileName)
        {
            var path = Path.Combine(outputFolder, fileName);
            new FileInfo(path).Directory.Create();
            using (var fileStream = File.Create(path))
            {
                stream.CopyTo(fileStream);
            }

            return Task.FromResult(new FileReference() { Name = fileName, ContentType=contentType });
        }

    }
}
﻿using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Invocation;
using Microsoft.Extensions.Configuration;

namespace Tests.SFTP
{
    public class TestBase
    {
        public IEnumerable<AuthenticationCredentialsProvider> Creds { get; set; }

        public InvocationContext InvocationContext { get; set; }

        public FileManager FileManager { get; set; }

        public TestBase()
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            Creds = config.GetSection("ConnectionDefinition").GetChildren()
                .Select(x => new AuthenticationCredentialsProvider(AuthenticationCredentialsRequestLocation.None,x.Key, x.Value)).ToList();


            var relativePath = config.GetSection("TestFolder").Value;
            var projectDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName;
            var folderLocation = Path.Combine(projectDirectory, relativePath);

            InvocationContext = new InvocationContext
            {
                AuthenticationCredentialsProviders = Creds,
            };

            FileManager = new FileManager(folderLocation);
        }

        public int GetInputFileSize(string fileName)
        {
            var path = Path.Combine(FileManager.inputFolder, fileName);
            Assert.IsTrue(File.Exists(path), $"File not found at: {path}");
            var bytes = File.ReadAllBytes(path);
            return bytes.Length;
        }

        public int GetOutputFileSize(string fileName)
        {
            var path = Path.Combine(FileManager.outputFolder, fileName);
            Assert.IsTrue(File.Exists(path), $"File not found at: {path}");
            var bytes = File.ReadAllBytes(path);
            return bytes.Length;
        }
    }
}

using Apps.SFTP;
using Apps.SFTP.Models.Requests;
using Blackbird.Applications.Sdk.Common.Files;
using Tests.SFTP.Base;

namespace Tests.SFTP;

[TestClass]
public class ActionsTests : ActionsTestsBase
{
    public ActionsTests() : base("ConnectionDefinition")
    {
    }
}

[TestClass]
public class FtpActionsTests : ActionsTestsBase
{
    public FtpActionsTests() : base("FtpConnectionDefinition")
    {
    }
}

public abstract class ActionsTestsBase : TestBase
{
    protected const string FileName = "Translate.txt";
    protected const string AlternativeFileName = "Translate_renamed.txt";
    protected const string DirectoryPath = "/test";
    protected const string SizeTestFilename = "test.json";

    protected ActionsTestsBase(string connectionSectionName) : base(connectionSectionName)
    {
    }

    private async Task<bool> DoesFileExist(string directory, string fileName)
    {
        var actions = new Actions(InvocationContext, FileManager);
        var directoryResponse = await actions.ListDirectory(new ListDirectoryRequest { FolderPath = directory });
        return directoryResponse.DirectoriesItems.Any(x => x.Name == fileName);
    }

    [TestMethod]
    public async Task UploadFile_IsOk()
    {
        var actions = new Actions(InvocationContext, FileManager);
        await actions.UploadFile(new UploadFileRequest
        {
            File = new FileReference
            {
                Name = FileName
            },
            Path = DirectoryPath,
        });

        Assert.IsTrue(await DoesFileExist(DirectoryPath, FileName));
    }

    [TestMethod]
    public async Task DownloadFile_IsOk()
    {
        await UploadFile_IsOk();

        var actions = new Actions(InvocationContext, FileManager);
        var response = await actions.DownloadFile(new DownloadFileRequest { FileId = DirectoryPath + '/' + FileName });
        Assert.AreEqual(FileName, response.File.Name);
    }

    [TestMethod]
    public async Task DownloadFile_Throws_for_unknown_file()
    {
        var actions = new Actions(InvocationContext, FileManager);
        await Throws.MisconfigurationException(() =>
            actions.DownloadFile(new DownloadFileRequest { FileId = DirectoryPath + '/' + "does_not_exist.txt" }));
    }

    [TestMethod]
    public async Task RenameFile_IsOk()
    {
        await UploadFile_IsOk();

        string targetDirectory = "/test2";
        var actions = new Actions(InvocationContext, FileManager);
        await actions.RenameFile(new RenameFileRequest
        {
            NewFileName = $"{targetDirectory}/{AlternativeFileName}",
            OldPath = $"{DirectoryPath}/{FileName}"
        });
        
        
        Assert.IsFalse(await DoesFileExist(DirectoryPath, FileName), "File should no longer be in the old directory.");
        Assert.IsTrue(await DoesFileExist(targetDirectory, AlternativeFileName), "File was not found in the new directory.");
    }

    [TestMethod]
    public async Task RenameFile_Throws_for_unknown_file()
    {
        await UploadFile_IsOk();

        var actions = new Actions(InvocationContext, FileManager);
        await Throws.MisconfigurationException(() => actions.RenameFile(new RenameFileRequest
        {
            NewFileName = AlternativeFileName,
            OldPath = DirectoryPath + '/' + "does_not_exist.txt"
        }));
    }

    [TestMethod]
    public async Task DeleteFile_IsOk()
    {
        await UploadFile_IsOk();

        var actions = new Actions(InvocationContext, FileManager);
        await actions.DeleteFile(new DeleteFileRequest { FilePath = DirectoryPath + '/' + FileName });
        Assert.IsFalse(await DoesFileExist(DirectoryPath, FileName));
    }

    [TestMethod]
    public async Task DeleteFile_Throws_for_unknown_file()
    {
        var actions = new Actions(InvocationContext, FileManager);
        await Throws.MisconfigurationException(() => actions.DeleteFile(new DeleteFileRequest
        {
            FilePath = DirectoryPath + '/' + "does_not_exists.txt"
        }));
    }

    [TestMethod]
    public async Task ListDirectoryFiles_IsOk()
    {
        await UploadFile_IsOk();

        var actions = new Actions(InvocationContext, FileManager);
        var response = await actions.ListDirectory(new ListDirectoryRequest { FolderPath = DirectoryPath });
        Assert.IsTrue(response.DirectoriesItems.Any());
    }

    [TestMethod]
    public async Task SearchFiles_IsOk()
    {
        await UploadFile_IsOk();

        var actions = new Actions(InvocationContext, FileManager);
        var response = await actions.ListDirectory(new ListDirectoryRequest
        {
            FolderPath = DirectoryPath,
            UpdatedFrom = DateTime.Now.AddMinutes(-1),
            UpdatedTo = DateTime.Now.AddMinutes(1)
        });

        Assert.IsTrue(response.DirectoriesItems.Any());
    }

    [TestMethod]
    public async Task Uploaded_and_downloaded_file_are_equal_size()
    {
        var actions = new Actions(InvocationContext, FileManager);
        await actions.UploadFile(new UploadFileRequest
        {
            File = new FileReference
            {
                Name = SizeTestFilename
            },
            Path = DirectoryPath,
        });

        await actions.DownloadFile(new DownloadFileRequest { FileId = DirectoryPath + '/' + SizeTestFilename });

        var uploadFileSize = GetInputFileSize(SizeTestFilename);
        var downloadFileSize = GetOutputFileSize(SizeTestFilename);

        Assert.AreEqual(uploadFileSize, downloadFileSize);
    }

    [TestMethod]
    public async Task DownloadFile__IsOk()
    {
        await UploadFile_IsOk();

        var actions = new Actions(InvocationContext, FileManager);
        var response = await actions.DownloadFile(new DownloadFileRequest { FileId = DirectoryPath + '/' + FileName });
        Assert.IsNotNull(response);
    }
}

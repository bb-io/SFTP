using Apps.SFTP;
using Apps.SFTP.DataHandlers;
using Apps.SFTP.Models.Requests;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems;
using Newtonsoft.Json;

namespace Tests.SFTP;

[TestClass]
public class DataSourceTests : TestBase
{
    public const string fileName = "Translate.txt";
    public const string alternativeFileName = "Translate_renamed.txt";
    public const string directory = "test";

    public const string sizeTestFilename = "test.json";

    [TestMethod]
    public void Folder_data_handler()
    {
        var handler = new FolderDataHandler(InvocationContext);

        var actions = new FolderActions(InvocationContext);
        actions.CreateDirectory(new CreateDirectoryRequest { FolderPath = null, Name = "Test 1"});
        actions.CreateDirectory(new CreateDirectoryRequest { FolderPath = null, Name = "Test 2" });
        actions.CreateDirectory(new CreateDirectoryRequest { FolderPath = "/Test 1", Name = "Test 3" });

        var folders = handler.GetFolderContent(new FolderContentDataSourceContext { FolderId = null });
        Console.WriteLine(JsonConvert.SerializeObject(folders, Formatting.Indented));
        Assert.IsTrue(folders.Any(x => x.DisplayName == "Test 1"));
        Assert.IsTrue(folders.Any(x => x.DisplayName == "Test 2"));

        var folders2 = handler.GetFolderContent(new FolderContentDataSourceContext { FolderId = "/Test 1" });
        Console.WriteLine(JsonConvert.SerializeObject(folders2, Formatting.Indented));
        Assert.IsTrue(folders2.Any(x => x.DisplayName == "Test 3"));

        var breadCrumbs = handler.GetFolderPath(new FolderPathDataSourceContext { FileDataItemId = "/Test 1/test.txt" });
        Console.WriteLine(JsonConvert.SerializeObject(breadCrumbs, Formatting.Indented));
        Assert.IsTrue(breadCrumbs.First().DisplayName == "/");
        Assert.IsTrue(breadCrumbs.Last().DisplayName == "Test 1/");
    }

    [TestMethod]
    public void Folder_data_handler_empty()
    {
        var handler = new FolderDataHandler(InvocationContext);
        var folders = handler.GetFolderContent(new FolderContentDataSourceContext { FolderId = "" });
        Console.WriteLine(JsonConvert.SerializeObject(folders, Formatting.Indented));
    }

    [TestMethod]
    public async Task File_data_handler()
    {
        var handler = new FileDataHandler(InvocationContext);

        var actions = new Actions(InvocationContext, FileManager);

        var input = new UploadFileRequest
        {
            File = new FileReference
            {
                Name = fileName
            },
        };
        await actions.UploadFile(input);

        var folders = handler.GetFolderContent(new FolderContentDataSourceContext { FolderId = null });
        Console.WriteLine(JsonConvert.SerializeObject(folders, Formatting.Indented));
        Assert.IsTrue(folders.Any(x => x.DisplayName == fileName));

        var breadCrumbs = handler.GetFolderPath(new FolderPathDataSourceContext { FileDataItemId = "/Test 1/test.txt" });
        Console.WriteLine(JsonConvert.SerializeObject(breadCrumbs, Formatting.Indented));
        Assert.IsTrue(breadCrumbs.First().DisplayName == "/");

    }
}

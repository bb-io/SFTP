using Apps.SFTP;
using Apps.SFTP.Models.Requests;
using Apps.SFTP.Webhooks;
using Apps.SFTP.Webhooks.Payload;
using Apps.SFTP.Webhooks.Polling.Memory;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.Sdk.Common.Polling;
using Newtonsoft.Json;

namespace Tests.SFTP;

[TestClass]
public class PollingTests : TestBase
{
    public const string fileName = "Translate.txt";
    public const string alternativeFileName = "Translate_renamed.txt";
    public const string directory = "test";

    public const string sizeTestFilename = "test.json";

    [TestMethod]
    public async Task Created_or_updated()
    {
        var actions = new Actions(InvocationContext, FileManager);
        var polling = new PollingList(InvocationContext);

        var firstPoll = await polling.OnFilesAddedOrUpdated(new PollingEventRequest<SFTPMemory> { Memory = null, PollingTime = DateTime.Now }, new ParentFolderInput { });
        Console.WriteLine(JsonConvert.SerializeObject(firstPoll, Formatting.Indented));

        var input = new UploadFileRequest
        {
            File = new FileReference
            {
                Name = fileName
            },
            Path = directory,
        };

        await actions.UploadFile(input);

        var secondPoll = await polling.OnFilesAddedOrUpdated(new PollingEventRequest<SFTPMemory> { Memory = firstPoll.Memory, PollingTime = DateTime.Now }, new ParentFolderInput { });

        Console.WriteLine(JsonConvert.SerializeObject(secondPoll, Formatting.Indented));

        Assert.IsTrue(secondPoll.FlyBird);
    }
}

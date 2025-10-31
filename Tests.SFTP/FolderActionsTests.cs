using Apps.SFTP;
using Apps.SFTP.Models.Requests;

namespace Tests.SFTP
{
    [TestClass]
    public class FolderActionsTests : TestBase
    {
        public const string fileName = "Translate.txt";
        public const string alternativeFileName = "Translate_renamed.txt";
        public const string directory = "test2";

        public const string sizeTestFilename = "test.json";

        [TestMethod]
        public void CreateDirectory_IsOk()
        {
            var actions = new FolderActions(InvocationContext);
            var input = new CreateDirectoryRequest
            {
                FolderPath = "/<!&@9fe137c94360b321&>"
            };

            actions.CreateDirectory(input);
        }

        [TestMethod]
        public void DeleteDirectory_IsOk()
        {            
            var actions = new FolderActions(InvocationContext);
            actions.CreateDirectory(new CreateDirectoryRequest
            {
                FolderPath = null,
                Name = directory,
            });
            var input = new DeleteDirectoryRequest { FolderPath = directory  };

            actions.DeleteDirectory(input);
        }
    }
}

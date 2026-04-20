using Apps.SFTP;
using Apps.SFTP.Models.Requests;

namespace Tests.SFTP;

[TestClass]
public class FolderActionsTests : FolderActionsTestsBase
{
    public FolderActionsTests() : base("ConnectionDefinition")
    {
    }
}

[TestClass]
public class FtpFolderActionsTests : FolderActionsTestsBase
{
    public FtpFolderActionsTests() : base("FtpConnectionDefinition")
    {
    }
}

public abstract class FolderActionsTestsBase : TestBase
{
    protected const string DirectoryName = "test2";

    protected FolderActionsTestsBase(string connectionSectionName) : base(connectionSectionName)
    {
    }

    [TestMethod]
    public async Task CreateDirectory_IsOk()
    {
        var actions = new FolderActions(InvocationContext);
        await actions.CreateDirectory(new CreateDirectoryRequest
        {
            FolderPath = "/<!&@9fe137c94360b321&>",
            Name = DirectoryName
        });
    }

    [TestMethod]
    public async Task DeleteDirectory_IsOk()
    {
        var actions = new FolderActions(InvocationContext);
        await actions.CreateDirectory(new CreateDirectoryRequest
        {
            FolderPath = null,
            Name = DirectoryName,
        });

        await actions.DeleteDirectory(new DeleteDirectoryRequest { FolderPath = DirectoryName });
    }
}

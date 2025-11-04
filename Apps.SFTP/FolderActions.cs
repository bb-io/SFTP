using Blackbird.Applications.Sdk.Common;
using Apps.SFTP.Models.Requests;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Apps.SFTP.Invocables;

namespace Apps.SFTP;

[ActionList("Folders")]
public class FolderActions(InvocationContext context) : SFTPInvocable(context)
{
    [Action("Create folder", Description = "Create new folder by path")]
    public void CreateDirectory([ActionParameter] CreateDirectoryRequest input)
    {
        UseClient(client =>
        {
            client.CreateDirectory(input.FolderPath + "/" + input.Name);
            return true;
        });
    }

    [Action("Delete folder", Description = "Delete folder by path")]
    public void DeleteDirectory([ActionParameter] DeleteDirectoryRequest input)
    {
        UseClient(client =>
        {
            client.DeleteDirectory(input.FolderPath);
            return true;
        });
    }
}
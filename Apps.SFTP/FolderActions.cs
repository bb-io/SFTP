using Apps.SFTP.Invocables;
using Apps.SFTP.Models.Requests;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.SFTP;

[ActionList("Folders")]
public class FolderActions(InvocationContext context) : FileTransferInvocable(context)
{
    [Action("Create folder", Description = "Create new folder by path")]
    public void CreateDirectory([ActionParameter] CreateDirectoryRequest input)
    {
        var path = string.IsNullOrWhiteSpace(input.FolderPath)
            ? input.Name
            : $"{input.FolderPath.TrimEnd('/')}/{input.Name}";

        UseClient(client =>
        {
            client.CreateDirectoryAsync(path).GetAwaiter().GetResult();
            return true;
        });
    }

    [Action("Delete folder", Description = "Delete folder by path")]
    public void DeleteDirectory([ActionParameter] DeleteDirectoryRequest input)
    {
        if (string.IsNullOrWhiteSpace(input.FolderPath))
        {
            throw new PluginMisconfigurationException("Please enter a valid path.");
        }

        UseClient(client =>
        {
            client.DeleteDirectoryAsync(input.FolderPath).GetAwaiter().GetResult();
            return true;
        });
    }
}

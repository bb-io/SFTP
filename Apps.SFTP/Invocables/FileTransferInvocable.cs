using Apps.SFTP.Api;
using Apps.SFTP.Models;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.SFTP.Invocables;

public class FileTransferInvocable(InvocationContext invocationContext) : BaseInvocable(invocationContext)
{
    protected AuthenticationCredentialsProvider[] Creds =>
        InvocationContext.AuthenticationCredentialsProviders.ToArray();

    protected async Task<List<FileTransferItem>> ListDirectoryItemsAsync(
        string folderPath,
        bool includeSubfolders,
        Func<FileTransferItem, bool> predicate)
    {
        using var client = FileTransferClientFactory.Create(Creds);
        await client.ConnectAsync();

        var items = await client.ExecuteAsync(async () =>
            (await client.ListDirectoryAsync(folderPath, includeSubfolders)).ToList());

        return items.Where(predicate).ToList();
    }
}

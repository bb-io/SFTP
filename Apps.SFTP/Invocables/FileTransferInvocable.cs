using Apps.SFTP.Api;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.SFTP.Invocables;

public class FileTransferInvocable(InvocationContext invocationContext) : BaseInvocable(invocationContext)
{
    protected AuthenticationCredentialsProvider[] Creds =>
        InvocationContext.AuthenticationCredentialsProviders.ToArray();

    protected async Task<T> UseClientAsync<T>(Func<FileTransferClient, Task<T>> action)
    {
        using var client = FileTransferClientFactory.Create(Creds);
        await client.ConnectAsync();
        return await client.ExecuteAsync(() => action(client));
    }

    protected async Task UseClientAsync(Func<FileTransferClient, Task> action) =>
        await UseClientAsync<bool>(async client =>
        {
            await action(client);
            return true;
        });

    protected T UseClient<T>(Func<FileTransferClient, T> action)
    {
        using var client = FileTransferClientFactory.Create(Creds);
        client.ConnectAsync().GetAwaiter().GetResult();
        return client.ExecuteAsync(() => Task.FromResult(action(client))).GetAwaiter().GetResult();
    }
}

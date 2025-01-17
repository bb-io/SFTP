using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Renci.SshNet.Common;

namespace Apps.SFTP.Invocables
{
    public class SFTPInvocable : BaseInvocable
    {
        protected AuthenticationCredentialsProvider[] Creds => InvocationContext.AuthenticationCredentialsProviders.ToArray();

        public SFTPInvocable(InvocationContext invocationContext) : base(invocationContext)
        {

        }


        //For sync methods
        protected T UseClient<T>(Func<BlackbirdSftpClient, T> action)
        {
            using var client = new BlackbirdSftpClient(Creds);
            try
            {
                return action(client);
            }
            catch (SshException ex)
            {
                throw new PluginApplicationException($"SFTP error: {ex.Message}", ex);
            }
        }


        //For async methods
        protected async Task<T> UseClientAsync<T>(Func<BlackbirdSftpClient, Task<T>> action)
        {
            using var client = new BlackbirdSftpClient(Creds);
            try
            {
                return await action(client);
            }
            catch (SshException ex)
            {
                throw new PluginApplicationException($"SFTP error: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new PluginApplicationException($"SFTP error: {ex.Message}", ex);
            }
        }
    }
}

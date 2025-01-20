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
            try
            {
                using var client = new BlackbirdSftpClient(Creds);
                var result = action(client);
                if (client.IsConnected)
                    client.Disconnect();
                return result;
            }
            catch (SshAuthenticationException ex)
            {
                throw new PluginMisconfigurationException($"The server denied access to the current connection credentials. Please update the connection.", ex);
            }
            catch (SftpPathNotFoundException ex)
            {
                throw new PluginMisconfigurationException($"The provided file or path does not exist on the server. Please make sure the path is valid.", ex);
            }
            catch (SshException ex)
            {
                throw new PluginApplicationException($"SFTP error: {ex.Message}", ex);
            }
        }


        //For async methods
        protected async Task<T> UseClientAsync<T>(Func<BlackbirdSftpClient, Task<T>> action)
        {            
            try
            {
                using var client = new BlackbirdSftpClient(Creds);
                var result = await action(client);
                if (client.IsConnected)
                    client.Disconnect();
                return result;
            }
            catch (SshAuthenticationException ex)
            {
                throw new PluginMisconfigurationException($"The server denied access to the current connection credentials. Please update the connection.", ex);
            }
            catch (SftpPathNotFoundException ex)
            {
                throw new PluginMisconfigurationException($"The provided file or path does not exist on the server. Please make sure the path is valid.", ex);
            }
            catch (SshException ex)
            {
                throw new PluginApplicationException($"SFTP error: {ex.Message}", ex);
            }
        }
    }
}

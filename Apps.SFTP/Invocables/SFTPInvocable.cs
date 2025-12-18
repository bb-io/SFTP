using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Renci.SshNet.Common;
using System.Diagnostics;

namespace Apps.SFTP.Invocables
{
    public class SFTPInvocable : BaseInvocable
    {
        protected AuthenticationCredentialsProvider[] Creds => InvocationContext.AuthenticationCredentialsProviders.ToArray();

        public SFTPInvocable(InvocationContext invocationContext) : base(invocationContext)
        {

        }


        // For sync methods
        protected T UseClient<T>(Func<BlackbirdSftpClient, T> action)
        {
            var (host, port, login) = GetSafeConnectionMeta();

            LogInfo("SFTP: starting. Host={0}, Port={1}, Login={2}", host, port, login);

            try
            {
                LogInfo("SFTP: creating client.");
                using var client = new BlackbirdSftpClient(Creds);

                LogInfo("SFTP: client created. IsConnected={0}", client.IsConnected);

                LogInfo("SFTP: configuring client. KeepAliveInterval={0}s, OperationTimeout={1}s", 30, 60);
                ConfigureClient(client);

                LogInfo("SFTP: connecting.");
                var connectSw = Stopwatch.StartNew();
                ConnectWithRetry(client, host, port, login);
                connectSw.Stop();

                LogInfo("SFTP: connected. IsConnected={0}, ConnectElapsedMs={1}", client.IsConnected, connectSw.ElapsedMilliseconds);

                LogInfo("SFTP: executing action...");
                var actionSw = Stopwatch.StartNew();
                var result = action(client);
                actionSw.Stop();

                LogInfo("SFTP: action finished. ElapsedMs={0}", actionSw.ElapsedMilliseconds);

                if (client.IsConnected)
                {
                    LogInfo("SFTP: disconnecting.");
                    client.Disconnect();
                    LogInfo("SFTP: disconnected.");
                }
                else
                {
                    LogInfo("SFTP: already disconnected before explicit disconnect.");
                }

                LogInfo("SFTP: finished successfully.");
                return result;
            }
            catch (SshAuthenticationException ex)
            {
                LogError("SFTP: authentication failed. Host={0}, Port={1}, Login={2}. Exception={3}",
                    host, port, login, ex.ToString());

                throw new PluginMisconfigurationException(
                    $"Authentication failed: {ex.Message}. The server denied access to the current connection credentials. Please verify and update your credentials.");
            }
            catch (SftpPathNotFoundException ex)
            {
                LogError("SFTP: path not found. Host={0}, Port={1}, Login={2}. Exception={3}",
                    host, port, login, ex.ToString());

                throw new PluginMisconfigurationException(
                    $"File or path not found: {ex.Message}. The specified file or directory does not exist on the server. Please check the provided path.");
            }
            catch (SshException ex)
            {
                LogError("SFTP: SSH/SFTP exception. Host={0}, Port={1}, Login={2}. Exception={3}",
                    host, port, login, ex.ToString());

                throw new PluginApplicationException($"SFTP/SSH connection error: {ex.Message}");
            }
            catch (Exception ex)
            {
                LogError("SFTP: unexpected error. Host={0}, Port={1}, Login={2}. Exception={3}",
                    host, port, login, ex.ToString());

                throw new PluginApplicationException(ex.Message);
            }
        }

        // For async methods
        protected async Task<T> UseClientAsync<T>(Func<BlackbirdSftpClient, Task<T>> action)
        {
            var (host, port, login) = GetSafeConnectionMeta();

            LogInfo("SFTP: starting (async). Host={0}, Port={1}, Login={2}", host, port, login);

            try
            {
                LogInfo("SFTP: creating client (async).");
                using var client = new BlackbirdSftpClient(Creds);

                LogInfo("SFTP: client created (async). IsConnected={0}", client.IsConnected);

                LogInfo("SFTP: configuring client (async). KeepAliveInterval={0}s, OperationTimeout={1}s", 30, 60);
                ConfigureClient(client);

                LogInfo("SFTP: connecting (async)...");
                var connectSw = Stopwatch.StartNew();
                ConnectWithRetry(client, host, port, login);
                connectSw.Stop();

                LogInfo("SFTP: connected (async). IsConnected={0}, ConnectElapsedMs={1}", client.IsConnected, connectSw.ElapsedMilliseconds);

                LogInfo("SFTP: executing action (async).");
                var actionSw = Stopwatch.StartNew();
                var result = await action(client);
                actionSw.Stop();

                LogInfo("SFTP: action finished (async). ElapsedMs={0}", actionSw.ElapsedMilliseconds);

                if (client.IsConnected)
                {
                    LogInfo("SFTP: disconnecting (async).");
                    client.Disconnect();
                    LogInfo("SFTP: disconnected (async).");
                }
                else
                {
                    LogInfo("SFTP: already disconnected before explicit disconnect (async).");
                }

                LogInfo("SFTP: finished successfully (async).");
                return result;
            }
            catch (SshAuthenticationException ex)
            {
                LogError("SFTP: authentication failed (async). Host={0}, Port={1}, Login={2}. Exception={3}",
                    host, port, login, ex.ToString());

                throw new PluginMisconfigurationException(
                    $"Authentication failed: {ex.Message}. The server denied access to the current connection credentials. Please verify and update your credentials.");
            }
            catch (SftpPathNotFoundException ex)
            {
                LogError("SFTP: path not found (async). Host={0}, Port={1}, Login={2}. Exception={3}",
                    host, port, login, ex.ToString());

                throw new PluginMisconfigurationException(
                    $"File or path not found: {ex.Message}. The specified file or directory does not exist on the server. Please check the provided path.");
            }
            catch (SshException ex)
            {
                LogError("SFTP: SSH/SFTP exception (async). Host={0}, Port={1}, Login={2}. Exception={3}",
                    host, port, login, ex.ToString());

                throw new PluginApplicationException($"SFTP/SSH connection error: {ex.Message}");
            }
            catch (Exception ex)
            {
                LogError("SFTP: unexpected error (async). Host={0}, Port={1}, Login={2}. Exception={3}",
                    host, port, login, ex.ToString());

                throw new PluginApplicationException(ex.Message);
            }
        }

        private static void ConfigureClient(BlackbirdSftpClient client)
        {
            client.KeepAliveInterval = TimeSpan.FromSeconds(30);
            client.OperationTimeout = TimeSpan.FromSeconds(60);
        }

        private void ConnectWithRetry(BlackbirdSftpClient client, string? host, string? port, string? login)
        {
            try
            {
                LogInfo("SFTP: Connect attempt 1. Host={0}, Port={1}, Login={2}", host, port, login);
                client.Connect();
                LogInfo("SFTP: Connect attempt 1 succeeded. IsConnected={0}", client.IsConnected);
            }
            catch (Exception ex) when (IsHandshakeTransient(ex))
            {
                LogWarning("SFTP: Connect attempt 1 failed (transient). Host={0}, Port={1}, Login={2}. Will retry after {3}ms. Exception={4}",
                    host, port, login, 2000, ex.ToString());

                Thread.Sleep(2000);

                LogInfo("SFTP: Connect attempt 2. Host={0}, Port={1}, Login={2}", host, port, login);
                client.Connect();
                LogInfo("SFTP: Connect attempt 2 succeeded. IsConnected={0}", client.IsConnected);
            }
        }

        private static bool IsHandshakeTransient(Exception ex)
        {
            var msg = ex.ToString();
            return ex is SshConnectionException
                || msg.Contains("does not contain an SSH identification string", StringComparison.OrdinalIgnoreCase)
                || msg.Contains("An established connection was aborted by the server", StringComparison.OrdinalIgnoreCase);
        }

        private (string? host, string? port, string? login) GetSafeConnectionMeta()
        {
            string? host = Creds.FirstOrDefault(p => p.KeyName == "host")?.Value;
            string? port = Creds.FirstOrDefault(p => p.KeyName == "port")?.Value;
            string? login = Creds.FirstOrDefault(p => p.KeyName == "login")?.Value;
            return (host, port, login);
        }

        private void LogInfo(string message, params object[] args)
            => InvocationContext.Logger?.LogInformation?.Invoke(message, args);

        private void LogWarning(string message, params object[] args)
            => InvocationContext.Logger?.LogWarning?.Invoke(message, args);

        private void LogError(string message, params object[] args)
            => InvocationContext.Logger?.LogError?.Invoke(message, args);
    }
}

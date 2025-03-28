using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Renci.SshNet;
using Renci.SshNet.Common;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace Apps.SFTP;

public class BlackbirdSftpClient : SftpClient
{
    private static ConnectionInfo GetConnectionInfo(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders)
    {
        var host = authenticationCredentialsProviders.First(p => p.KeyName == "host").Value;
        var port = authenticationCredentialsProviders.First(p => p.KeyName == "port").Value;
        var login = authenticationCredentialsProviders.First(p => p.KeyName == "login").Value;
        var password = authenticationCredentialsProviders.First(p => p.KeyName == "password").Value;

        if (!password.Contains("PRIVATE KEY"))
        {
            return new ConnectionInfo(host, Int32.Parse(port), login, new PasswordAuthenticationMethod(login, password));
        }
        var bytes = Encoding.UTF8.GetBytes(password);
        var key = new PrivateKeyFile(new MemoryStream(bytes));
        return new ConnectionInfo(host, Int32.Parse(port), login, new PrivateKeyAuthenticationMethod(login, key));
    }

    public BlackbirdSftpClient(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders) : base(GetConnectionInfo(authenticationCredentialsProviders))
    {
        try
        {
            this.BufferSize = 32 * 1024 * 12;
            this.Connect();
        }
        catch (SshAuthenticationException ex)
        {
            throw new PluginMisconfigurationException($"Authentication failed: {ex.Message}. The server denied access to the current connection credentials. Please verify and update your credentials.");
        }
        catch (SftpPathNotFoundException ex)
        {
            throw new PluginMisconfigurationException($"File or path not found: {ex.Message}. The specified file or directory does not exist on the server. Please check the provided path.");
        }
        catch (SshException ex)
        {
            throw new PluginApplicationException($"{ex.Message} - Make sure the target path is correct.");
        }
        catch (Exception ex)
        {
            throw new PluginApplicationException(ex.Message);
        }

    }
}
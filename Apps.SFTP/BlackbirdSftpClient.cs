using Blackbird.Applications.Sdk.Common.Authentication;
using Renci.SshNet;
using static System.Net.Mime.MediaTypeNames;
using System.Text;

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
        this.BufferSize = 32 * 1024 * 12;
        this.Connect();
    }
}
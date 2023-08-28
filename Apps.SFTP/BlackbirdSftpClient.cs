using Blackbird.Applications.Sdk.Common.Authentication;
using Renci.SshNet;

namespace Apps.SFTP;

public class BlackbirdSftpClient : SftpClient
{
    private static ConnectionInfo GetConnectionInfo(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders)
    {
        var host = authenticationCredentialsProviders.First(p => p.KeyName == "host").Value;
        var port = authenticationCredentialsProviders.First(p => p.KeyName == "port").Value;
        var login = authenticationCredentialsProviders.First(p => p.KeyName == "login").Value;
        var password = authenticationCredentialsProviders.First(p => p.KeyName == "password").Value;

        return new ConnectionInfo(host, Int32.Parse(port), login, new PasswordAuthenticationMethod(login, password));
    }

    public BlackbirdSftpClient(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders) : base(GetConnectionInfo(authenticationCredentialsProviders))
    {
        this.Connect();
    }
}
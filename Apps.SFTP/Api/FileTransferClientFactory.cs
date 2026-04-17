using Apps.SFTP.Api.Ftp;
using Apps.SFTP.Api.Sftp;
using Apps.SFTP.Constants;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Exceptions;

namespace Apps.SFTP.Api;

public static class FileTransferClientFactory
{
    public static FileTransferClient Create(IEnumerable<AuthenticationCredentialsProvider> creds)
    {
        var credArray = creds as AuthenticationCredentialsProvider[] ?? creds.ToArray();
        var connectionType = credArray.First(p => p.KeyName == CredNames.ConnectionType).Value;

        return connectionType switch
        {
            ConnectionTypes.Sftp => new BlackbirdSftpClient(credArray),
            ConnectionTypes.Ftp  => new BlackbirdFtpClient(credArray),
            _ => throw new PluginMisconfigurationException(
                $"Unsupported connection type: '{connectionType}'. Expected '{ConnectionTypes.Sftp}' or '{ConnectionTypes.Ftp}'.")
        };
    }
}
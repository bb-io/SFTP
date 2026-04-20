using Apps.SFTP.Constants;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Connections;

namespace Apps.SFTP.Connections;

public class ConnectionDefinition : IConnectionDefinition
{
    public IEnumerable<ConnectionPropertyGroup> ConnectionPropertyGroups => new List<ConnectionPropertyGroup>()
    {
        new()
        {
            Name = ConnectionTypes.Sftp,
            DisplayName = "SFTP",
            AuthenticationType = ConnectionAuthenticationType.Undefined,
            ConnectionProperties = new List<ConnectionProperty>()
            {
                new(CredNames.Host)
                {
                    DisplayName = "Host"
                },
                new(CredNames.Port)
                {
                    DisplayName = "Port"
                },
                new(CredNames.Login)
                {
                    DisplayName = "Username"
                },
                new(CredNames.Password)
                {
                    DisplayName = "Password or Private Key",
                    Sensitive = true
                }
            }
        },
        new() 
        {
            Name = ConnectionTypes.Ftp,
            DisplayName = "FTP",
            AuthenticationType = ConnectionAuthenticationType.Undefined,
            ConnectionProperties = new List<ConnectionProperty>()
            {
                new(CredNames.Host)
                {
                    DisplayName = "Host"
                },
                new(CredNames.Port)
                {
                    DisplayName = "Port"
                },
                new(CredNames.Login)
                {
                    DisplayName = "Username"
                },
                new(CredNames.Password)
                {
                    DisplayName = "Password",
                    Sensitive = true
                }
            }
        }
    };

    public IEnumerable<AuthenticationCredentialsProvider> CreateAuthorizationCredentialsProviders(Dictionary<string, string> values)
    {
        var providers = values.Select(x => new AuthenticationCredentialsProvider(x.Key, x.Value)).ToList();

        var connectionType = values[nameof(ConnectionPropertyGroup)] switch
        {
            var ct when ConnectionTypes.SupportedConnectionTypes.Contains(ct) => ct,
            _ => throw new Exception($"Unknown connection type: {values[nameof(ConnectionPropertyGroup)]}")
        };

        providers.Add(new AuthenticationCredentialsProvider(CredNames.ConnectionType, connectionType));
        return providers;
    }
}
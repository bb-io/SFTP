using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Connections;

namespace Apps.SFTP.Connections;

public class ConnectionDefinition : IConnectionDefinition
{
    public IEnumerable<ConnectionPropertyGroup> ConnectionPropertyGroups => new List<ConnectionPropertyGroup>()
    {
        new ConnectionPropertyGroup
        {
            Name = "SFTP information",
            AuthenticationType = ConnectionAuthenticationType.Undefined,
            ConnectionProperties = new List<ConnectionProperty>()
            {
                new("host"){ DisplayName = "Host" },
                new("port"){ DisplayName = "Port"},
                new("login"){ DisplayName = "Username"},
                new("password"){ DisplayName = "Password or Private Key", Sensitive = true },
            }
        }
    };

    public IEnumerable<AuthenticationCredentialsProvider> CreateAuthorizationCredentialsProviders(Dictionary<string, string> values)
    {
        var host = values.First(v => v.Key == "host");
        yield return new AuthenticationCredentialsProvider(
            host.Key,
            host.Value
        );

        var port = values.First(v => v.Key == "port");
        yield return new AuthenticationCredentialsProvider(
            port.Key,
            port.Value
        );

        var login = values.First(v => v.Key == "login");
        yield return new AuthenticationCredentialsProvider(
            login.Key,
            login.Value
        );

        var password = values.First(v => v.Key == "password");
        yield return new AuthenticationCredentialsProvider(
            password.Key,
            password.Value
        );
    }
}
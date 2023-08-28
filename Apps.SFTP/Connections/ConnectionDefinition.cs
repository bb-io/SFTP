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
            ConnectionUsage = ConnectionUsage.Actions,
            ConnectionProperties = new List<ConnectionProperty>()
            {
                new ConnectionProperty("host"),
                new ConnectionProperty("port"),
                new ConnectionProperty("login"),
                new ConnectionProperty("password")
            }
        }
    };

    public IEnumerable<AuthenticationCredentialsProvider> CreateAuthorizationCredentialsProviders(Dictionary<string, string> values)
    {
        var host = values.First(v => v.Key == "host");
        yield return new AuthenticationCredentialsProvider(
            AuthenticationCredentialsRequestLocation.None,
            host.Key,
            host.Value
        );

        var port = values.First(v => v.Key == "port");
        yield return new AuthenticationCredentialsProvider(
            AuthenticationCredentialsRequestLocation.None,
            port.Key,
            port.Value
        );

        var login = values.First(v => v.Key == "login");
        yield return new AuthenticationCredentialsProvider(
            AuthenticationCredentialsRequestLocation.None,
            login.Key,
            login.Value
        );

        var password = values.First(v => v.Key == "password");
        yield return new AuthenticationCredentialsProvider(
            AuthenticationCredentialsRequestLocation.None,
            password.Key,
            password.Value
        );
    }
}
using Blackbird.Applications.Sdk.Common.Authentication;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.SFTP
{
    public class BlackbirdScpClient : ScpClient
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

        public BlackbirdScpClient(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders) : base(GetConnectionInfo(authenticationCredentialsProviders))
        {
            this.Connect();
        }
    }
}

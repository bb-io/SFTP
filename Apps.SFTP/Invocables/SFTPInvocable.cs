using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.SFTP.Invocables
{
    public class SFTPInvocable : BaseInvocable
    {
        protected AuthenticationCredentialsProvider[] Creds => InvocationContext.AuthenticationCredentialsProviders.ToArray();

        public SFTPInvocable(InvocationContext invocationContext) : base(invocationContext)
        {

        }
    }
}

using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Connections;

namespace Apps.SFTP.Connections;

public class ConnectionValidator : IConnectionValidator
{
    public async ValueTask<ConnectionValidationResponse> ValidateConnection(
        IEnumerable<AuthenticationCredentialsProvider> authProviders, CancellationToken cancellationToken)
    {
        try
        {
            using var client = new BlackbirdSftpClient(authProviders);

            if (client.IsConnected)
                return new ConnectionValidationResponse
                {
                    IsValid = true
                };

            return new ConnectionValidationResponse
            {
                IsValid = false,
                Message = "Failed to connect. Please check your connection parameters."
            };
        } catch(FormatException ex)
        {
            return new ConnectionValidationResponse()
            {
                IsValid = false,
                Message = ex.Message.Split("string ").Last(),
            };
        } catch(Exception ex)
        {
            return new ConnectionValidationResponse()
            {
                IsValid = false,
                Message = ex.Message,
            };
        }
        

    }
}
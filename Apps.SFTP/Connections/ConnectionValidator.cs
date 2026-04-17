using Apps.SFTP.Api;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Connections;

namespace Apps.SFTP.Connections;

public class ConnectionValidator : IConnectionValidator
{
    public async ValueTask<ConnectionValidationResponse> ValidateConnection(
        IEnumerable<AuthenticationCredentialsProvider> authProviders,
        CancellationToken cancellationToken)
    {
        try
        {
            using var client = FileTransferClientFactory.Create(authProviders);
            await client.ConnectAsync(cancellationToken);
            if (!client.IsConnected)
            {
                return new ConnectionValidationResponse
                {
                    IsValid = false,
                    Message = "Failed to connect. Please check your connection parameters."
                };
            }

            return new ConnectionValidationResponse { IsValid = true };
        }
        catch (FormatException ex)
        {
            return new ConnectionValidationResponse
            {
                IsValid = false,
                Message = ex.Message.Split("string ").Last(),
            };
        }
        catch (Exception ex)
        {
            return new ConnectionValidationResponse
            {
                IsValid = false,
                Message = ex.Message,
            };
        }
    }
}

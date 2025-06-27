using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.SDK.Blueprints.Interfaces.FileStorage;
namespace Apps.SFTP.Models.Responses;

public class DownloadFileResponse : IDownloadFileOutput
{
    public FileReference File { get; set; }
}
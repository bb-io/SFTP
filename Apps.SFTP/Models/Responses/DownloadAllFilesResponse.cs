using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.SFTP.Models.Responses
{
    public class DownloadAllFilesResponse
    {
        [Display("Downloaded files")]
        public IEnumerable<FileReference> Files { get; set; }
    }
}

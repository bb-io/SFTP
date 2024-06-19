
using Blackbird.Applications.Sdk.Common;

namespace Apps.SFTP.Webhooks.Payload
{
    public class ParentFolderInput
    {
        [Display("Parent folder path")]
        public string? Folder { get; set; }

        [Display("Include subfolders")]
        public bool? IncludeSubfolders { get; set; }
    }
}

using Apps.SFTP.Dtos;

namespace Apps.SFTP.Webhooks.Payload
{
    public class ChangedFilesResponse
    {
        public List<DirectoryItemDto> Files { get; set; }
    }
}

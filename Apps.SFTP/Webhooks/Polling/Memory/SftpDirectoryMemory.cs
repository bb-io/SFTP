namespace Apps.SFTP.Webhooks.Polling.Memory;

public class SftpDirectoryMemory
{
    public List<string> DirectoriesState { get; set; } = new();
}
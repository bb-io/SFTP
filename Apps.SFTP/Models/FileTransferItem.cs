namespace Apps.SFTP.Models;

public class FileTransferItem
{
    public string Name { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public bool IsFile { get; init; }
    public bool IsDirectory { get; init; }
    public DateTime LastModified { get; init; }
    public long? Size { get; init; }
}

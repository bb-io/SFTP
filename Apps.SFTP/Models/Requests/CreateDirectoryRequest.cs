namespace Apps.SFTP.Models.Requests;

public class CreateDirectoryRequest
{
    public string DirectoryName { get; set; }

    public string Path { get; set; }
}
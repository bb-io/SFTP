namespace Apps.SFTP.Models.Responses;

public class GetFileInformationResponse
{
    public long Size { get; set; }

    public string Path { get; set; }
}
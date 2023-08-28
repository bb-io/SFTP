namespace Apps.SFTP.Models.Requests;

public class RenameFileRequest
{
    public string OldPath { get; set; }

    public string NewPath { get; set; }
}
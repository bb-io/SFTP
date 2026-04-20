namespace Apps.SFTP.Constants;

public static class ConnectionTypes
{
    public const string Sftp = "SFTP information";
    public const string Ftp = "FTP information";
    
    public static List<string> SupportedConnectionTypes => [Sftp, Ftp];
}
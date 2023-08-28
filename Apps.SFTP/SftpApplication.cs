using Blackbird.Applications.Sdk.Common;

namespace Apps.SFTP;

public class SftpApplication : IApplication
{
    public string Name
    {
        get => "SFTP";
        set { }
    }

    public T GetInstance<T>()
    {
        throw new NotImplementedException();
    }
}
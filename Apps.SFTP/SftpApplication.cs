using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Metadata;

namespace Apps.SFTP;

public class SftpApplication : IApplication, ICategoryProvider
{
    
    public IEnumerable<ApplicationCategory> Categories
    {
        get => [ApplicationCategory.FileManagementAndStorage];
        set { }
    }
    
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
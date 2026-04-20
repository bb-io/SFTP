using System.Net;
using Apps.SFTP.Constants;
using Apps.SFTP.Models;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Exceptions;
using FluentFTP;
using FluentFTP.Exceptions;

namespace Apps.SFTP.Api.Ftp;

public class BlackbirdFtpClient : FileTransferClient
{
    private readonly AsyncFtpClient _client;
    private bool _disposed;

    public override bool IsConnected => _client.IsConnected;

    public BlackbirdFtpClient(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders)
    {
        var credentialsProviders = authenticationCredentialsProviders as AuthenticationCredentialsProvider[]
            ?? authenticationCredentialsProviders.ToArray();

        var host = credentialsProviders.First(p => p.KeyName == CredNames.Host).Value;
        var port = credentialsProviders.First(p => p.KeyName == CredNames.Port).Value;
        var login = credentialsProviders.First(p => p.KeyName == CredNames.Login).Value;
        var password = credentialsProviders.First(p => p.KeyName == CredNames.Password).Value;

        _client = new AsyncFtpClient
        {
            Host = host,
            Port = Convert.ToInt32(port),
            Credentials = new NetworkCredential(login, password)
        };

        _client.Config.EncryptionMode = FtpEncryptionMode.Explicit;
        _client.Config.ValidateAnyCertificate = true;
    }

    public override async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        if (_client.IsConnected)
        {
            return;
        }

        try
        {
            await _client.Connect(cancellationToken);
        }
        catch (Exception ex)
        {
            throw TranslateException(ex);
        }
    }

    public override void Disconnect()
    {
        if (_client.IsConnected)
        {
            _client.Disconnect().GetAwaiter().GetResult();
        }
    }

    public override async Task UploadAsync(Stream stream, string remotePath)
    {
        await _client.UploadStream(stream, remotePath);
    }

    public override async Task DownloadAsync(string remotePath, Stream destination)
    {
        await _client.DownloadStream(destination, remotePath);
    }

    public override async Task DeleteFileAsync(string path)
    {
        await _client.DeleteFile(path);
    }

    public override async Task RenameAsync(string oldPath, string newPath)
    {
        await _client.Rename(oldPath, newPath);
    }

    public override async Task CreateDirectoryAsync(string path)
    {
        await _client.CreateDirectory(path);
    }

    public override async Task DeleteDirectoryAsync(string path)
    {
        await _client.DeleteDirectory(path);
    }

    public override async Task<IEnumerable<FileTransferItem>> ListDirectoryAsync(string path, bool recursive = false)
    {
        var items = await _client.GetListing(path, recursive ? FtpListOption.Recursive : FtpListOption.Auto);
        return items.Select(MapItem).ToList();
    }

    public override async Task<FileTransferItem> GetFileInfoAsync(string path)
    {
        var item = await _client.GetObjectInfo(path, true);
        if (item is null)
        {
            throw new PluginMisconfigurationException($"File or path not found: {path}");
        }

        return MapItem(item);
    }

    protected override Exception TranslateException(Exception ex) => ex switch
    {
        FtpAuthenticationException => new PluginMisconfigurationException($"Authentication failed: {ex.Message}"),
        FtpCommandException commandException when commandException.CompletionCode == "550"
            => new PluginMisconfigurationException($"File or path not found: {ex.Message}"),
        FtpException => new PluginApplicationException($"FTP error: {ex.Message}"),
        _ => new PluginApplicationException(ex.Message)
    };

    public override void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        if (_client.IsConnected)
        {
            Disconnect();
        }

        _client.Dispose();
        _disposed = true;
    }

    private static FileTransferItem MapItem(FtpListItem item) => new()
    {
        Name = item.Name,
        FullName = item.FullName,
        IsFile = item.Type == FtpObjectType.File,
        IsDirectory = item.Type == FtpObjectType.Directory,
        LastModified = item.Modified,
        Size = item.Type == FtpObjectType.File ? item.Size : null
    };
}

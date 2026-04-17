using System.Text;
using Apps.SFTP.Constants;
using Apps.SFTP.Models;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Renci.SshNet;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;

namespace Apps.SFTP.Api.Sftp;

public class BlackbirdSftpClient : FileTransferClient
{
    private readonly SftpClient _client;
    private bool _disposed;

    public override bool IsConnected => _client.IsConnected;

    public BlackbirdSftpClient(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders)
    {
        _client = new SftpClient(GetConnectionInfo(authenticationCredentialsProviders))
        {
            BufferSize = 32 * 1024 * 12,
            KeepAliveInterval = TimeSpan.FromSeconds(30),
            OperationTimeout = TimeSpan.FromSeconds(60)
        };
    }

    public override async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        if (_client.IsConnected)
        {
            return;
        }

        try
        {
            await ConnectWithRetryAsync(cancellationToken);
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
            _client.Disconnect();
        }
    }

    public override Task UploadAsync(Stream stream, string remotePath) =>
        Task.Run(() => _client.UploadFile(stream, remotePath));

    public override Task DownloadAsync(string remotePath, Stream destination) =>
        Task.Run(() => _client.DownloadFile(remotePath, destination));

    public override Task DeleteFileAsync(string path) =>
        Task.Run(() => _client.DeleteFile(path));

    public override Task RenameAsync(string oldPath, string newPath) =>
        Task.Run(() => _client.RenameFile(oldPath, newPath));

    public override Task CreateDirectoryAsync(string path) =>
        Task.Run(() => _client.CreateDirectory(path));

    public override Task DeleteDirectoryAsync(string path) =>
        Task.Run(() => _client.DeleteDirectory(path));

    public override Task<IEnumerable<FileTransferItem>> ListDirectoryAsync(string path, bool recursive = false) =>
        Task.Run<IEnumerable<FileTransferItem>>(() => ListDirectoryInternal(path, recursive));

    public override Task<FileTransferItem> GetFileInfoAsync(string path) =>
        Task.Run(() =>
        {
            var attributes = _client.GetAttributes(path);
            return new FileTransferItem
            {
                Name = GetName(path),
                FullName = path,
                IsFile = !attributes.IsDirectory,
                IsDirectory = attributes.IsDirectory,
                LastModified = attributes.LastWriteTime,
                Size = attributes.IsDirectory ? null : attributes.Size
            };
        });

    protected override Exception TranslateException(Exception ex) => ex switch
    {
        SshAuthenticationException => new PluginMisconfigurationException($"Authentication failed: {ex.Message}"),
        SftpPathNotFoundException => new PluginMisconfigurationException($"File or path not found: {ex.Message}"),
        SshException => new PluginApplicationException($"SFTP/SSH connection error: {ex.Message}"),
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

    private static ConnectionInfo GetConnectionInfo(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders)
    {
        var credentialsProviders = authenticationCredentialsProviders as AuthenticationCredentialsProvider[]
            ?? authenticationCredentialsProviders.ToArray();

        var host = credentialsProviders.First(p => p.KeyName == CredNames.Host).Value;
        var port = credentialsProviders.First(p => p.KeyName == CredNames.Port).Value;
        var login = credentialsProviders.First(p => p.KeyName == CredNames.Login).Value;
        var password = credentialsProviders.First(p => p.KeyName == CredNames.Password).Value;

        if (!password.Contains("PRIVATE KEY", StringComparison.Ordinal))
        {
            return new ConnectionInfo(host, int.Parse(port), login, new PasswordAuthenticationMethod(login, password));
        }

        var bytes = Encoding.UTF8.GetBytes(password);
        var key = new PrivateKeyFile(new MemoryStream(bytes));
        return new ConnectionInfo(host, int.Parse(port), login, new PrivateKeyAuthenticationMethod(login, key));
    }

    private async Task ConnectWithRetryAsync(CancellationToken cancellationToken)
    {
        try
        {
            await Task.Run(_client.Connect, cancellationToken);
        }
        catch (Exception ex) when (IsHandshakeTransient(ex))
        {
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
            await Task.Run(_client.Connect, cancellationToken);
        }
    }

    private List<FileTransferItem> ListDirectoryInternal(string path, bool recursive)
    {
        var items = _client.ListDirectory(path)
            .Where(IsVisibleEntry)
            .Select(MapItem)
            .ToList();

        if (!recursive)
        {
            return items;
        }

        var nestedItems = items
            .Where(x => x.IsDirectory)
            .SelectMany(x => ListDirectoryInternal(x.FullName, true))
            .ToList();

        return items.Concat(nestedItems).ToList();
    }

    private static FileTransferItem MapItem(ISftpFile file) => new()
    {
        Name = file.Name,
        FullName = file.FullName,
        IsFile = file.IsRegularFile,
        IsDirectory = file.IsDirectory,
        LastModified = file.LastWriteTime,
        Size = file.IsDirectory ? null : file.Attributes.Size
    };

    private static bool IsVisibleEntry(ISftpFile file) => file.Name is not "." and not "..";

    private static bool IsHandshakeTransient(Exception ex)
    {
        var message = ex.ToString();
        return ex is SshConnectionException
               || message.Contains("does not contain an SSH identification string", StringComparison.OrdinalIgnoreCase)
               || message.Contains("An established connection was aborted by the server", StringComparison.OrdinalIgnoreCase);
    }

    private static string GetName(string path)
    {
        var normalizedPath = path.Replace('\\', '/').TrimEnd('/');
        var lastSlashIndex = normalizedPath.LastIndexOf('/');
        return lastSlashIndex >= 0 ? normalizedPath[(lastSlashIndex + 1)..] : normalizedPath;
    }
}

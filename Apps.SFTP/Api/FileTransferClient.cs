using Apps.SFTP.Models;
using Blackbird.Applications.Sdk.Common.Exceptions;

namespace Apps.SFTP.Api;

public abstract class FileTransferClient : IDisposable
{
    public abstract Task ConnectAsync(CancellationToken cancellationToken = default);
    public abstract void Disconnect();
    public abstract bool IsConnected { get; }

    public abstract Task UploadAsync(Stream stream, string remotePath);
    public abstract Task DownloadAsync(string remotePath, Stream destination);
    public abstract Task DeleteFileAsync(string path);
    public abstract Task RenameAsync(string oldPath, string newPath);
    public abstract Task CreateDirectoryAsync(string path);
    public abstract Task DeleteDirectoryAsync(string path);
    public abstract Task<IEnumerable<FileTransferItem>> ListDirectoryAsync(string path, bool recursive = false);

    public virtual Task<FileTransferItem> GetFileInfoAsync(string path) =>
        throw new NotSupportedException($"GetFileInfo is not supported by {GetType().Name}");

    protected abstract Exception TranslateException(Exception ex);

    public async Task<T> ExecuteAsync<T>(Func<Task<T>> action)
    {
        try
        {
            return await action();
        }
        catch (PluginMisconfigurationException)
        {
            throw;
        }
        catch (PluginApplicationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw TranslateException(ex);
        }
    }

    public async Task ExecuteAsync(Func<Task> action) =>
        await ExecuteAsync<bool>(async () =>
        {
            await action();
            return true;
        });

    public abstract void Dispose();
}

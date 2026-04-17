using Apps.SFTP.Api;
using Apps.SFTP.Dtos;
using Apps.SFTP.Invocables;
using Apps.SFTP.Models;
using Apps.SFTP.Models.Responses;
using Apps.SFTP.Webhooks.Payload;
using Apps.SFTP.Webhooks.Polling.Memory;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common.Polling;

namespace Apps.SFTP.Webhooks;

[PollingEventList("Folders")]
public class FolderPollingList(InvocationContext invocationContext) : FileTransferInvocable(invocationContext)
{
    [PollingEvent("On folders created", Description = "Triggers when folders are created")]
    public async Task<PollingEventResponse<SftpDirectoryMemory, ListDirectoryResponse>> OnDirectoriesCreated(
        PollingEventRequest<SftpDirectoryMemory> request,
        [PollingEventParameter] ParentFolderInput parentFolder)
    {
        var directories = await ListFoldersAsync(parentFolder.Folder ?? "/", parentFolder.IncludeSubfolders ?? false);
        var directoryState = directories.Select(x => x.FullName).ToList();

        if (request.Memory == null)
        {
            return new()
            {
                FlyBird = false,
                Memory = new SftpDirectoryMemory { DirectoriesState = directoryState }
            };
        }

        var newItems = directoryState.Except(request.Memory.DirectoriesState).ToList();
        if (newItems.Count == 0)
        {
            return new()
            {
                FlyBird = false,
                Memory = new SftpDirectoryMemory { DirectoriesState = directoryState }
            };
        }

        return new()
        {
            FlyBird = true,
            Memory = new SftpDirectoryMemory { DirectoriesState = directoryState },
            Result = new ListDirectoryResponse
            {
                DirectoriesItems = directories
                    .Where(x => newItems.Contains(x.FullName))
                    .Select(x => new DirectoryItemDto
                    {
                        Name = x.Name,
                        FileId = x.FullName
                    })
                    .ToList()
            }
        };
    }

    private async Task<List<FileTransferItem>> ListFoldersAsync(string folderPath, bool includeSubfolders)
    {
        using var client = FileTransferClientFactory.Create(Creds);
        await client.ConnectAsync();
        var all = await client.ExecuteAsync(async () => (await client.ListDirectoryAsync(folderPath, includeSubfolders)).ToList());
        return all.Where(x => x.IsDirectory).ToList();
    }
}

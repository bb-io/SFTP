using Apps.SFTP.Dtos;
using Apps.SFTP.Invocables;
using Apps.SFTP.Models;
using Apps.SFTP.Webhooks.Payload;
using Apps.SFTP.Webhooks.Polling.Memory;
using Blackbird.Applications.SDK.Blueprints;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common.Polling;

namespace Apps.SFTP.Webhooks;

[PollingEventList("Files")]
public class PollingList(InvocationContext invocationContext) : FileTransferInvocable(invocationContext)
{
    [BlueprintEventDefinition(BlueprintEvent.FilesCreatedOrUpdated)]
    [PollingEvent("On files updated", "Triggered when files are updated or new files are created")]
    public async Task<PollingEventResponse<SFTPMemory, ChangedFilesResponse>> OnFilesAddedOrUpdated(
        PollingEventRequest<SFTPMemory> request,
        [PollingEventParameter] ParentFolderInput parentFolder)
    {
        var filesInfo = await ListFilesAsync(parentFolder.Folder ?? "/", parentFolder.IncludeSubfolders ?? true);
        var newFilesState = filesInfo.Select(x => $"{x.FullName}|{x.LastModified}").ToList();

        if (request.Memory == null)
        {
            return new()
            {
                FlyBird = false,
                Memory = new SFTPMemory { FilesState = newFilesState }
            };
        }

        var changedItems = newFilesState.Except(request.Memory.FilesState).ToList();
        if (changedItems.Count == 0)
        {
            return new()
            {
                FlyBird = false,
                Memory = new SFTPMemory { FilesState = newFilesState }
            };
        }

        var changedFilesPath = changedItems.Select(x => x.Split('|').First()).ToList();
        return new()
        {
            FlyBird = true,
            Memory = new SFTPMemory { FilesState = newFilesState },
            Result = new ChangedFilesResponse
            {
                Files = filesInfo
                    .Where(x => changedFilesPath.Contains(x.FullName))
                    .Select(x => new DirectoryItemDto { Name = x.Name, FileId = x.FullName })
                    .ToList()
            }
        };
    }

    [PollingEvent("On files deleted", "Triggered when files are deleted")]
    public async Task<PollingEventResponse<SFTPMemory, ChangedFilesResponse>> OnFilesDeleted(
        PollingEventRequest<SFTPMemory> request,
        [PollingEventParameter] ParentFolderInput parentFolder)
    {
        var filesInfo = await ListFilesAsync(parentFolder.Folder ?? "/", parentFolder.IncludeSubfolders ?? true);
        var newFilesState = filesInfo.Select(x => x.FullName).ToList();

        if (request.Memory == null)
        {
            return new()
            {
                FlyBird = false,
                Memory = new SFTPMemory { FilesState = newFilesState }
            };
        }

        var deletedItems = request.Memory.FilesState.Except(newFilesState).ToList();
        if (deletedItems.Count == 0)
        {
            return new()
            {
                FlyBird = false,
                Memory = new SFTPMemory { FilesState = newFilesState }
            };
        }

        return new()
        {
            FlyBird = true,
            Memory = new SFTPMemory { FilesState = newFilesState },
            Result = new ChangedFilesResponse
            {
                Files = deletedItems
                    .Select(x => new DirectoryItemDto { Name = Path.GetFileName(x), FileId = x })
                    .ToList()
            }
        };
    }

    private async Task<List<FileTransferItem>> ListFilesAsync(string folderPath, bool includeSubfolders)
    {
        return await ListDirectoryItemsAsync(folderPath, includeSubfolders, x => x.IsFile);
    }
}

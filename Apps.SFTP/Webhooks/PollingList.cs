using Apps.SFTP.Dtos;
using Apps.SFTP.Invocables;
using Apps.SFTP.Webhooks.Payload;
using Apps.SFTP.Webhooks.Polling.Memory;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common.Polling;
using Blackbird.Applications.SDK.Blueprints;
using Renci.SshNet.Sftp;

namespace Apps.SFTP.Webhooks
{
    [PollingEventList("Files")]
    public class PollingList(InvocationContext invocationContext) : SFTPInvocable(invocationContext)
    {
        [BlueprintEventDefinition(BlueprintEvent.FilesCreatedOrUpdated)]
        [PollingEvent("On files updated", "Triggered when files are updated or new files are created")]
        public async Task<PollingEventResponse<SFTPMemory, ChangedFilesResponse>> OnFilesAddedOrUpdated(
            PollingEventRequest<SFTPMemory> request,
            [PollingEventParameter] ParentFolderInput parentFolder
            )
        {
            try
            {
                using var client = new BlackbirdSftpClient(Creds);
                var filesInfo = ListDirectoryFiles(client, parentFolder.Folder ?? "/", parentFolder.IncludeSubfolders ?? true);
                var newFilesState = filesInfo.Select(x => $"{x.FullName}|{x.LastWriteTime}").ToList();
                if (request.Memory == null)
                {
                    return new()
                    {
                        FlyBird = false,
                        Memory = new SFTPMemory() { FilesState = newFilesState }
                    };
                }
                var changedItems = newFilesState.Except(request.Memory.FilesState).ToList();
                if (changedItems.Count == 0)
                    return new()
                    {
                        FlyBird = false,
                        Memory = new SFTPMemory() { FilesState = newFilesState }
                    };
                var changedFilesPath = changedItems.Select(x => x.Split('|').First()).ToList();
                return new()
                {
                    FlyBird = true,
                    Memory = new SFTPMemory() { FilesState = newFilesState },
                    Result = new ChangedFilesResponse() { Files = filesInfo.Where(x => changedFilesPath.Contains(x.FullName)).Select(x => new DirectoryItemDto() { Name = x.Name, FileId = x.FullName }).ToList() }
                };
            }
            catch (Exception ex)
            {
                throw new PluginApplicationException(ex.Message);
            }
        }

        [PollingEvent("On files deleted", "Triggered when files are deleted")]
        public async Task<PollingEventResponse<SFTPMemory, ChangedFilesResponse>> OnFilesDeleted(
            PollingEventRequest<SFTPMemory> request,
            [PollingEventParameter] ParentFolderInput parentFolder
            )
        {
            try
            {
                using var client = new BlackbirdSftpClient(Creds);
                var filesInfo = ListDirectoryFiles(client, parentFolder.Folder ?? "/", parentFolder.IncludeSubfolders ?? true);
                var newFilesState = filesInfo.Select(x => $"{x.FullName}").ToList();
                if (request.Memory == null)
                {
                    return new()
                    {
                        FlyBird = false,
                        Memory = new SFTPMemory() { FilesState = newFilesState }
                    };
                }
                var deletedItems = request.Memory.FilesState.Except(newFilesState).ToList();
                if (deletedItems.Count == 0)
                    return new()
                    {
                        FlyBird = false,
                        Memory = new SFTPMemory() { FilesState = newFilesState }
                    };
                return new()
                {
                    FlyBird = true,
                    Memory = new SFTPMemory() { FilesState = newFilesState },
                    Result = new ChangedFilesResponse() { Files = deletedItems.Select(x => new DirectoryItemDto() { Name = Path.GetFileName(x), FileId = x }).ToList() }
                };
            }
            catch (Exception ex)
            {
                throw new PluginApplicationException(ex.Message);
            }
        }

        private List<ISftpFile> ListDirectoryFiles(BlackbirdSftpClient sftpClient, string folderPath, bool includeSubfolder)
        {
            var filesList = new List<ISftpFile>();
            var items = new List<ISftpFile>();
            try
            {
                items = UseClient((sftpClient) => sftpClient.ListDirectory(folderPath).Where(x => x.Name != "." && x.Name != "..").ToList());
            }
            catch (Exception ex)
            {

                throw new PluginApplicationException(ex.Message);
            }

            foreach (var entry in items)
            {
                if (entry.IsDirectory && includeSubfolder)
                {
                    filesList.AddRange(ListDirectoryFiles(sftpClient, entry.FullName, includeSubfolder));
                }
                else if(!entry.IsDirectory)
                {
                    filesList.Add(entry);
                }
            }
            return filesList;
        }
        
        private List<ISftpFile> ListDirectoryFolders(BlackbirdSftpClient sftpClient, string folderPath, bool includeSubfolder)
        {
            var folderList = new List<ISftpFile>();
            var items = new List<ISftpFile>();
            try
            {
                 items =
                UseClient((sftpClient) => sftpClient.ListDirectory(folderPath)
                .Where(x => x.Name != "." && x.Name != "..")
                .ToList());
            }
            catch (Exception ex)
            {
                throw new PluginApplicationException(ex.Message);
            }

            foreach (var entry in items)
            {
                if (entry.IsDirectory)
                {
                    folderList.Add(entry);
            
                    if (includeSubfolder)
                    {
                        folderList.AddRange(ListDirectoryFolders(sftpClient, entry.FullName, includeSubfolder));
                    }
                }
            }

            return folderList;
        }
    }
}

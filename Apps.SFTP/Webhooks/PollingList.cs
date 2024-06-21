using Apps.SFTP.Dtos;
using Apps.SFTP.Invocables;
using Apps.SFTP.Webhooks.Payload;
using Apps.SFTP.Webhooks.Polling.Memory;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common.Polling;
using Renci.SshNet.Sftp;

namespace Apps.SFTP.Webhooks
{
    [PollingEventList]
    public class PollingList : SFTPInvocable
    {
        public PollingList(InvocationContext invocationContext) : base(invocationContext)
        {
        }

        [PollingEvent("On files created or updated", "On files created or updated")]
        public async Task<PollingEventResponse<SFTPMemory, ChangedFilesResponse>> OnFilesAddedOrUpdated(
            PollingEventRequest<SFTPMemory> request,
            [PollingEventParameter] ParentFolderInput parentFolder
            )
        {
            using var client = new BlackbirdSftpClient(Creds);
            var filesInfo = ListDirectory(client, parentFolder.Folder ?? "/", parentFolder.IncludeSubfolders ?? true);
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
                Result = new ChangedFilesResponse() { Files = filesInfo.Where(x => changedFilesPath.Contains(x.FullName)).Select(x => new DirectoryItemDto() { Name = x.Name, Path = x.FullName }).ToList() }
            };
        }

        [PollingEvent("On files deleted", "On files deleted")]
        public async Task<PollingEventResponse<SFTPMemory, ChangedFilesResponse>> OnFilesDeleted(
            PollingEventRequest<SFTPMemory> request,
            [PollingEventParameter] ParentFolderInput parentFolder
            )
        {
            using var client = new BlackbirdSftpClient(Creds);
            var filesInfo = ListDirectory(client, parentFolder.Folder ?? "/", parentFolder.IncludeSubfolders ?? true);
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
                Result = new ChangedFilesResponse() { Files = deletedItems.Select(x => new DirectoryItemDto() { Name = Path.GetFileName(x), Path = x }).ToList() }
            };
        }

        private List<ISftpFile> ListDirectory(BlackbirdSftpClient sftpClient, string folderPath, bool includeSubfolder)
        {
            var filesList = new List<ISftpFile>();
            var items = sftpClient.ListDirectory(folderPath).Where(x => x.Name != "." && x.Name != "..").ToList();
            foreach (var entry in items)
            {
                if (entry.IsDirectory && includeSubfolder)
                {
                    filesList.AddRange(ListDirectory(sftpClient, entry.FullName, includeSubfolder));
                }
                else if(!entry.IsDirectory)
                {
                    filesList.Add(entry);
                }
            }
            return filesList;
        }
    }
}

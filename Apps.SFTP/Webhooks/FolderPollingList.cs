using Apps.SFTP.Dtos;
using Apps.SFTP.Invocables;
using Apps.SFTP.Models.Responses;
using Apps.SFTP.Webhooks.Payload;
using Apps.SFTP.Webhooks.Polling.Memory;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common.Polling;
using Renci.SshNet.Sftp;

namespace Apps.SFTP.Webhooks
{
    [PollingEventList("Folders")]
    public class FolderPollingList(InvocationContext invocationContext) : SFTPInvocable(invocationContext)
    {
        [PollingEvent("On folders created", Description = "Triggers when folders are created")]
        public Task<PollingEventResponse<SftpDirectoryMemory, ListDirectoryResponse>> OnDirectoriesCreated(
            PollingEventRequest<SftpDirectoryMemory> request,
            [PollingEventParameter] ParentFolderInput parentFolder)
        {
            try
            {
                using var client = new BlackbirdSftpClient(Creds);
                var directories = ListDirectoryFolders(client, parentFolder.Folder ?? "/",
                    parentFolder.IncludeSubfolders ?? false);

                var directoryState = directories.Select(x => x.FullName).ToList();
                if (request.Memory == null)
                {
                    return Task.FromResult<PollingEventResponse<SftpDirectoryMemory, ListDirectoryResponse>>(new()
                    {
                        FlyBird = false,
                        Memory = new SftpDirectoryMemory { DirectoriesState = directoryState }
                    });
                }

                var newItems = directoryState.Except(request.Memory.DirectoriesState).ToList();
                if (newItems.Count == 0)
                {
                    return Task.FromResult<PollingEventResponse<SftpDirectoryMemory, ListDirectoryResponse>>(new()
                    {
                        FlyBird = false,
                        Memory = new SftpDirectoryMemory { DirectoriesState = directoryState }
                    });
                }

                return Task.FromResult<PollingEventResponse<SftpDirectoryMemory, ListDirectoryResponse>>(new()
                {
                    FlyBird = true,
                    Memory = new SftpDirectoryMemory { DirectoriesState = directoryState },
                    Result = new ListDirectoryResponse
                    {
                        DirectoriesItems = directories.Where(x => newItems.Contains(x.FullName)).Select(x => new DirectoryItemDto
                        {
                            Name = x.Name,
                            FileId = x.FullName
                        })
                    }
                });
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

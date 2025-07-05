using Apps.SFTP.Dtos;
using Blackbird.Applications.Sdk.Common;

namespace Apps.SFTP.Models.Responses;

public class ListDirectoryResponse
{
    [Display("Folder items")]
    public IEnumerable<DirectoryItemDto> DirectoriesItems { get; set; }
}
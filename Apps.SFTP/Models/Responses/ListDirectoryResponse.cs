using Apps.SFTP.Dtos;

namespace Apps.SFTP.Models.Responses;

public class ListDirectoryResponse
{
    public IEnumerable<DirectoryItemDto> DirectoriesItems { get; set; }
}
using Apps.SFTP.Api.Sftp;
using Tests.SFTP.Base;

namespace Tests.SFTP;

[TestClass]
public class SftpClientTests : TestBase
{
    private const string RemoteFolder = "/test";

    [TestMethod]
    public async Task UploadAsync_with_non_seekable_stream_uploads_whole_content()
    {
        var remotePath = $"{RemoteFolder}/non_seekable_upload_test.bin";
        var content = new byte[1024 * 1024 + 321];
        new Random(11).NextBytes(content);

        using var client = new BlackbirdSftpClient(Creds);
        await client.ConnectAsync();

        await client.UploadAsync(new NonSeekableStream(new MemoryStream(content)), remotePath);

        using var downloaded = new MemoryStream();
        await client.DownloadAsync(remotePath, downloaded);
        CollectionAssert.AreEqual(content, downloaded.ToArray());
    }
}

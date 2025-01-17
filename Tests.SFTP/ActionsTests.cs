using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apps.SFTP;
using Apps.SFTP.Connections;

namespace Tests.SFTP
{
    [TestClass]
    public class ActionsTests :TestBase
    {

        [TestMethod]
        public async Task UploadFile()
        {
            var client = new Actions(InvocationContext,FileManager);

            var response = client.UploadFile();
            //Assert.IsTrue(result.IsValid);
        }
    }
}

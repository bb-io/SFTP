using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.SFTP.Models.Responses
{
    public class GetFileInformationResponse
    {
        public long Size { get; set; }

        public string Path { get; set; }
    }
}

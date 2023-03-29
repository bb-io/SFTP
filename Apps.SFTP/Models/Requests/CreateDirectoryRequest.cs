using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.SFTP.Models.Requests
{
    public class CreateDirectoryRequest
    {
        public string DirectoryName { get; set; }

        public string Path { get; set; }
    }
}

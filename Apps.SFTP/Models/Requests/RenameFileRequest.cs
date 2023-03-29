using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.SFTP.Models.Requests
{
    public class RenameFileRequest
    {
        public string OldPath { get; set; }

        public string NewPath { get; set; }
    }
}

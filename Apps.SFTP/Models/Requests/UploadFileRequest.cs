﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.SFTP.Models.Requests
{
    public class UploadFileRequest
    {
        public byte[] File { get; set; }

        public string FileName { get; set; }

        public string Path { get; set; }
    }
}

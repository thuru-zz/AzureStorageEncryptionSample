using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace WebApplication.Models
{
    public class AttachmentDto
    {
        public Stream FileStream { get; set; }
        public string FileName { get; set; }
    }
}
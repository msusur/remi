using System;
using System.Collections.Generic;

namespace ReMi.Plugin.ZenDesk.Models
{
    public class Upload
    {
        public IEnumerable<Attachment> Attachments { get; set; }

        public DateTime Expires_At { get; set; }

        public string Token { get; set; }
    }
}

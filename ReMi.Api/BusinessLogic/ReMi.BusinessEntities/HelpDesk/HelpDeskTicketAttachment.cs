
using System;

namespace ReMi.BusinessEntities.HelpDesk
{
    public class HelpDeskTicketAttachment
    {
        public string HelpDeskTicketId { get; set; }
        public byte[] Data { get; set; }
        public string FileName { get; set; }
        public string Comment { get; set; }
        public string HelpDeskAttachmentId { get; set; }

        public Guid ReleaseTaskId { get; set; }
        public Guid ReleaseAttachmentId { get; set; }
    }
}

using System;

namespace ReMi.BusinessEntities.ReleasePlan
{
    public class ReleaseTaskAttachment
    {
        public Guid ExternalId { get; set; }

        public string Name { get; set; }

        public byte[] Attachment { get; set; }

        public Guid ReleaseTaskId { get; set; }

        public string HelpDeskAttachmentId { get; set; }

        public override string ToString()
        {
            return string.Format("[ExternalId = {0}, Name = {1}, Attachment = {2} bytes, ReleaseTaskId = {3}]",
                ExternalId, Name, Attachment != null ? Attachment.Length : 0, ReleaseTaskId);
        }
    }
}

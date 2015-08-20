using System;

namespace ReMi.DataEntities.ReleasePlan
{
    public class ReleaseTaskAttachment
    {
        public int ReleaseTaskAttachmentId { get; set; }

        public Guid ExternalId { get; set; }

        public string Name { get; set; }

        public byte[] Attachment { get; set; }

        public string HelpDeskAttachmentId { get; set; }

        public int ReleaseTaskId { get; set; }


        public override string ToString()
        {
            return string.Format("[ReleaseTaskAttachmentId={0}, ExternalId={1}, Name={2}, Attachment.Length={3}, HelpDeskAttachmentId={4}, ReleaseTaskId={5}]",
                ReleaseTaskAttachmentId, ExternalId, Name, Attachment.Length, HelpDeskAttachmentId, ReleaseTaskId);
        }
    }
}

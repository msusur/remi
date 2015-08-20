using System;

namespace ReMi.BusinessEntities.ReleasePlan
{
    public class ReleaseTaskAttachmentView
    {
        public Guid ExternalId { get; set; }

        public string Name { get; set; }

        public string ServerName { get; set; }

        public string Type { get; set; }

        public int Size { get; set; }
    }
}

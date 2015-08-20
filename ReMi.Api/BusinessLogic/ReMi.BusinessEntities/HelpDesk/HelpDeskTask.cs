
using System;

namespace ReMi.BusinessEntities.HelpDesk
{
    public class HelpDeskTask
    {
        public string Number { get; set; }
        public string Url { get; set; }
        public string Subject { get; set; }
        public string Description { get; set; }

        public Guid ReleaseTaskId { get; set; }
        public Guid ReleaseWindowId { get; set; }
        public Guid? CreatedByExternalId { get; set; }
    }
}

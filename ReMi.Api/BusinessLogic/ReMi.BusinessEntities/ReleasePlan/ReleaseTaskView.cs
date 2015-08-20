using System;
using System.Collections.Generic;

namespace ReMi.BusinessEntities.ReleasePlan
{
    public class ReleaseTaskView
    {
        public Guid ExternalId { get; set; }

        public Guid ReleaseWindowId { get; set; }

        public string Description { get; set; }

        public string CreatedBy { get; set; }

        public DateTime CreatedOn { get; set; }

        public string Assignee { get; set; }

        public Guid AssigneeExternalId { get; set; }

        public DateTime? CompletedOn { get; set; }

        public string Type { get; set; }

        public string HelpDeskReference { get; set; }

        public string HelpDeskUrl { get; set; }

        public DateTime? ReceiptConfirmedOn { get; set; }

        public IEnumerable<ReleaseTaskAttachmentView> Attachments { get; set; }


        public bool RequireSiteDown { get; set; }

        public string Risk { get; set; }

        public string WhereTested { get; set; }

        public int? LengthOfRun { get; set; }

        public short Order { get; set; }
    }
}

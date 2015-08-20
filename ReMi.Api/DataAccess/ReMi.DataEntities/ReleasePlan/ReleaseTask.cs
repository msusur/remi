using ReMi.Common.Constants.ReleasePlan;
using ReMi.DataEntities.Auth;
using ReMi.DataEntities.ReleaseCalendar;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ReMi.DataEntities.ReleasePlan
{
    public class ReleaseTask
    {
        public int ReleaseTaskId { get; set; }

        public Guid ExternalId { get; set; }

        public ReleaseTaskType Type { get; set; }

        [StringLength(2048)]
        public string Description { get; set; }

        public string HelpDeskReference { get; set; }

        public string HelpDeskUrl { get; set; }

        public ICollection<ReleaseTaskAttachment> Attachments { get; set; }

        public int ReleaseWindowId { get; set; }

        public int CreatedByAccountId { get; set; }

        
        public DateTime CreatedOn { get; set; }

        public int AssigneeAccountId { get; set; }

        public DateTime? ReceiptConfirmedOn { get; set; }

        public DateTime? CompletedOn { get; set; }


        public bool RequireSiteDown { get; set; }

        public ReleaseTaskRisk Risk { get; set; }

        public ReleaseTaskEnvironment? WhereTested { get; set; }

        public int? LengthOfRun { get; set; }

        public short Order { get; set; }


        // Relations

        public virtual ReleaseWindow ReleaseWindow { get; set; }

        public virtual Account CreatedBy { get; set; }

        public virtual Account Assignee { get; set; }

        public override string ToString()
        {
            return string.Format("[ReleaseTaskId={0}, ExternalId={1}, Type={2}, Description={3}, HelpDeskReference={4}, HelpDeskUrl={5}, Attachment.Count={6}" +
                ", ReleaseWindowId={7}, AssigneeAccountId={8}, CreatedOn={9}, CreatedByAccountId={10}, CompletedOn={11}" +
                ", RequireSiteDown={12}, Risk={13}, WhereTested={14}, LengthOfRun={15}, Order={16}]",
                ReleaseTaskId, ExternalId, Type, Description,
                HelpDeskReference, HelpDeskUrl,
                (Attachments != null ? Attachments.Count() : 0),
                ReleaseWindowId, AssigneeAccountId, CreatedOn, CreatedByAccountId, CompletedOn, 
                RequireSiteDown, Risk, WhereTested, LengthOfRun, Order);
        }
    }
}

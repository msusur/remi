using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using ReMi.Common.Constants.ReleaseCalendar;
using ReMi.DataEntities.Metrics;
using ReMi.DataEntities.ReleaseExecution;
using ReMi.DataEntities.ReleasePlan;
using System;
using System.Collections.Generic;
using ReMi.DataEntities.Auth;
using ReMi.DataEntities.SourceControl;

namespace ReMi.DataEntities.ReleaseCalendar
{
    public class ReleaseWindow
    {
        [Key]
        public int ReleaseWindowId { get; set; }

        public DateTime StartTime { get; set; }
        [Required, StringLength(128)]
        public string Sprint { get; set; }

        public bool RequiresDowntime { get; set; }

        public ReleaseType ReleaseType { get; set; }

        public DateTime CreatedOn { get; set; }

        public int CreatedById { get; set; }

        public Guid ExternalId { get; set; }

        [StringLength(1024)]
        public string Description { get; set; }

        public DateTime OriginalStartTime { get; set; }

        public ReleaseDecision ReleaseDecision { get; set; }

        [DefaultValue(false)]
        public bool IsFailed { get; set; }

        public virtual ICollection<Metric> Metrics { get; set; }

        public virtual ICollection<CheckList> CheckList { get; set; }

        public virtual ICollection<ReleaseParticipant> ReleaseParticipants { get; set; }
        public virtual ICollection<ReleaseTask> ReleaseTasks { get; set; }
        public virtual ICollection<SourceControlChangeToReleaseWindow> SourceControlChangeToReleaseWindows { get; set; }

        public virtual ICollection<ReleaseContent> ReleaseContent { get; set; }
        public virtual ICollection<SignOff> SignOffs { get; set; }
        public virtual ICollection<ReleaseApprover> ReleaseApprovers { get; set; }
        public virtual ICollection<ReleaseJob> ReleaseJobs { get; set; }

        public virtual ICollection<ReleaseProduct> ReleaseProducts { get; set; }

        public virtual ReleaseNote ReleaseNotes { get; set; }

        public virtual Account CreatedBy { get; set; }


        public override string ToString()
        {
            return
                string.Format(
                    "[StartTime={0}, OriginalStartTime={1}, ReleaseType={2}, RequiresDowntime={3}, " +
                    "CreatedOn={4}, ReleaseWindowId={5}, Sprint={6}, Description={7}, ReleaseDecision={8}, IsFailed={9}, CreatedBy={10}]",
                    StartTime, OriginalStartTime, ReleaseType, RequiresDowntime, CreatedOn, ReleaseWindowId,
                    Sprint, Description, ReleaseDecision, IsFailed, CreatedBy);
        }
    }
}

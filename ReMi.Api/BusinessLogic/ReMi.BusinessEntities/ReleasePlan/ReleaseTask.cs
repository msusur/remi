using System;

namespace ReMi.BusinessEntities.ReleasePlan
{
    public class ReleaseTask
    {
        public Guid ExternalId { get; set; }

        public Guid ReleaseWindowId { get; set; }

        public string Description { get; set; }

        public string Assignee { get; set; }

        public Guid AssigneeExternalId { get; set; }

        public string CreatedBy { get; set; }

        public Guid? CreatedByExternalId { get; set; }

        public string Type { get; set; }

        public bool CreateHelpDeskTicket { get; set; }

        public string HelpDeskTicketReference { get; set; }

        public string HelpDeskTicketUrl { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime? CompletedOn { get; set; }

        public bool RequireSiteDown { get; set; }

        public string Risk { get; set; }

        public string WhereTested { get; set; }

        public int? LengthOfRun { get; set; }
    }
}

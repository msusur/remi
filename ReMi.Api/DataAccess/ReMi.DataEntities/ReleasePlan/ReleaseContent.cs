using System;
using ReMi.Common.Constants.ReleasePlan;
using ReMi.DataEntities.Auth;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ReMi.DataEntities.ReleaseCalendar;

namespace ReMi.DataEntities.ReleasePlan
{
    public class ReleaseContent
    {
        public int ReleaseContentId { get; set; }

        public string Comment { get; set; }

        [Index(IsUnique = true)]
        public Guid TicketId { get; set; }

        public string TicketKey { get; set; }

        public TicketRisk TicketRisk { get; set; }

        public bool IncludeToReleaseNotes { get; set; }

        public int LastChangedByAccountId { get; set; }

        public int? ReleaseWindowsId { get; set; }

        [StringLength(1024)]
        public string Description { get; set; }

        [StringLength(128)]
        public string Assignee { get; set; }

        public string TicketUrl { get; set; }

        public virtual Account LastChangedByAccount { get; set; }

        public virtual ReleaseWindow ReleaseWindow { get; set; }

        public override string ToString()
        {
            return string.Format("[TicketInformationId = {0}, Comment = {1}, TicketId = {2}, TicketKey = {3}, TicketKey = {4}, LastChangedByAccountId = {5}, LastChangedByAccount = {6}, Description = {7}, Assignee = {8}, TicketUrl={9} ]",
                ReleaseContentId, Comment, TicketId, TicketKey, TicketRisk, LastChangedByAccountId, LastChangedByAccount, Description, Assignee, TicketUrl);
        }
    }
}

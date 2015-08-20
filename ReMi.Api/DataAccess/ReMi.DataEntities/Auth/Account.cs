using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ReMi.DataEntities.ReleaseCalendar;
using ReMi.DataEntities.ReleaseExecution;
using ReMi.DataEntities.Subscriptions;

namespace ReMi.DataEntities.Auth
{
    public class Account
    {
        #region scalar props

        public int AccountId { get; set; }

        [Index]
        public Guid ExternalId { get; set; }

        public string Name { get; set; }

        public string FullName { get; set; }

        [StringLength(512), Index(IsUnique = true)]
        public string Email { get; set; }

        public int RoleId { get; set; }

        public bool IsBlocked { get; set; }

        public string Description { get; set; }

        public DateTime CreatedOn { get; set; }

        #endregion

        #region navigational props

        public virtual ICollection<Session> Sessions { get; set; }

        public virtual ICollection<AccountProduct> AccountProducts { get; set; }

        public virtual ICollection<ReleaseParticipant> ReleaseParticipants { get; set; }

        public virtual ICollection<AccountNotification> AccountNotifications { get; set; }

        public virtual ICollection<SignOff> SignOffs { get; set; }

        public virtual Role Role { get; set; }

        #endregion

        public override string ToString()
        {
            return string.Format("[AccountId = {0}, ExternalId = {1}, Name = {2}, FullName = {3}, Email = {4}, Description = {5}, Role = {6}, IsBlocked = {7}, CreatedOn = {8}]",
                AccountId, ExternalId, Name, FullName, Email, Description, Role, IsBlocked, CreatedOn);
        }
    }
}

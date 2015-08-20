using System;

namespace ReMi.BusinessEntities.Auth
{
    public class Session
    {
        public Guid ExternalId { get; set; }

        public Guid AccountId { get; set; }

        public DateTime? ExpireAfter { get; set; }

        public DateTime? Completed { get; set; }

        public string Description { get; set; }

        public DateTime CreatedOn { get; set; }

        public override string ToString()
        {
            return string.Format("[ExternalId = {0}, AccountId = {1}, Description = {2}, ExpireAfter = {3}, Completed = {4}, CreatedOn = {5}]",
                ExternalId, AccountId, Description, ExpireAfter, Completed, CreatedOn);
        }
    }
}

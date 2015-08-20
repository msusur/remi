using System;

namespace ReMi.DataEntities.Auth
{
    public class Session
    {
         #region .ctor

        public Session()
        {
        }

        #endregion

        #region scalar props

        public int SessionId { get; set; }

        public Guid ExternalId { get; set; }

        public int AccountId { get; set; }

        public DateTime? ExpireAfter { get; set; }

        public DateTime? Completed { get; set; }

        public string Description { get; set; }

        public DateTime CreatedOn { get; set; }

        #endregion

        #region navigational props

        public virtual Account Account { get; set; }

        #endregion

        public override string ToString()
        {
            return string.Format("[SessionId = {0}, ExternalId = {1}, AccountId = {2}, Description = {3}, ExpireAfter = {4}, Completed = {5}, CreatedOn = {6}]", SessionId, ExternalId, AccountId, Description, ExpireAfter, Completed, CreatedOn);
        }
    }
}

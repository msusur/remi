using System;

namespace ReMi.BusinessEntities.Evt
{
    public class Event
    {
        #region props

        public string Description { get; set; }

        public string Data { get; set; }

        public EventStateType State { get; set; }

        public DateTime CreatedOn { get; set; }

        public Guid ExternalId { get; set; }

        #endregion

        public override string ToString()
        {
            return string.Format("[Description = {0}, State = {1}, CreatedOn = {2}, ExternalId={3}, Data = {4}]",
                Description, State, CreatedOn, ExternalId, Data);
        }

    }
}

using System;
using System.Collections.Generic;
using ReMi.Common.Utils;

namespace ReMi.DataEntities.Evt
{
    public class Event
    {
        #region .ctor

        public Event()
        {
            CreatedOn = SystemTime.Now;
        }

        #endregion

        #region scalar props

        public int EventId { get; set; }

        public Guid ExternalId { get; set; }

        public string Data { get; set; }

        public string Description { get; set; }

        public string Handler { get; set; }

        public DateTime CreatedOn { get; set; }

        #endregion

        #region navigational props

        public virtual List<EventHistory> EventHistory { get; set; }

        #endregion

        public override string ToString()
        {
            return string.Format("[EventId = {0}, ExternalId = {1}, Description = {2}, Handler = {3}, CreatedOn = {4}, Data = {5}]",
                EventId, ExternalId, Description, Handler, CreatedOn, Data);
        }
    }
}

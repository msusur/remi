using System;
using ReMi.Common.Utils;

namespace ReMi.DataEntities.Evt
{
    public class EventHistory
    {
        #region .ctor

        public EventHistory()
        {
            CreatedOn = SystemTime.Now;
        }

        #endregion

        #region scalar props

        public int EventHistoryId { get; set; }

        public int EventId { get; set; }

        public EventStateType State { get; set; }

        public DateTime CreatedOn { get; set; }

        public string Details { get; set; }

        #endregion

        #region navigational props

        public virtual Event Event { get; set; }

        #endregion

        public override string ToString()
        {
            return string.Format("[EventId={0}, State={1}, CreatedOn={2}, EventHistoryId={3}, Details={4}]",
                EventId, State, CreatedOn, EventHistoryId, Details);
        }
    }
}

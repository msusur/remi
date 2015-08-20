using ReMi.Common.Constants.ReleaseCalendar;
using System;
using System.Collections.Generic;
using ReMi.Common.Utils;

namespace ReMi.BusinessEntities.ReleaseCalendar
{
    public class ReleaseWindowView
    {
        public Guid ExternalId { get; set; }

        public DateTime StartTime { get; set; }

        private DateTime? _endTime;

        public DateTime EndTime
        {
            get
            {
                return _endTime ?? StartTime.AddHours(2);
            }
            set { _endTime = value; }
        }

        public IEnumerable<string> Products { get; set; }

        public string Sprint { get; set; }

        public String Status { get; set; }

        public ReleaseType ReleaseType { get; set; }

        public bool RequiresDowntime { get; set; }

        public String Description { get; set; }

        public bool IsFailed { get; set; }

        public bool IsMaintenance { get; set; }

        public override string ToString()
        {
            return
                string.Format(
                    "[ExternalId={0}, StartTime={1}, EndTime={2}, Products={3}, Sprint={4}, Status={5}, " +
                    "ReleaseType={6}, RequiresDowntime={7}, Description={8}, IsFailed={9}, IsMaintenance={10}]",
                    ExternalId, StartTime, EndTime, Products.FormatElements(), Sprint, Status, ReleaseType, RequiresDowntime, Description, IsFailed, IsMaintenance);
        }
    }
}

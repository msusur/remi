using System;
using System.Collections.Generic;
using ReMi.BusinessEntities.Plugins;
using ReMi.Common.Constants.ReleaseCalendar;
using ReMi.Common.Utils;

namespace ReMi.BusinessEntities.ReleaseCalendar
{
    public class ReleaseWindow
    {
        #region props

        public DateTime StartTime { get; set; }

        private DateTime? _endTime;

        public DateTime EndTime
        {
            get
            {
                if (_endTime.HasValue)
                    return _endTime.Value;

                return StartTime.AddHours(2);
            }
            set { _endTime = value; }
        }

        public DateTime OriginalStartTime { get; set; }

        public IEnumerable<string> Products { get; set; }

        public string Sprint { get; set; }

        public string Status { get; set; }

        public ReleaseType ReleaseType { get; set; }

        public bool RequiresDowntime { get; set; }

        public Guid ExternalId { get; set; }

        public DateTime? ApprovedOn { get; set; }

        public DateTime? ClosedOn { get; set; }

        public string ReleaseNotes { get; set; }

        public string ReleaseTypeDescription { get; set; }

        public string Description { get; set; }

        public DateTime? SignedOff { get; set; }

        public string ReleaseDecision { get; set; }

        public string Issues { get; set; }

        public bool IsFailed { get; set; }

        public IEnumerable<PluginView> Plugins { get; set; }

        #endregion

        public override string ToString()
        {
            return
                string.Format(
                    "[StartTime={0}, EndTime={1}, OriginalStartTime={2}, Products={3}, ReleaseType={4}, RequiresDowntime={5}, " +
                    "ExternalId={6}, ApprovedOn={7}, ClosedOn={8}, Sprint={9}, ReleaseNotes={10}, Description={11}, Status={12}, " +
                    "SignedOff={13}, ReleaseDecision={14}, Issues={15}, Plugins={16}]",
                    StartTime, EndTime, OriginalStartTime, Products.FormatElements(), ReleaseType, RequiresDowntime, ExternalId, ApprovedOn, ClosedOn,
                    Sprint, ReleaseNotes, Description, Status, SignedOff, ReleaseDecision, Issues, Plugins.FormatElements());
        }

    }
}

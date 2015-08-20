using System;
using System.Collections.Generic;
using ReMi.BusinessEntities.ReleaseCalendar;
using ReMi.Common.Utils;

namespace ReMi.BusinessEntities.Metrics
{
    public class Measurement
    {
        public IEnumerable<MeasurementTime> Metrics { get; set; }
        public ReleaseWindow ReleaseWindow { get; set; }

        public override string ToString()
        {
            return String.Format("[Metrics={0}, ReleaseWindow={1}]", Metrics.FormatElements(),
                ReleaseWindow);
        }
    }
}

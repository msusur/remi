using ReMi.BusinessEntities.Metrics;
using ReMi.Common.Utils;
using System;
using System.Collections.Generic;

namespace ReMi.Queries.Metrics
{
    public class GetMeasurementsResponse
    {
        public IEnumerable<Measurement> Measurements { get; set; }

        public override string ToString()
        {
            return String.Format("[Measurements={0}]",
                Measurements.FormatElements());
        }
    }
}

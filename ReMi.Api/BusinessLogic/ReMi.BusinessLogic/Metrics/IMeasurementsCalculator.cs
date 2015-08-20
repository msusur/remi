using System.Collections.Generic;
using ReMi.BusinessEntities.Metrics;
using ReMi.BusinessEntities.ReleaseCalendar;
using ReMi.Common.Constants.Metrics;
using ReMi.Contracts;

namespace ReMi.BusinessLogic.Metrics
{
    public interface IMeasurementsCalculator
    {
        Measurement Calculate(ReleaseWindow releaseWindow, IEnumerable<Metric> metrics, params MeasurementType[] types);
        MeasurementTimeByType Calculate(MeasurementType type, ReleaseWindow releaseWindow, IEnumerable<Metric> metrics);
    }
}

using System;
using ReMi.Common.Constants.ReleaseExecution;

namespace ReMi.BusinessEntities.Exceptions
{
	public class MissingMetricException : ApplicationException
	{
		public MissingMetricException(Guid releaseWindowId, MetricType metricType)
            : base(FormatMessage(releaseWindowId, metricType))
		{
		}

        public MissingMetricException(Guid releaseWindowId, MetricType metricType, Exception innerException)
            : base(FormatMessage(releaseWindowId, metricType), innerException)
		{
		}

        private static string FormatMessage(Guid releaseWindowId, MetricType metricType)
		{
            return string.Format("Missing metric type [{0}] for rlease window [{1}]", metricType, releaseWindowId);
		}

		
	}
}

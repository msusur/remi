using System;

namespace ReMi.DataAccess.Exceptions.DeploymentTool
{
    public class DeploymentJobMeasurementAlreadyExists : ApplicationException
    {
        public DeploymentJobMeasurementAlreadyExists(Guid releaseWindowId)
            : base(FormatMessage(releaseWindowId))
        {
        }

        public DeploymentJobMeasurementAlreadyExists(Guid releaseWindowId, Exception innerException)
            : base(FormatMessage(releaseWindowId), innerException)
        {
        }

        private static string FormatMessage(Guid entityRef)
        {
            return string.Format("Deployment job measurements for release '{0}' already exists", entityRef);
        }
    }
}

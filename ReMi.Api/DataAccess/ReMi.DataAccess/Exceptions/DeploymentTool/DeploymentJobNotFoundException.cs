using System;

namespace ReMi.DataAccess.Exceptions.DeploymentTool
{
    public class DeploymentJobNotFoundException : EntityNotFoundException
    {
        public DeploymentJobNotFoundException(Guid jobId) :
            base("DeploymentJob", jobId)
        {
        }

        public DeploymentJobNotFoundException(Guid jobId, Exception innerException) :
            base("DeploymentJob", jobId, innerException)
        {
        }
    }
}

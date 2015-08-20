using System;

namespace ReMi.BusinessEntities.DeploymentTool
{
    public class ReleaseJob
    {
        public Guid ExternalId { get; set; }

        public string Name { get; set; }

        public Guid JobId { get; set; }

        public int Order { get; set; }

        public bool IsIncluded { get; set; }

        public int? LastBuildNumber { get; set; }

        public override string ToString()
        {
            return string.Format("[Name={0}, ExternalId={1}, JobId={2}, Order={3}, IsIncluded={4}, LastBuildNumber={5}]",
                Name, ExternalId, JobId, Order, IsIncluded, LastBuildNumber);
        }
    }
}

using System;

namespace ReMi.Contracts.Plugins.Data.DeploymentTool
{
    public class ReleaseJob
    {
        public Guid ExternalId { get; set; }
        public int Order { get; set; }
        public bool IsIncluded { get; set; }
        public bool IsDisabled { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return string.Format("[ExternalId={0},Order={1},IsIncluded={2},IsDisabled={3},Name={4}]",
                ExternalId, Order, IsIncluded, IsDisabled, Name);
        }
    }
}

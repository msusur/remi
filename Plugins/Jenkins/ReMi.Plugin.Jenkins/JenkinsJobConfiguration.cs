
using System;

namespace ReMi.Plugin.Jenkins
{
    public class JenkinsJobConfiguration
    {
        public string Name { get; set; }
        public bool IsIncludedByDefault { get; set; }
        public bool IsDisabled { get; set; }
        public Guid ExternalId { get; set; }

        public override string ToString()
        {
            return string.Format("[Name={0}, IsIncludedByDefault={1}, IsDisabled={2}, ExternalId={3}]",
                Name, IsIncludedByDefault, IsDisabled, ExternalId);
        }
    }
}

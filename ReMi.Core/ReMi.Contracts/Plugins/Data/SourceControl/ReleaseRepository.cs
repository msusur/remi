using System;

namespace ReMi.Contracts.Plugins.Data.SourceControl
{
    public class ReleaseRepository
    {
        public Guid ExternalId { get; set; }
        public string Repository { get; set; }

        public bool LatestChange { get; set; }
        public string ChangesFrom { get; set; }
        public string ChangesTo { get; set; }

        public bool IsIncluded { get; set; }
        public bool IsDisabled { get; set; }

        public override string ToString()
        {
            return string.Format("[ExternalId={0}, Repository={1}, LatestChange={2}, ChangesFrom={3}, ChangesTo={4}, IsIncluded={5}, IsDisabled={6}]",
                ExternalId, Repository, LatestChange, ChangesFrom, ChangesTo, IsIncluded, IsDisabled);
        }
    }
}

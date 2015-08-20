using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ReMi.DataEntities.ReleaseCalendar;

namespace ReMi.DataEntities.ReleasePlan
{
    [Table("ReleaseRepositories", Schema = Constants.SchemaName)]
    public class ReleaseRepository
    {
        [Key]
        public int ReleaseRepositoryId { get; set; }

        public string Name { get; set; }

        [Index("IX_RepositoryId_ReleaseWindowId", IsUnique = true, Order = 1)]
        public Guid RepositoryId { get; set; }

        public bool LatestChange { get; set; }
        public string ChangesFrom { get; set; }
        public string ChangesTo { get; set; }

        public bool IsIncluded { get; set; }


        [Index("IX_RepositoryId_ReleaseWindowId", IsUnique = true, Order = 2)]
        public int ReleaseWindowId { get; set; }
        public virtual ReleaseWindow ReleaseWindow { get; set; }

        public override string ToString()
        {
            return string.Format("[ReleaseRepositoryId={0}, Name={1}, RepositoryId={2}, " +
                                 "LatestChange={3}, ChangesFrom={4}, ChangesTo={5}, IsIncluded={6}, ReleaseWindowId={7}]",
                ReleaseRepositoryId, Name, RepositoryId, LatestChange, ChangesFrom, ChangesTo, IsIncluded, ReleaseWindowId);
        }
    }
}

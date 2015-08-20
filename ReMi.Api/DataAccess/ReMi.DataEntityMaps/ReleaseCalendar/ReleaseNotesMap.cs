using ReMi.DataEntities.ReleaseCalendar;
using System.Data.Entity.ModelConfiguration;

namespace ReMi.DataEntityMaps.ReleaseCalendar
{
    public class ReleaseNotesMap : EntityTypeConfiguration<ReleaseNote>
    {
        public ReleaseNotesMap()
        {
            HasRequired(x => x.ReleaseWindow)
                .WithRequiredDependent(x => x.ReleaseNotes)
                .WillCascadeOnDelete(true);
        }
    }
}

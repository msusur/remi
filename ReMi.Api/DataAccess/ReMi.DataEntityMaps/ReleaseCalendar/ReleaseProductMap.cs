using System.Data.Entity.ModelConfiguration;
using ReMi.DataEntities.ReleaseCalendar;

namespace ReMi.DataEntityMaps.ReleaseCalendar
{
    public class ReleaseProductMap : EntityTypeConfiguration<ReleaseProduct>
    {
        public ReleaseProductMap()
        {
            HasRequired(x => x.ReleaseWindow)
                .WithMany(x => x.ReleaseProducts)
                .HasForeignKey(x => x.ReleaseWindowId)
                .WillCascadeOnDelete(true);

            HasRequired(x => x.Product)
                .WithMany(x => x.ReleaseProducts)
                .HasForeignKey(x => x.ProductId)
                .WillCascadeOnDelete(true);
        }
    }
}

using System.Data.Entity.ModelConfiguration;
using ReMi.DataEntities.ProductRequests;

namespace ReMi.DataEntityMaps.ProductRequests
{
    public class ProductRequestRegistrationMap : EntityTypeConfiguration<ProductRequestRegistration>
    {
        public ProductRequestRegistrationMap()
        {
            HasRequired(x => x.ProductRequestType)
                .WithMany(x => x.Registrations)
                .HasForeignKey(x => x.ProductRequestTypeId)
                .WillCascadeOnDelete(false);

            HasRequired(o => o.CreatedBy).WithMany().HasForeignKey(o => o.CreatedByAccountId).WillCascadeOnDelete(false);

            HasOptional(x => x.RemovingReason)
                .WithRequired(x => x.ProductRequestRegistration)
                .WillCascadeOnDelete(true);
        }
    }
}

using System.Data.Entity.ModelConfiguration;
using ReMi.DataEntities.ProductRequests;

namespace ReMi.DataEntityMaps.ProductRequests
{
    public class ProductRequestRegistrationTaskMap : EntityTypeConfiguration<ProductRequestRegistrationTask>
    {
        public ProductRequestRegistrationTaskMap()
        {
            HasRequired(x => x.ProductRequestRegistration)
              .WithMany(x => x.Tasks)
              .HasForeignKey(x => x.ProductRequestRegistrationId)
              .WillCascadeOnDelete(true);

            HasRequired(x => x.ProductRequestTask)
              .WithMany(x => x.RegistrationTasks)
              .HasForeignKey(x => x.ProductRequestTaskId)
              .WillCascadeOnDelete(false);

            HasOptional(o => o.LastChangedBy).WithMany().HasForeignKey(o => o.LastChangedByAccountId).WillCascadeOnDelete(false);
        }
    }
}

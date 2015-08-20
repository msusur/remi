using ReMi.DataEntities.Products;
using System.Data.Entity.ModelConfiguration;

namespace ReMi.DataEntityMaps.Products
{
    public class ProductGoServerMap : EntityTypeConfiguration<ProductGoServer>
    {
        public ProductGoServerMap()
        {
            HasRequired(x => x.Product)
              .WithMany()
              .HasForeignKey(x => x.ProductId)
              .WillCascadeOnDelete(true);
        }
    }
}

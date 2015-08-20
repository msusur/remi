using ReMi.DataEntities;
using ReMi.DataEntities.Products;
using System.Data.Entity.ModelConfiguration;

namespace ReMi.DataEntityMaps.Products
{
    public class ProductMap : EntityTypeConfiguration<Product>
    {
        public ProductMap()
        {
            // Primary Key 
            HasKey(t => t.ProductId);

            // Properties 
            Property(t => t.Description)
                .IsRequired()
                .HasMaxLength(128)
                .IsUnicode(false);

            // Table & Column Mappings 
            ToTable("Products", Constants.SchemaName);
            Property(t => t.ProductId).HasColumnName("ProductId");
            Property(t => t.Description).HasColumnName("Description");
            Property(t => t.ChooseTicketsByDefault).HasColumnName("ChooseTicketsByDefault");
        }
    }
}

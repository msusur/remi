using ReMi.DataEntities;
using ReMi.DataEntities.Products;
using System.Data.Entity.ModelConfiguration;

namespace ReMi.DataEntityMaps.Products
{
    public class ProductJiraBoardMap : EntityTypeConfiguration<ProductJiraBoard>
    {
        public ProductJiraBoardMap()
        {
            // Primary Key 
            HasKey(t => t.ProductJiraBoardId);

            // Properties 
            Property(t => t.ProductJiraBoardId)
                .IsRequired()
                .HasColumnName("ProductJiraBoardId");

            Property(t => t.ProductJiraBoardName)
                .IsRequired()
                .HasMaxLength(256)
                .IsUnicode(false)
                .HasColumnName("ProductJiraBoardName");

            Property(t => t.JiraBoardFilter)
                .IsRequired()
                .HasMaxLength(4096)
                .IsUnicode(true)
                .HasColumnName("JiraBoardFilter");

            Property(t => t.ProductId)
                .IsRequired()
                .HasColumnName("ProductId");

            // Table & Column Mappings 
            ToTable("ProductJiraBoard", Constants.SchemaName);
        }
    }
}

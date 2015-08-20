using System.Data.Entity.ModelConfiguration;
using ReMi.DataEntities;
using ReMi.DataEntities.Auth;

namespace ReMi.DataEntityMaps.Auth
{
    public class AccountProductMap : EntityTypeConfiguration<AccountProduct>
    {
        public AccountProductMap()
		{
			// Primary Key 
			HasKey(t => t.AccountProductId);

			// Properties 
            Property(t => t.CreatedOn)
                .IsRequired();
		
 
			// Table & Column Mappings 
            ToTable("AccountProducts", Constants.AuthSchemaName);
            Property(t => t.AccountProductId).HasColumnName("AccountProductId");
            Property(t => t.AccountId).HasColumnName("AccountId");
            Property(t => t.ProductId).HasColumnName("ProductId");
            Property(t => t.CreatedOn).HasColumnName("CreatedOn");
            Property(t => t.IsDefault).HasColumnName("IsDefault");
        }
    }
}

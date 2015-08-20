using System.Data.Entity.ModelConfiguration;
using ReMi.DataEntities;
using ReMi.DataEntities.Auth;

namespace ReMi.DataEntityMaps.Auth
{
    public class AccountMap : EntityTypeConfiguration<Account>
    {
        public AccountMap()
        {
            // Primary Key 
            HasKey(t => t.AccountId);

            // Properties 
            Property(t => t.ExternalId)
                .IsRequired();

            Property(t => t.Name)
                .HasMaxLength(128)
                .IsUnicode(false)
                .IsRequired();

            Property(t => t.FullName)
                .HasMaxLength(128)
                .IsUnicode(false)
                .IsRequired();

            Property(t => t.Email)
                .HasMaxLength(128)
                .IsUnicode(false)
                .IsRequired();

            Property(t => t.RoleId)
                .IsRequired();

            Property(t => t.IsBlocked)
                .IsRequired();

            Property(t => t.Description)
                .IsOptional()
                .HasMaxLength(128)
                .IsUnicode(false);

            Property(t => t.CreatedOn)
                .IsOptional();

            HasRequired(x => x.Role)
                .WithMany()
                .HasForeignKey(x => x.RoleId)
                .WillCascadeOnDelete(false);

            // Table & Column Mappings 
            ToTable("Accounts", Constants.AuthSchemaName);
            Property(t => t.AccountId).HasColumnName("AccountId");
            Property(t => t.ExternalId).HasColumnName("ExternalId");
            Property(t => t.Name).HasColumnName("Name");
            Property(t => t.FullName).HasColumnName("FullName");
            Property(t => t.Email).HasColumnName("Email");
            Property(t => t.IsBlocked).HasColumnName("IsBlocked");
            Property(t => t.Description).HasColumnName("Description");
            Property(t => t.CreatedOn).HasColumnName("CreatedOn");
        }
    }
}

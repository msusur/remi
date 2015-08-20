using System.Data.Entity.ModelConfiguration;
using ReMi.DataEntities;
using ReMi.DataEntities.Auth;

namespace ReMi.DataEntityMaps.Auth
{
    public class SessionMap : EntityTypeConfiguration<Session>
    {
        public SessionMap()
		{
			// Primary Key 
			HasKey(t => t.SessionId);

			// Properties 
            Property(t => t.ExternalId)
                .IsRequired();

            Property(t => t.AccountId)
                .IsRequired();

            Property(t => t.ExpireAfter)
                .IsOptional();

            Property(t => t.Completed)
                .IsOptional();

            Property(t => t.Description)
				.IsOptional()
				.HasMaxLength(128)
				.IsUnicode(false);			
 
			// Table & Column Mappings 
            ToTable("Sessions", Constants.AuthSchemaName);
            Property(t => t.SessionId).HasColumnName("SessionId");
            Property(t => t.ExternalId).HasColumnName("ExternalId");
            Property(t => t.AccountId).HasColumnName("AccountId");
            Property(t => t.ExpireAfter).HasColumnName("ExpireAfter");
            Property(t => t.Completed).HasColumnName("Completed");
			Property(t => t.Description).HasColumnName("Description");
            Property(t => t.CreatedOn).HasColumnName("CreatedOn"); 
		}
    }
}

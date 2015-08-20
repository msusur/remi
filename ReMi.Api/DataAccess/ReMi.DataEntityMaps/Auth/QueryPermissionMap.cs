using ReMi.DataEntities.Auth;
using System.Data.Entity.ModelConfiguration;

namespace ReMi.DataEntityMaps.Auth
{
    public class QueryPermissionMap : EntityTypeConfiguration<QueryPermission>
    {
        public QueryPermissionMap()
        {
            HasRequired(x => x.Query)
                .WithMany(x => x.QueryPermissions)
                .HasForeignKey(x => x.QueryId);

            HasRequired(x => x.Role)
                .WithMany()
                .HasForeignKey(x => x.RoleId)
                .WillCascadeOnDelete(false);
        }
    }
}

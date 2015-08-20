using System.Data.Entity.ModelConfiguration;
using ReMi.DataEntities.Auth;

namespace ReMi.DataEntityMaps.Auth
{
    public class CommandPermissionMap : EntityTypeConfiguration<CommandPermission>
    {
        public CommandPermissionMap()
        {
            HasRequired(x => x.Command)
                .WithMany(x => x.CommandPermissions)
                .HasForeignKey(x => x.CommandId);

            HasRequired(x => x.Role)
                .WithMany()
                .HasForeignKey(x => x.RoleId)
                .WillCascadeOnDelete(false);
        }
    }
}

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ReMi.Common.Constants;
using ReMi.Common.Utils.Enums;

namespace ReMi.DataEntities.Auth
{
    [Table("Roles", Schema = Constants.AuthSchemaName)]
    public class Role : EnumDescription
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override int Id { get; set; }

        [Index(IsUnique = true)]
        public Guid ExternalId { get; set; }

        public override string ToString()
        {
            return string.Format("[Id={0}, Name={1}, Decsription={2}, ExternalId={3}]",
                Id, Name, Description, ExternalId);
        }
    }
}

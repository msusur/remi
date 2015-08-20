using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReMi.DataEntities.Auth;
using ReMi.DataEntities.ReleaseCalendar;

namespace ReMi.DataEntities.ReleaseExecution
{
    [Table("SignOffs", Schema = Constants.SchemaName)]
    public class SignOff
    {
        [Key]
        public int SignOffId { get; set; }

        [Required]
        public int AccountId { get; set; }

        [Required]
        public int ReleaseWindowId { get; set; }

        [Required, Index(IsUnique = true)]
        public Guid ExternalId { get; set; }

        public DateTime? SignedOff { get; set; }

        public String Comment { get; set; }

        public virtual Account Account { get; set; }
        public virtual ReleaseWindow ReleaseWindow { get; set; }

        public override string ToString()
        {
            return
                String.Format(
                    "[SignOffId={0}, AccountId={1}, ReleaseWindowId={2}, ExternalId={3}, SignedOff={4}, Comment={5}]",
                    SignOffId, AccountId, ReleaseWindowId, ExternalId, SignedOff.HasValue, Comment);
        }
    }
}

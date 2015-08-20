using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReMi.DataEntities.SourceControl
{
    [Table("SourceControlChanges", Schema = Constants.SchemaName)]
    public class SourceControlChange
    {
        [Key]
        public int SourceControlChangeId { get; set; }
        [StringLength(256), Required]
        public string Owner { get; set; }
        [StringLength(2048), Required]
        public string Description { get; set; }
        [StringLength(256), Required, Index(IsUnique = true)]
        public string Identifier { get; set; }
        [StringLength(256), Required]
        public string Repository { get; set; }

        public DateTime? Date { get; set; }

        public virtual ICollection<SourceControlChangeToReleaseWindow> SourceControlChangeToReleaseWindows { get; set; }

        public override string ToString()
        {
            return string.Format("[SourceControlChangeId={0}, Owner={1}, Description={2}, Identifier={3}, Repository={4}, Date={5}]",
                SourceControlChangeId, Owner, Description, Identifier, Repository, Date);
        }
    }
}

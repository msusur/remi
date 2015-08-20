using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ReMi.DataEntities.ReleaseCalendar;

namespace ReMi.DataEntities.SourceControl
{
    [Table("SourceControlChangesToReleaseWindows", Schema = Constants.SchemaName)]
    public class SourceControlChangeToReleaseWindow
    {
        [Key]
        public int SourceControlChangeToProductId { get; set; }

        [Required]
        public int SourceControlChangeId { get; set; }
        [Required]
        public int ReleaseWindowId { get; set; }

        public virtual ReleaseWindow ReleaseWindow { get; set; }
        public virtual SourceControlChange Change { get; set; }

        public override string ToString()
        {
            return String.Format("[SourceControlChangeToProductId={0},SourceControlChangeId={1},ReleaseWindowId={2}]",
                SourceControlChangeToProductId, SourceControlChangeId, ReleaseWindowId);
        }
    }
}

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReMi.DataEntities.Reports
{
    [Table("ReportColumns", Schema = Constants.ReportSchemaName)]
    public class ReportColumn
    {
        [Key]
        public int ReportColumnId { get; set; }
        [Required, StringLength(256), Index("IX_ReportColumnOrder", 3, IsUnique = true)]
        public String Name { get; set; }
        [Required, Index("IX_ReportColumnOrder", 1, IsUnique = true)]
        public int ReportDescriptionId { get; set; }
        [Required, Index("IX_ReportColumnOrder", 2, IsUnique = true)]
        public int Order { get; set; }

        public override string ToString()
        {
            return String.Format("[ReportColumnId={3},Name={0},ReportDescriptionId={1},Order={2}]", Name,
                ReportDescriptionId, Order, ReportColumnId);
        }
    }
}

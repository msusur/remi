using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace ReMi.DataEntities.Reports
{
    [Table("ReportDescriptions", Schema = Constants.ReportSchemaName)]
    public class ReportDescription
    {
        [Key]
        public int ReportDescriptionId { get; set; }
        [Required, Index(IsUnique = true), StringLength(256)]
        public String Name { get; set; }
        [Required, StringLength(256)]
        public String ProcedureName { get; set; }

        public virtual ICollection<ReportColumn> ReportColumns { get; set; }
        public virtual ICollection<ReportParameter> ReportParameters { get; set; }

        public override string ToString()
        {
            return String.Format("[ReportDescriptionId={2},Name={0}, ProcedureName={1}]", Name, ProcedureName, ReportDescriptionId);
        }
    }
}

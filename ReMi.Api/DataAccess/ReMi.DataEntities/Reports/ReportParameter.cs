using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReMi.DataEntities.Reports
{
    [Table("ReportParameters", Schema = Constants.ReportSchemaName)]
    public class ReportParameter
    {
        [Key]
        public int ReportParameterId { get; set; }
        [Required, StringLength(256), Index("IX_ReportParameter", 2, IsUnique = true)]
        public String Name { get; set; }
        [Required, Index("IX_ReportParameter", 1, IsUnique = true)]
        public int ReportDescriptionId { get; set; }

        [Required, StringLength(256)]
        public String Description { get; set; }

        /// <summary>
        /// Sql type
        /// </summary>
        [Required, StringLength(256)]
        public String Type { get; set; }

        public override string ToString()
        {
            return String.Format("ReportParameterId={0}, Name={1}, ReportDescriptionId={2}, Description={3}, Type={4}",
                ReportParameterId, Name, ReportDescriptionId, Description, Type);
        }
    }
}

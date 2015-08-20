using ReMi.Common.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace ReMi.DataEntities.Products
{
    [Table("BusinessUnit", Schema = Constants.SchemaName)]
    public class BusinessUnit
    {
        [Key]
        public int BusinessUnitId { get; set; }

        [Index(IsUnique = true)]
        public Guid ExternalId { get; set; }

        [StringLength(128), Index(IsUnique = true)]
        public string Name { get; set; }

        public string Description { get; set; }

        public virtual ICollection<Product> Packages { get; set; }

        public override string ToString()
        {
            return
                string.Format(
                    "[BusinessUnitId={0}, ExternalId={1}, Name={2}, Description={3}, Packages=[{4}]]",
                    BusinessUnitId, ExternalId, Name, Description,
                    Packages.IsNullOrEmpty() ? string.Empty : string.Join(", ", Packages.Select(x => x.Description)));
        }
    }
}

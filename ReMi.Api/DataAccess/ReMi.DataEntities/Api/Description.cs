using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReMi.DataEntities.Api
{
    [Table("Descriptions", Schema = Constants.ApiSchemaName)]
    public class Description
    {
        [Key]
        public int DescriptionId { get; set; }

        public String DescriptionText { get; set; }

        [StringLength(512), Required]
        public String Url { get; set; }

        [StringLength(16), Required]
        public String HttpMethod { get; set; }

        public override string ToString()
        {
            return String.Format("[DescriptionId={0}, DescriptionText={1}, Url={2}, HttpMethod={3}]", DescriptionId, DescriptionText, Url,
                HttpMethod);
        }
    }
}

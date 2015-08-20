using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ReMi.DataEntities.Products;

namespace ReMi.DataEntities.DeploymentTool
{
    [Table("GoPipelineConfigurations", Schema = Constants.SchemaName)]
    public class GoPipelineConfiguration
    {
        [Key]
        public int GoPipelineConfigurationId { get; set; }

        [Index(IsUnique = true)]
        public Guid ExternalId { get; set; }

        [MaxLength(1024), Required]
        public string Name { get; set; }

        public int ProductId { get; set; }

        public bool IsIncludedByDefault { get; set; }
         
        public bool IsDisabled { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }

        public override string ToString()
        {
            return string.Format("[GoPipelineConfigurationId={0}, Name={1}, ProductId={2}, IsIncludedByDefault={3}, IsDisabled={4}, ExternalId={5}]",
                GoPipelineConfigurationId, Name, ProductId, IsIncludedByDefault, IsDisabled, ExternalId);
        }
    }
}

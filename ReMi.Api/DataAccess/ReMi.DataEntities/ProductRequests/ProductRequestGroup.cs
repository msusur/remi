using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReMi.DataEntities.ProductRequests
{
    [Table("ProductRequestGroups", Schema = Constants.SchemaName)]
    public class ProductRequestGroup
    {
        #region scalar props

        [Key]
        public int ProductRequestGroupId { get; set; }

        public int ProductRequestTypeId { get; set; }

        [Index(IsUnique = true)]
        public Guid ExternalId { get; set; }

        [MaxLength(1024), Required]
        public string Name { get; set; }

        #endregion

        #region navigational props

        [ForeignKey("ProductRequestTypeId")]
        public virtual ProductRequestType RequestType { get; set; }

        public virtual ICollection<ProductRequestTask> RequestTasks { get; set; }

        public virtual ICollection<ProductRequestGroupAssignee> Assignees { get; set; }

        #endregion

        public override string ToString()
        {
            return string.Format("[ProductRequestGroupId={0}, ProductRequestTypeId={1}, Name={2}, ExternalId={3}]",
                ProductRequestGroupId, ProductRequestTypeId, Name, ExternalId);
        }
    }
}

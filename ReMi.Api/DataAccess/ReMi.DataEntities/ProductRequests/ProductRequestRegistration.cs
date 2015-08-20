using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ReMi.DataEntities.Auth;

namespace ReMi.DataEntities.ProductRequests
{
    [Table("ProductRequestRegistrations", Schema = Constants.SchemaName)]
    public class ProductRequestRegistration
    {
        [Key]
        public int ProductRequestRegistrationId { get; set; }

        [Index(IsUnique = true)]
        public Guid ExternalId { get; set; }

        public int ProductRequestTypeId { get; set; }

        [StringLength(1024), Required]
        public string Description { get; set; }

        public DateTime CreatedOn { get; set; }

        public int CreatedByAccountId { get; set; }

        public bool? Deleted { get; set; }

        public virtual ProductRequestRegistrationRemovingReason RemovingReason { get; set; }

        #region navigational props

        [ForeignKey("ProductRequestTypeId")]
        public virtual ProductRequestType ProductRequestType { get; set; }

        public virtual ICollection<ProductRequestRegistrationTask> Tasks { get; set; }

        [ForeignKey("CreatedByAccountId")]
        public virtual Account CreatedBy { get; set; }

        #endregion

        public override string ToString()
        {
            return string.Format("[ProductRequestRegistrationId={0}, ExternalId={1}, ProductRequestTypeId={2}, Description={3}, CreatedOn={4}, CreatedByAccountId={5}, Deleted={6}]",
                ProductRequestRegistrationId, ExternalId, ProductRequestTypeId, Description, CreatedOn, CreatedByAccountId, Deleted);
        }
    }
}

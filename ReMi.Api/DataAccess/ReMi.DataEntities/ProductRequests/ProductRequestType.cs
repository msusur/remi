using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReMi.DataEntities.ProductRequests
{
    [Table("ProductRequestTypes", Schema = Constants.SchemaName)]
    public class ProductRequestType
    {
        #region scalar props

        [Key]
        public int ProductRequestTypeId { get; set; }

        [Index(IsUnique = true)]
        public Guid ExternalId { get; set; }

        [MaxLength(1024), Required]
        public string Name { get; set; }

        #endregion

        #region navigational props

        public virtual ICollection<ProductRequestGroup> RequestGroups { get; set; }

        public virtual ICollection<ProductRequestRegistration> Registrations { get; set; }

        #endregion

        public override string ToString()
        {
            return string.Format("[ProductRequestTypeId={0}, Name={1}, ExternalId={2}]",
                ProductRequestTypeId, Name, ExternalId);
        }
    }
}

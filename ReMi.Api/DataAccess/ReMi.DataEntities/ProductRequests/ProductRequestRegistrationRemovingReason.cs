using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ReMi.Common.Constants.ProductRequests;

namespace ReMi.DataEntities.ProductRequests
{
    [Table("ProductRequestRegistrationRemovingReasons", Schema = Constants.SchemaName)]
    public class ProductRequestRegistrationRemovingReason
    {
        [Key]
        public int ProductRequestRegistrationId { get; set; }

        public RemovingReason RemovingReason { get; set; }

        public string Comment { get; set; }

        public virtual ProductRequestRegistration ProductRequestRegistration { get; set; }

        public override string ToString()
        {
            return string.Format("[ProductRequestRegistrationId={0}, RemovingReason={1}, Comment={2}]",
                ProductRequestRegistrationId, RemovingReason, Comment);
        }
    }
}

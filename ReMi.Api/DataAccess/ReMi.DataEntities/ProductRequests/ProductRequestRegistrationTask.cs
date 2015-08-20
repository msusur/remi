using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ReMi.DataEntities.Auth;

namespace ReMi.DataEntities.ProductRequests
{
    [Table("ProductRequestRegistrationTasks", Schema = Constants.SchemaName)]
    public class ProductRequestRegistrationTask
    {
        [Key]
        public int ProductRequestRegistrationTaskId { get; set; }

        public int ProductRequestRegistrationId { get; set; }

        public int ProductRequestTaskId { get; set; }

        public bool IsCompleted { get; set; }

        public string Comment { get; set; }

        public int? LastChangedByAccountId { get; set; }

        public DateTime? LastChangedOn { get; set; }


        #region navigational props

        [ForeignKey("LastChangedByAccountId")]
        public virtual Account LastChangedBy { get; set; }

        [ForeignKey("ProductRequestRegistrationId")]
        public virtual ProductRequestRegistration ProductRequestRegistration { get; set; }

        [ForeignKey("ProductRequestTaskId")]
        public virtual ProductRequestTask ProductRequestTask { get; set; }

        #endregion

        public override string ToString()
        {
            return string.Format("[ProductRequestRegistrationTaskId={0}, IsCompleted={1}, ProductRequestRegistrationId={2}, " +
                "ProductRequestTaskId={3}, LastChangedByAccountId={4}, LastChangedOn={5}, Comment={6}]",
                ProductRequestRegistrationTaskId, IsCompleted, ProductRequestRegistrationId,
                ProductRequestTaskId, LastChangedByAccountId, LastChangedOn, Comment);
        }
    }
}

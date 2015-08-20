using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReMi.DataEntities.ProductRequests
{
    [Table("ProductRequestTasks", Schema = Constants.SchemaName)]
    public class ProductRequestTask
    {
        #region scalar props

        [Key]
        public int ProductRequestTaskId { get; set; }

        public int ProductRequestGroupId { get; set; }

        [Index(IsUnique = true)]
        public Guid ExternalId { get; set; }

        [Required]
        public string Question { get; set; }

        #endregion

        #region navigational props

        [ForeignKey("ProductRequestGroupId")]
        public virtual ProductRequestGroup RequestGroup { get; set; }

        public virtual ICollection<ProductRequestRegistrationTask> RegistrationTasks { get; set; }

        #endregion

        public override string ToString()
        {
            return string.Format("[ProductRequestTaskId={0}, Question={1}, ExternalId={2}]",
                ProductRequestTaskId, Question, ExternalId);
        }
    }
}

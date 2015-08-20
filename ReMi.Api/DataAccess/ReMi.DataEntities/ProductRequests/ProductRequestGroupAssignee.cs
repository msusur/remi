using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ReMi.DataEntities.Auth;

namespace ReMi.DataEntities.ProductRequests
{
    [Table("ProductRequestGroupAssignees", Schema = Constants.SchemaName)]
    public class ProductRequestGroupAssignee
    {
        #region scalar props

        [Key]
        public int ProductRequestGroupAssigneeId { get; set; }

        public int ProductRequestGroupId { get; set; }

        public int AccountId { get; set; }

        #endregion

        #region navigational props

        [ForeignKey("ProductRequestGroupId")]
        public virtual ProductRequestGroup RequestGroup { get; set; }

        [ForeignKey("AccountId")]
        public virtual Account Account { get; set; }

        #endregion

        public override string ToString()
        {
            return string.Format("[ProductRequestGroupAssigneeId={0}, ProductRequestGroupId={1}, AccountId={2}]",
                ProductRequestGroupAssigneeId, ProductRequestGroupId, AccountId);
        }
    }
}

using ReMi.DataEntities.Products;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReMi.DataEntities.Auth
{
    public class AccountProduct
    {
        #region .ctor

        public AccountProduct()
        {
        }

        #endregion

        #region scalar props

        public int AccountProductId { get; set; }

        public int AccountId { get; set; }

        public int ProductId { get; set; }

        public DateTime CreatedOn { get; set; }

        public bool IsDefault { get; set; }

        #endregion

        #region navigational props

        public virtual Account Account { get; set; }
        public virtual Product Product { get; set; }

        #endregion

        public override string ToString()
        {
            return string.Format("[AccountProductId = {0}, AccountId = {1}, ProductId = {2}, CreatedOn = {3}]",
                AccountProductId, AccountId, ProductId, CreatedOn);
        }
    }
}

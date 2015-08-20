using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ReMi.DataEntities.Products;
using System;

namespace ReMi.DataEntities.ReleaseCalendar
{
    [Table("ReleaseProducts", Schema = Constants.SchemaName)]
    public class ReleaseProduct
    {
        #region scalar props

        [Key]
        public int ReleaseProductId { get; set; }

        [Index("IX_ReleaseProductPair", 1, IsUnique = true)]
        public int ProductId { get; set; }

        [Index("IX_ReleaseProductPair", 2, IsUnique = true)]
        public int ReleaseWindowId { get; set; }

        public DateTime CreatedOn { get; set; }

        #endregion

        #region navigational props

        public virtual ReleaseWindow ReleaseWindow { get; set; }

        public virtual Product Product { get; set; }

        #endregion


        public override string ToString()
        {
            return
                string.Format(
                    "[ReleaseProductId={0}, ProductId={1}, ReleaseWindowId={2}, CreatedOn={3}]",
                    ReleaseProductId, ProductId, ReleaseWindowId, CreatedOn);
        }
    }
}

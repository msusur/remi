using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReMi.DataEntities.Products
{
    public class ProductJiraBoard
    {
        #region scalar props

        public int ProductJiraBoardId { get; set; }

        [Index(IsUnique=true)]
        public Guid ExternalId { get; set; }

        [Index(IsUnique=true)]
        [MaxLength(256)]
        public string ProductJiraBoardName { get; set; }

        public int ProductId { get; set; }

        public string JiraBoardFilter { get; set; }

        public string DefectFilter { get; set; }

        #endregion

        #region navigational props

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }

        #endregion

        public override string ToString()
        {
            return
                string.Format(
                    "[ProductJiraBoardName = {0}, ProductId = {1}, JiraBoardFilter={2}, DefectFilter={3}, ExternalId={4}]",
                    ProductJiraBoardName, ProductId, JiraBoardFilter, DefectFilter, ExternalId);
        }
    }
}

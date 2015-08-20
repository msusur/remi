using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReMi.DataEntities.Products
{
    [Table("ProductGoServers", Schema = Constants.SchemaName)]
    public class ProductGoServer
    {
        #region scalar props

        public int ProductGoServerId { get; set; }

        [Index(IsUnique = true)]
        public Guid ExternalId { get; set; }

        public int ProductId { get; set; }

        public string GoServerUrl { get; set; }

        #endregion

        #region navigational props

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }

        #endregion

        public override string ToString()
        {
            return string.Format("[ProductGoServerId={0}, ProductId = {1}, GoServerUrl={2}, ExternalId={3}]",
                ProductGoServerId, ProductId, GoServerUrl);
        }
    }
}

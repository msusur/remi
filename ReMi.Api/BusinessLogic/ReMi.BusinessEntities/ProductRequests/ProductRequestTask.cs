using System;

namespace ReMi.BusinessEntities.ProductRequests
{
    public class ProductRequestTask
    {
        public Guid ExternalId { get; set; }
        public string Question { get; set; }

        public Guid ProductRequestGroupId { get; set; }

        public override string ToString()
        {
            return string.Format("[Question={0}, ExternalId={1}, ProductRequestGroupId={2}]",
                Question, ExternalId, ProductRequestGroupId);
        }
    }
}

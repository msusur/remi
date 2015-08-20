using System;
using System.Collections.Generic;
using ReMi.BusinessEntities.Products;
using ReMi.Common.Utils;

namespace ReMi.Queries.Configuration
{
    public class GetProductsResponse
    {
        public List<ProductView> Products { get; set; }

        public override string ToString()
        {
            return String.Format("[Products={0}]", Products.FormatElements());
        }
    }
}

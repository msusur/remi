using System;
using System.Collections.Generic;
using ReMi.BusinessEntities.ProductRequests;
using ReMi.Common.Utils;

namespace ReMi.Queries.ProductRequests
{
    public class GetProductRequestsResponse 
    {
        public IEnumerable<ProductRequestType> ProductRequestTypes { get; set; }

        public override string ToString()
        {
            return String.Format("[ProductRequestTypes={0}]", ProductRequestTypes.FormatElements());
        }
    }
}

using System;
using System.Collections.Generic;
using ReMi.Common.Utils;

namespace ReMi.BusinessEntities.ProductRequests
{
    public class ProductRequestType
    {
        public Guid ExternalId { get; set; }
        public string Name { get; set; }

        public IEnumerable<ProductRequestGroup> RequestGroups { get; set; }

        public override string ToString()
        {
            return string.Format("[Name={0}, ExternalId={1}, RequestGroups={2}]",
                Name, ExternalId, RequestGroups.FormatElements());
        }
    }
}

using ReMi.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ReMi.BusinessEntities.Products
{
    public class BusinessUnitView
    {
        public Guid ExternalId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public IEnumerable<ProductView> Packages { get; set; }

        public override string ToString()
        {
            return
                string.Format(
                    "[ExternalId={0}, Name={1}, Description={2}, Packages=[{3}]]",
                    ExternalId, Name, Description,
                    Packages.IsNullOrEmpty() ? string.Empty : string.Join(", ", Packages.Select(x => x.Name)));
        }
    }
}

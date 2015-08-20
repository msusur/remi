using ReMi.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ReMi.BusinessEntities.Products
{
    public class BusinessUnit
    {
        public Guid ExternalId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public IEnumerable<Product> Packages { get; set; }

        public override string ToString()
        {
            return
                string.Format(
                    "[ExternalId={0}, Name={1}, Description={2}, Packages=[{3}]]",
                    ExternalId, Name, Description,
                    Packages.IsNullOrEmpty() ? string.Empty : string.Join(", ", Packages.Select(x => x.Description)));
        }

        public override bool Equals(object obj)
        {
            return ExternalId.Equals(((BusinessUnit) obj).ExternalId);
        }

        public override int GetHashCode()
        {
            return ExternalId.GetHashCode();
        }
    }
}

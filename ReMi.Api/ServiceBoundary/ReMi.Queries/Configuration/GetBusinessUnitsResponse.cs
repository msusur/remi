using System;
using System.Collections.Generic;
using ReMi.BusinessEntities.Products;
using ReMi.Common.Utils;

namespace ReMi.Queries.Configuration
{
    public class GetBusinessUnitsResponse
    {
        public IEnumerable<BusinessUnitView> BusinessUnits { get; set; }

        public override string ToString()
        {
            return String.Format("[BusinessUnits={0}]", BusinessUnits.FormatElements());
        }
    }
}

using System.Collections.Generic;
using ReMi.BusinessEntities.ProductRequests;
using ReMi.Common.Utils;

namespace ReMi.Queries.ProductRequests
{
    public class GetProductRequestRegistrationsResponse
    {
        public IEnumerable<ProductRequestRegistration> Registrations { get; set; }

        public override string ToString()
        {
            return string.Format("[Registrations={0}]", Registrations.FormatElements());
        }
    }
}

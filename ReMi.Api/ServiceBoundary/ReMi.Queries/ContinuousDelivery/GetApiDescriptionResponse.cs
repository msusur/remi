using System.Collections.Generic;
using ReMi.BusinessEntities.Api;

namespace ReMi.Queries.ContinuousDelivery
{
    public class GetApiDescriptionResponse
    {
        public IEnumerable<ApiDescriptionFull> ApiDescriptions { get; set; }
    }
}

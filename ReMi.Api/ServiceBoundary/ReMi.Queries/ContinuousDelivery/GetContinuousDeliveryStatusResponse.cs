using ReMi.BusinessEntities.ContinuousDelivery;
using System.Collections.Generic;

namespace ReMi.Queries.ContinuousDelivery
{
    public class GetContinuousDeliveryStatusResponse
    {
        public IEnumerable<string> Products { get; set; }

        public IEnumerable<StatusCheckItem> StatusCheck { get; set; } 
    }
}

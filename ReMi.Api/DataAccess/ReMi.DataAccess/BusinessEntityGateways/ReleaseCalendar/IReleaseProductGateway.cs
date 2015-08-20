using System;
using System.Collections.Generic;

namespace ReMi.DataAccess.BusinessEntityGateways.ReleaseCalendar
{
    public interface IReleaseProductGateway : IDisposable
    {
        void AssignProductsToRelease(Guid releaseWindowId, IEnumerable<string> products);
    }
}

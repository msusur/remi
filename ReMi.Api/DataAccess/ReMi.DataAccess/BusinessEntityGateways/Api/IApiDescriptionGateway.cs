using System;
using System.Collections.Generic;
using ReMi.BusinessEntities.Api;

namespace ReMi.DataAccess.BusinessEntityGateways.Api
{
    public interface IApiDescriptionGateway : IDisposable
    {
        List<ApiDescription> GetApiDescriptions();

        void RemoveApiDescriptions(List<ApiDescription> descriptions);
        void CreateApiDescriptions(List<ApiDescription> descriptions);
        void UpdateApiDescription(ApiDescription description);
    }
}

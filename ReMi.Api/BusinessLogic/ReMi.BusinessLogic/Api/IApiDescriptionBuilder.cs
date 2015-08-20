using System;
using System.Collections.Generic;
using ReMi.BusinessEntities.Api;

namespace ReMi.BusinessLogic.Api
{
    public interface IApiDescriptionBuilder
    {
        IEnumerable<ApiDescription> GetApiDescriptions();
        String FormatType(Type type, int recursionLevel, Dictionary<int, String> levels);
    }
}

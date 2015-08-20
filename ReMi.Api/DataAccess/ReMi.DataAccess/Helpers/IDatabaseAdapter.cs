using System;
using System.Collections.Generic;
using ReMi.DataEntities.Reports;

namespace ReMi.DataAccess.Helpers
{
    public interface IDatabaseAdapter : IDisposable
    {
        List<List<String>> RunStoredProcedure(String query, int columns, IDictionary<string, object> parameters);
    }
}

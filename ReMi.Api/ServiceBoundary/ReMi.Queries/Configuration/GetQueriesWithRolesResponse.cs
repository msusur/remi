using ReMi.BusinessEntities.Api;
using System.Collections.Generic;

namespace ReMi.Queries.Configuration
{
    public class GetQueriesWithRolesResponse
    {
        public IEnumerable<Query> Queries { get; set; }
    }
}

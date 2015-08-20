using ReMi.BusinessEntities.Api;
using System.Collections.Generic;

namespace ReMi.Queries.Configuration
{
    public class GetCommandsByNamesResponse
    {
        public IEnumerable<Command> Commands { get; set; }
    }
}

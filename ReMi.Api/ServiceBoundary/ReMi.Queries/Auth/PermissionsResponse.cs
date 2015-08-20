using System;
using System.Collections.Generic;
using ReMi.Common.Utils;

namespace ReMi.Queries.Auth
{
    public class PermissionsResponse
    {
        public IEnumerable<String> Commands { get; set; }
        public IEnumerable<String> Queries { get; set; }

        public override string ToString()
        {
            return String.Format("[Commands={0}, Queries={1}]", Commands.FormatElements(), Queries.FormatElements());
        }
    }
}

using System;
using System.Collections.Generic;

namespace ReMi.DataAccess.Helpers
{
    public class CqrsPermissions
    {
        public String Name { get; set; }
        public IEnumerable<int> RoleIds { get; set; }
        public IEnumerable<string> RoleNames { get; set; }
    }
}

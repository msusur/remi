
using System.Collections.Generic;
using System.Linq;
using ReMi.BusinessEntities.Auth;

namespace ReMi.BusinessEntities.Api
{
    public class Query
    {
        public int QueryId { get; set; }

        public string Name { get; set; }

        public string Group { get; set; }

        public string Description { get; set; }

        public bool IsStatic { get; set; }

        public IEnumerable<Role> Roles { get; set; }

        public string Namespace { get; set; }

        public bool HasRuleApplied { get; set; }

        public override string ToString()
        {
            return string.Format("[QueryId={0}, Name={1}, Group={2}, Description={3}, IsStatic={4}, Roles.Count={5}, Namespace={6}, HasRuleApplied={7}]",
                QueryId, Name, Group, Description, IsStatic, Roles.Count(), Namespace, HasRuleApplied);
        }
    }
}

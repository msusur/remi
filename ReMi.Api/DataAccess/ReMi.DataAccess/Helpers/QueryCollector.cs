using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ReMi.Common.Utils.Enums;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.Queries.ExecPoll;

namespace ReMi.DataAccess.Helpers
{
    public static class QueryCollector
    {
        public static IEnumerable<QueryDescription> Collect()
        {
            var queryTypes = Assembly.GetAssembly(typeof(GetCommandStateRequest)).GetTypes()
                .Where(p => typeof(IQuery).IsAssignableFrom(p) && !p.IsAbstract && p.IsClass)
                .ToList();
            var missingQueryAttribute = queryTypes
                .Where(x => !Attribute.GetCustomAttributes(x).Any(a => a is QueryAttribute))
                .Select(x => x.FullName)
                .OrderBy(x => x)
                .ToList();
            if (missingQueryAttribute.Any())
                throw new ApplicationException(string.Format("Queries don't have attribut: [{0}]",
                    string.Join(Environment.NewLine, missingQueryAttribute)));

            return queryTypes.Select(x =>
            {
                var attr = (QueryAttribute)Attribute.GetCustomAttributes(x).First(a => a is QueryAttribute);
                return new QueryDescription
                {
                    Description = attr.Description,
                    Group = EnumDescriptionHelper.GetDescription(attr.Group),
                    Name = x.Name,
                    IsStatic = attr.IsStatic,
                    Namespace = x.Namespace
                };
            }).ToArray();
        }
    }
}

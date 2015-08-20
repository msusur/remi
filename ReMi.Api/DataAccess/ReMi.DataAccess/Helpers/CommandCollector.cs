using ReMi.Commands.ReleaseCalendar;
using ReMi.Common.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ReMi.Common.Utils.Enums;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.DataAccess.Helpers
{
    public static class CommandCollector
    {
        public static IEnumerable<CommandDescription> Collect()
        {
            var commandTypes = Assembly.GetAssembly(typeof(BasicReleaseWindowCommand)).GetTypes()
                .Where(p => typeof(ICommand).IsAssignableFrom(p) && !p.IsAbstract && p.IsClass)
                .ToList();

            var missingCommandAttribute = commandTypes
                .Where(x => !Attribute.GetCustomAttributes(x).Any(a => a is CommandAttribute))
                .Select(x => x.FullName)
                .OrderBy(x => x)
                .ToList();

            if (missingCommandAttribute.Any())
                throw new ApplicationException(string.Format("Command don't has attribut: [{0}]",
                    string.Join(Environment.NewLine, missingCommandAttribute)));

            return commandTypes.Select(x =>
            {
                var attr = (CommandAttribute) Attribute.GetCustomAttributes(x).First(a => a is CommandAttribute);
                return new CommandDescription
                {
                    Description = attr.Description,
                    Group = EnumDescriptionHelper.GetDescription(attr.Group),
                    Name = x.Name,
                    IsBackground = attr.IsBackground,
                    Namespace = x.Namespace
                };
            }).ToArray();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Web.Http;
using Autofac;
using ReMi.BusinessEntities.Api;
using ReMi.BusinessLogic.Api;
using ReMi.Common.Cqrs;
using ReMi.Contracts.Cqrs;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Api.Insfrastructure
{
    public class ApiDescriptionBuilder : IApiDescriptionBuilder
    {
        public const String IncorrectRoute = "Incorrect";

        private readonly IEnumerable<Type> _controllerTypes;
        private readonly IEnumerable<Type> _commandTypes;

        public ApiDescriptionBuilder(IEnumerable<Type> controllerTypes, IEnumerable<Type> commandTypes)
        {
            _controllerTypes = controllerTypes;
            _commandTypes = commandTypes;
        }

        public IEnumerable<ApiDescription> GetApiDescriptions()
        {
            var descriptions = new List<ApiDescription>();

            foreach (var controller in _controllerTypes)
            {
                var route = GetRoutePrefix(controller);

                var methods = controller.GetMethods().Where(m => m.IsPublic).ToList();

                descriptions.AddRange(
                    FillApiDescriptions(methods, route, _commandTypes.ToList()));
            }

            return descriptions
                .Where(o => !string.IsNullOrWhiteSpace(o.Name))
                .OrderBy(o => o.Method)
                .ThenBy(o => o.Name)
                .ToList();
        }

        public String FormatType(Type type, int recursionLevel, Dictionary<int, String> levels)
        {
            if (type == null)
            {
                return String.Empty;
            }

            var isEnumerable = IsEnumerable(type);

            levels = ManageLevels(levels, isEnumerable, recursionLevel, type);

            var props = GetProperties(isEnumerable, type).ToList();

            if ((!isEnumerable && (!props.Any() || type.FullName.StartsWith("System."))) || type.IsEnum)
            {
                var value = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)
                    ? FormatNullable(type)
                    : type.Name;

                return String.Format("\"{0}\"", value);
            }

            var result = new StringBuilder();

            recursionLevel++;

            result.Append(isEnumerable ? "[ {" : "{");

            result.Append(
                FormatProperties(props, levels, recursionLevel));

            result.Append(isEnumerable ? "} ]" : "}");

            return result.ToString();
        }

        private static String FormatNullable(Type type)
        {
            return type.GetGenericArguments()[0].Name + "?";
        }

        private static bool CheckNestingTree(String typeName, int recursionLevel, Dictionary<int, String> levels)
        {
            for (var counter = 0; counter < recursionLevel; counter++)
            {
                if (levels[counter] == typeName)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsEnumerable(Type type)
        {
            return type.IsGenericType
                && (type.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                    || type.GetInterfaces().Any(t => t.GetGenericTypeDefinition() == typeof(IEnumerable<>)));
        }

        private static String GetRouteAttribute(MethodInfo method, String route)
        {
            var actionRoute =
                method.GetCustomAttributesData()
                    .FirstOrDefault(z => z.AttributeType == typeof(RouteAttribute));

            if (actionRoute == null)
                return IncorrectRoute;

            return actionRoute.ConstructorArguments.Count == 0
                ? route
                : String.Format("{0}{1}/", route, actionRoute.ConstructorArguments.First().Value);
        }

        private ApiDescription CreateCommandDescription(String url, Type type)
        {
            if (type != null)
            {
                var commandAttrs = type.GetCustomAttributes(typeof(CommandAttribute)).ToArray();
                if (commandAttrs.Any())
                {
                    if (((CommandAttribute)commandAttrs[0]).IsBackground)
                        return null;
                }
            }

            var desc = new ApiDescription
            {
                Url = url,
                Name = type == null ? string.Empty : type.Name,
                Method = HttpMethod.Post.Method,
                InputFormat = FormatType(type, 0, null)
            };

            return desc;
        }

        private static String GetRoutePrefix(Type type)
        {
            var routePrefix =
                               type.GetCustomAttributesData()
                                   .FirstOrDefault(z => z.AttributeType == typeof(RoutePrefixAttribute));

            var route = "/";
            if (routePrefix != null)
            {
                route += String.Format("{0}/", routePrefix.ConstructorArguments.First().Value);
            }

            return route;
        }

        private ApiDescription CreateQueryApiDescription(String url, MethodInfo method)
        {
            var inputFormat = String.Join(", ",
                method.GetParameters()
                    .Select(
                        p => String.Format("\"{0}\" : \"{1}\"", p.Name, p.ParameterType.Name)));

            if (!string.IsNullOrWhiteSpace(inputFormat))
                inputFormat = "{" + inputFormat + "}";

            var desc = new ApiDescription
            {
                OutputFormat = FormatType(method.ReturnType, 0, null),
                Method = HttpMethod.Get.Method,
                Name = method.ReturnType.Name.Replace("Response", string.Empty),
                Url = url,
                InputFormat = inputFormat,
            };

            return desc;
        }

        private static Dictionary<int, String> ManageLevels(Dictionary<int, String> levels, bool isEnumerable, int recursionLevel, Type type)
        {
            if (levels == null)
            {
                levels = new Dictionary<int, String>();
            }

            while (levels.Count > recursionLevel)
            {
                levels.Remove(levels.Last().Key);
            }

            levels.Add(recursionLevel, isEnumerable ? type.GetGenericArguments()[0].FullName : type.FullName);

            return levels;
        }

        private string FormatDictionary(PropertyInfo prop, Type type)
        {
            var genericArguments = prop.PropertyType.GetGenericArguments();
            if (genericArguments == null || genericArguments.Length < 2)
                throw new ArgumentException("Generic arguments are invalid", "prop");

            var value = !string.IsNullOrEmpty(genericArguments[1].Namespace)
                    && genericArguments[1].Namespace.StartsWith("System.")
                    && !IsEnumerable(type.GetGenericArguments()[1])
                ? genericArguments[1].FullName
                : FormatType(genericArguments[1], 0, null);

            return
                String.Format("\"Key\" : \"{0}\", \"Value\" : {1}",
                    genericArguments[0].Name, value);
        }

        private static bool SimpleTypeCheck(PropertyInfo prop, Dictionary<int, String> levels, int recursionLevel)
        {
            return !prop.PropertyType.IsEnum
                && !CheckNestingTree(
                       IsEnumerable(prop.PropertyType)
                           ? prop.PropertyType.GetGenericArguments()[0].FullName
                           : prop.PropertyType.FullName, recursionLevel, levels)
                && (!prop.PropertyType.FullName.StartsWith("System.") || IsEnumerable(prop.PropertyType));
        }

        private static IEnumerable<PropertyInfo> GetProperties(bool isEnumerable, Type type)
        {
            return isEnumerable
                ? type.GetGenericArguments()[0].GetProperties(BindingFlags.Public | BindingFlags.Instance)
                : type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        }

        private IEnumerable<ApiDescription> FillApiDescriptions(IEnumerable<MethodInfo> methods, String route, IList<Type> commandTypes)
        {
            var result = new List<ApiDescription>();

            foreach (var method in methods)
            {
                var url = GetRouteAttribute(method, route);

                if (url == IncorrectRoute)
                {
                    continue;
                }

                if (method.GetCustomAttributes(typeof(HttpGetAttribute)).Any())
                {
                    result.Add(CreateQueryApiDescription(url, method));
                    continue;
                }

                if (method.GetCustomAttributes(typeof(HttpPostAttribute)).Any())
                {
                    if (url.Contains("deliver/{commandName}"))
                    {
                        result.AddRange(
                            commandTypes
                                .Select(t => CreateCommandDescription(url.Replace("{commandName}", t.Name), t))
                                .Where(t => t != null)
                                .ToList());
                    }
                    else
                    {
                        var arguments = method.GetParameters();
                        var commandDescription = CreateCommandDescription(url, arguments.Length > 0 ? arguments[0].ParameterType : null);

                        if (commandDescription != null)
                            result.Add(commandDescription);
                    }
                }
            }

            return result;
        }

        private string FormatProperties(IEnumerable<PropertyInfo> props, Dictionary<int, String> levels, int recursionLevel)
        {
            var propertyLines = new List<string>();

            foreach (var prop in props)
            {
                if (prop.PropertyType.IsAssignableTo<BaseContext>())
                {
                    continue;
                }

                if ((typeof(IDictionary<,>)).IsAssignableFrom(prop.PropertyType) || (typeof(IDictionary<,>)).Name == prop.PropertyType.Name)
                {
                    propertyLines.Add(
                        String.Format("\"{0}\" : [ {{{1}}} ]",
                            prop.Name, FormatDictionary(prop, prop.PropertyType))
                    );

                    continue;
                }

                if (SimpleTypeCheck(prop, levels, recursionLevel))
                {
                    propertyLines.Add(
                        String.Format("\"{0}\" : {1}",
                            prop.Name,
                            IsEnumerable(prop.PropertyType)
                                ? String.Format("[ {{ \"{0}\" : \"{0}\" }} ]", prop.PropertyType.GetGenericArguments()[0].Name)
                                : String.Format("\"{0}\"", prop.PropertyType.Name)));
                    continue;
                }

                propertyLines.Add(
                    String.Format("\"{0}\" : {1}",
                        prop.Name,
                        FormatType(prop.PropertyType, recursionLevel, levels)
                    )
                );
            }

            return string.Join(", ", propertyLines);
        }
    }
}

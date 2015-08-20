using System;
using System.Linq;
using System.Text;
using Common.Logging;
using CSScriptLibrary;
using ReMi.BusinessEntities.Auth;
using ReMi.BusinessEntities.BusinessRules;
using ReMi.BusinessEntities.Exceptions;
using System.Collections.Generic;
using ReMi.Common.Constants.BusinessRules;
using ReMi.Common.Utils;
using ReMi.DataAccess.BusinessEntityGateways.Auth;
using ReMi.DataAccess.BusinessEntityGateways.BusinessRules;

namespace ReMi.BusinessLogic.BusinessRules
{
    public class BusinessRuleEngine : IBusinessRuleEngine
    {
        private const string Parameter = "var {0} = ({1})parameters[\"{0}\"];";
        private readonly static string[] References =
        {
            "ReMi.BusinessEntities",
            "ReMi.Common.Enums"
        };

        private const string Usings = @"
            using System;
            using System.Linq;
            using System.Collections.Generic;";

        private const string ScriptWrapper = @"
            #{Usings}

            public class Script : ReMi.BusinessLogic.BusinessRules.IBusinessRuleScript
            {
                public object Execute(ReMi.BusinessEntities.Auth.Account account, IDictionary<string, object> parameters)
                {
                    #{parameters}
                    #{script}
                }
            }";

        public IApplicationSettings ApplicationSettings { get; set; }
        public Func<IBusinessRuleGateway> BusinessRuleGatewayFactory { get; set; }
        public Func<IAccountsGateway> AccountGatewayFactory { get; set; }
        public ISerialization Serialization { get; set; }
        private static readonly ILog Logger = LogManager.GetCurrentClassLogger();

        public object Execute(Guid userId, BusinessRuleGroup group, string rule, IDictionary<string, string> parameters)
        {
            var ruleDescription = GetRuleDescription(group, rule);
            var account = GetAccount(userId);

            return Execute(account, ruleDescription, parameters);
        }

        public object Execute(Guid userId, Guid ruleId, IDictionary<string, string> parameters)
        {
            var ruleDescription = GetRuleDescription(ruleId);
            var account = GetAccount(userId);

            return Execute(account, ruleDescription, parameters);
        }

        public T Execute<T>(Guid userId, Guid ruleId, IDictionary<string, object> parameters)
        {
            var ruleDescription = GetRuleDescription(ruleId);
            Validate(true, ruleDescription, parameters);
            var account = GetAccount(userId);

            return (T)Execute(account, ruleDescription, parameters);
        }

        public object Test(BusinessRuleDescription ruleDescription)
        {
            var account = GetAccount(ruleDescription);
            var parameters = GetParameters(ruleDescription);

            return Execute(account, ruleDescription, parameters);
        }

        public Type GetType(string type)
        {
            var ruleDescription = new BusinessRuleDescription
            {
                Script = string.Format("return typeof({0});", type)
            };
            var script = BuildScript(ruleDescription);
            var scriptExecution = Compile(ruleDescription, script);

            return (Type)scriptExecution.Execute(null, null);
        }

        private Account GetAccount(Guid userId)
        {
            if (userId == Guid.Empty) return null;
            using (var gateway = AccountGatewayFactory())
            {
                return gateway.GetAccount(userId, true);
            }
        }

        private object Execute(Account account, BusinessRuleDescription ruleDescription, IDictionary<string, string> parameters)
        {
            Validate(false, ruleDescription, parameters.ToDictionary(x => x.Key, x => (object)x.Value));

            var convertedParameters = ConvertParameters(parameters, ruleDescription, Serialization);

            return Execute(account, ruleDescription, convertedParameters);
        }

        private object Execute(Account account, BusinessRuleDescription ruleDescription, IDictionary<string, object> convertedParameters)
        {
            var script = BuildScript(ruleDescription);
            var scriptExecution = Compile(ruleDescription, script);

            Logger.InfoFormat("Business Rule '{0}/{1}' Parameters: {2}", ruleDescription.Group, ruleDescription.Name,
                FormatLogHelper.FormatEntry(Serialization, ApplicationSettings, convertedParameters));
            Logger.InfoFormat("Executing Business Rule: {0}/{1}{2}", ruleDescription.Group, ruleDescription.Name, script);

            var result = scriptExecution.Execute(account, convertedParameters);

            Logger.InfoFormat("Business Rule '{0}/{1}' returned result: {2}", ruleDescription.Group, ruleDescription.Name, result);

            return result;
        }

        private BusinessRuleDescription GetRuleDescription(Guid ruleId)
        {
            using (var gateway = BusinessRuleGatewayFactory())
            {
                return gateway.GetBusinessRule(ruleId);
            }
        }

        private BusinessRuleDescription GetRuleDescription(BusinessRuleGroup group, string rule)
        {
            using (var gateway = BusinessRuleGatewayFactory())
            {
                return gateway.GetBusinessRule(group, rule);
            }
        }

        private static IBusinessRuleScript Compile(BusinessRuleDescription ruleDescription, string script)
        {
            try
            {
                return (IBusinessRuleScript)CSScript.LoadCode(script, References).CreateObject("*");
            }
            catch (Exception ex)
            {
                throw new BusinessRuleCompilationException(ruleDescription.Group, ruleDescription.Description, script, ex.Message, ex);
            }
        }

        private IDictionary<string, object> ConvertParameters(IDictionary<string, string> parameters,
            BusinessRuleDescription ruleDescription, ISerialization serialization)
        {
            if (ruleDescription == null)
                return null;

            if (ruleDescription.Parameters.IsNullOrEmpty())
                return null;



            var convertedParameters = new Dictionary<string, object>();
            foreach (var parameter in ruleDescription.Parameters)
            {
                var stringValue = parameters[parameter.Name];
                if (GetType(parameter.Type).FullName == "System.String")
                {
                    convertedParameters.Add(parameter.Name, stringValue);
                    continue;
                }
                try
                {
                    convertedParameters.Add(parameter.Name, serialization.FromJson(stringValue, GetType(parameter.Type)));
                }
                catch (Exception ex)
                {
                    throw new BusinessRuleParamterTypeMissmatchException(ruleDescription.Group, ruleDescription.Description,
                        parameter.Name, GetType(parameter.Type).FullName, parameters[parameter.Name].GetType().FullName, ex);
                }
            }
            return convertedParameters;
        }

        private void Validate(bool validateType,
            BusinessRuleDescription ruleDescription, IDictionary<string, object> parameters)
        {
            if (ruleDescription == null || string.IsNullOrEmpty(ruleDescription.Script))
            {
                throw new BusinessRuleEmptyException();
            }

            if (ruleDescription.Parameters.IsNullOrEmpty() && parameters.IsNullOrEmpty()) return;

            if ((ruleDescription.Parameters.IsNullOrEmpty() && !parameters.IsNullOrEmpty())
                || (parameters.IsNullOrEmpty() && !ruleDescription.Parameters.IsNullOrEmpty())
                || (parameters != null && ruleDescription.Parameters.Count() != parameters.Count))
            {
                throw new BusinessRuleParamtersMissmatchException(ruleDescription.Group, ruleDescription.Description,
                    parameters == null ? new string[0] : parameters.Keys.ToArray(),
                    ruleDescription.Parameters == null ? new string[0] : ruleDescription.Parameters.Select(x => x.Name));
            }

            if (parameters != null && !ruleDescription.Parameters.All(x => parameters.ContainsKey(x.Name)))
            {
                throw new BusinessRuleParamtersMissmatchException(ruleDescription.Group, ruleDescription.Description,
                    parameters.Keys.ToArray(),
                    ruleDescription.Parameters.Select(x => x.Name));                
            }
            if (!validateType || parameters == null ||
                ruleDescription.Parameters.All(x => GetType(x.Type).IsInstanceOfType(parameters[x.Name]))) return;

            var parameter = ruleDescription.Parameters.First(x => !GetType(x.Type).IsInstanceOfType(parameters[x.Name]));
            throw new BusinessRuleParamterTypeMissmatchException(ruleDescription.Group, ruleDescription.Description,
                parameter.Name, GetType(parameter.Type).FullName, parameters[parameter.Name].GetType().FullName);
        }

        private static string BuildScript(BusinessRuleDescription ruleDescription)
        {
            var parameters = BuildParameters(ruleDescription.Parameters);

            return ScriptWrapper
                .Replace("#{Usings}", Usings)
                .Replace("#{parameters}", parameters)
                .Replace("#{script}", ruleDescription.Script);
        }

        private static string BuildParameters(IEnumerable<BusinessRuleParameter> parameters)
        {
            // ReSharper disable once PossibleMultipleEnumeration
            if (parameters.IsNullOrEmpty())
                return string.Empty;

            // ReSharper disable once PossibleMultipleEnumeration
            var businessRuleParameters = parameters as BusinessRuleParameter[] ?? parameters.ToArray();

            var builder = new StringBuilder();
            foreach (var parameter in businessRuleParameters)
            {
                builder.AppendFormat(Parameter, parameter.Name, parameter.Type).AppendLine();
            }

            return builder.ToString();
        }

        private static IDictionary<string, string> GetParameters(BusinessRuleDescription ruleDescription)
        {
            return ruleDescription.Parameters == null
                ? new Dictionary<string, string>()
                : ruleDescription.Parameters.ToDictionary(x => x.Name, x => x.TestData.JsonData);
        }

        private Account GetAccount(BusinessRuleDescription ruleDescription)
        {
            return Serialization.FromJson<Account>(ruleDescription.AccountTestData.JsonData);
        }
    }
}

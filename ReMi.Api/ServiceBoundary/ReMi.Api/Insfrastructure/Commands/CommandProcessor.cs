using Autofac.Integration.WebApi;
using Common.Logging;
using Newtonsoft.Json;
using ReMi.Common.Utils;
using ReMi.Common.WebApi.Exceptions;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dependencies;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Api.Insfrastructure.Commands
{
    public class CommandProcessor : ICommandProcessor
    {
        public IClientRequestInfoRetriever ClientRequestInfoRetriever { get; set; }

        private readonly IDependencyResolver _resolver = GlobalConfiguration.Configuration.DependencyResolver;

        private readonly ILog _log = LogManager.GetCurrentClassLogger();

        public HttpResponseMessage HandleRequest(HttpActionContext actionContext, string commandName, bool isSynchronous)
        {
            var command = RetrieveCommandExecution(actionContext.ControllerContext, commandName);

            if (command == null)
            {
                return actionContext.Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            try
            {
                command.CommandContext.IsSynchronous = isSynchronous;
                return ProcessCommnad(actionContext, command);
            }
            catch (Exception ex)
            {
                if (ex.InnerException is HttpResponseException)
                {
                    throw ex.InnerException;
                }
                _log.Error(ex);

                return actionContext.Request.CreateResponse(HttpStatusCode.BadRequest);
            }
        }

        private ICommand RetrieveCommandExecution(HttpControllerContext controllerContext, string commandName)
        {
            try
            {
                _log.DebugFormat(controllerContext.Request.Headers.FormatElements());
                var commandIdHeader = controllerContext.Request.Headers.GetValues("CommandId").FirstOrDefault();
                if (string.IsNullOrWhiteSpace(commandIdHeader)) return null;
                var commandId = Guid.Parse(commandIdHeader);

                var body = controllerContext.Request.Content.ReadAsStringAsync().Result;
                if (string.IsNullOrWhiteSpace(body))
                    throw new Exception("Command's body is empty!");

                var registrations = _resolver.GetRootLifetimeScope().ComponentRegistry.Registrations.ToArray();
                var registration = registrations.FirstOrDefault(x => x.Activator.LimitType.Name.Equals(commandName, StringComparison.CurrentCultureIgnoreCase));
                if (registration == null)
                    throw new CommandNotImplementedException(commandName, commandId);

                var commandType = registration.Activator.LimitType;
                var command = (ICommand)JsonConvert.DeserializeObject(body, commandType);

                if (command == null)
                    throw new CommandDeserializationException(commandName, commandId);

                command.CommandContext = new CommandContext
                {
                    Id = commandId,
                    UserHostAddress = ClientRequestInfoRetriever.UserHostAddress,
                    UserHostName = ClientRequestInfoRetriever.UserHostName
                };

                return command;
            }
            catch (Exception ex)
            {
                _log.Error(ex);
            }

            return null;
        }

        private HttpResponseMessage ProcessCommnad(HttpActionContext actionContext, ICommand command)
        {
            if (command == null) return actionContext.Request.CreateResponse(HttpStatusCode.BadRequest);

            var handler = _resolver.GetService(typeof(ICommandProcessorGeneric<>).MakeGenericType(command.GetType()));

            var method = handler.GetType().GetMethod("HandleRequest", BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public);


            var result = method.Invoke(handler, new object[] { actionContext, command });

            return (HttpResponseMessage)result;
        }
    }
}

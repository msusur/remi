using Common.Logging;
using Microsoft.AspNet.SignalR.Hubs;

namespace ReMi.Api.Insfrastructure.Notifications
{
    public class ErrorHandlingPipelineModule : HubPipelineModule
    {
        private static readonly ILog Logger = LogManager.GetCurrentClassLogger();

        protected override void OnIncomingError(ExceptionContext exceptionContext, IHubIncomingInvokerContext invokerContext)
        {
            Logger.Error(exceptionContext.Error);

            base.OnIncomingError(exceptionContext, invokerContext);
        }
    }
}

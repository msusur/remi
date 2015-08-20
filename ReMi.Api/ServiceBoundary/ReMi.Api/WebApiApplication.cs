using System;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using Common.Logging;

namespace ReMi.Api
{
    public class WebApiApplication : HttpApplication
    {
        private static readonly ILog Logger = LogManager.GetCurrentClassLogger();

        protected virtual void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);

            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            DependencyResolverConfig.Configure(GlobalConfiguration.Configuration);

            AutoMapperConfiguration.Configure(GlobalConfiguration.Configuration);

            FormattersConfig.Configure(GlobalConfiguration.Configuration);

            GlobalConfiguration.Configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.LocalOnly;

            DatabaseUpdater.InitialiseDatabases(GlobalConfiguration.Configuration);
        }

        protected void Application_End(object sender, EventArgs e)
        {
            // Code that runs on application shutdown. 

            // Logged by PostSharp
        }

        protected void Application_Error()
        {
            var exception = Server.GetLastError();

            Logger.Error(exception);
        }
    }
}

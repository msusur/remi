using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.Cors;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ReMi.Api
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            //TODO: do we need versioning in routes

            //Attribute routing
            config.MapHttpAttributeRoutes();

            SetupFormatters(config);

            config.EnableCors(new EnableCorsAttribute("*", "*", "*"));

            // Uncomment the following line of code to enable query support for actions with an IQueryable or IQueryable<T> return type.
            // To avoid processing unexpected or malicious queries, use the validation settings on QueryableAttribute to validate incoming queries.
            // For more information, visit http://go.microsoft.com/fwlink/?LinkId=279712.
            //config.EnableQuerySupport();

            // To disable tracing in your application, please comment out or remove the following line of code
            // For more information, refer to: http://www.asp.net/web-api
            config.EnableSystemDiagnosticsTracing();
        }

        private static void SetupFormatters(HttpConfiguration config)
        {
            var jsonFormatter = new JsonMediaTypeFormatter();

            JsonSerializerSettings settings = jsonFormatter.CreateDefaultSerializerSettings();
            settings.ContractResolver = new JsonContractResolver(new JsonMediaTypeFormatter());
            settings.Converters.Add(new StringEnumConverter());
            settings.Converters.Add(new IsoDateTimeConverter());
            settings.Converters.Add(new JavaScriptDateTimeConverter());

            jsonFormatter.SerializerSettings = settings;

            config.Formatters.Clear();
            //config.Formatters.Add(new XmlMediaTypeFormatter());
            config.Formatters.Add(jsonFormatter);
            config.Formatters.Add(new FormUrlEncodedMediaTypeFormatter());
        }
    }
}

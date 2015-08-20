using System.Net.Http.Formatting;
using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ReMi.Api
{
    public static class FormattersConfig
    {
        public static void Configure(HttpConfiguration config)
        {
            var jsonFormatter = new JsonMediaTypeFormatter();

            JsonSerializerSettings settings = jsonFormatter.CreateDefaultSerializerSettings();
            settings.ContractResolver = new JsonContractResolver(new JsonMediaTypeFormatter());
            settings.Converters.Add(new StringEnumConverter());
            settings.Converters.Add(new IsoDateTimeConverter());
            settings.Converters.Add(new JavaScriptDateTimeConverter());

            jsonFormatter.SerializerSettings = settings;

            config.Formatters.Clear();
            config.Formatters.Add(jsonFormatter);
            config.Formatters.Add(new XmlMediaTypeFormatter());
            config.Formatters.Add(new FormUrlEncodedMediaTypeFormatter());
        }
    }
}

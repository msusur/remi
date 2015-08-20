using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using RestSharp;
using RestSharp.Deserializers;
using RestSharp.Serializers;

namespace ReMi.Plugin.Common.Serialization
{
    public class JsonSerializationImpl : ISerializer, IDeserializer
    {
        public T Deserialize<T>(IRestResponse response)
        {
            return Deserialize<T>(response.Content);
        }

        public static T Deserialize<T>(string value)
        {
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Formatting = Formatting.Indented,
            };

            settings.Converters.Add(new StringEnumConverter());

            return JsonConvert.DeserializeObject<T>(value, settings);
        }

        public string RootElement { get; set; }
        public string Namespace { get; set; }
        public string DateFormat { get; set; }
        public string ContentType { get; set; }

        public JsonSerializationImpl()
        {
            ContentType = "application/json";
            DateFormat = "json";
        }

        public string Serialize(object obj)
        {
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Formatting = Formatting.Indented,
            };

            settings.Converters.Add(new StringEnumConverter());

            return JsonConvert.SerializeObject(obj, settings);
        }
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using ReMi.Common.Utils;

namespace ReMi.Api.Insfrastructure
{
    public class Serialization : DefaultContractResolver, ISerialization
    {
        private IEnumerable<string> _ignoredProperties;

        public string ToJson(object data, IEnumerable<string> ignoredProperties = null, bool isFormatted = true)
        {
            _ignoredProperties = ignoredProperties;
            var settings = new JsonSerializerSettings
            {
                ContractResolver = this,
                Formatting = isFormatted ? Formatting.Indented : Formatting.None
            };

            settings.Converters.Add(new StringEnumConverter());

            return JsonConvert.SerializeObject(data, settings);
        }

        public T FromJson<T>(string json)
        {
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Formatting = Formatting.Indented,
            };

            settings.Converters.Add(new StringEnumConverter());

            return JsonConvert.DeserializeObject<T>(json, settings);
        }

        public object FromJson(string json, Type type)
        {
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Formatting = Formatting.Indented,
            };

            settings.Converters.Add(new StringEnumConverter());

            return JsonConvert.DeserializeObject(json, type, settings);
        }

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            var properties = base.CreateProperties(type, memberSerialization);
            return _ignoredProperties.IsNullOrEmpty()
                ? properties
                : properties.Where(x => !_ignoredProperties.Any(i => i.Equals(x.PropertyName))).ToList();
        }
    }
}

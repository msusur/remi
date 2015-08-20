using System;
using System.Collections.Generic;

namespace ReMi.Common.Utils
{
    public interface ISerialization
    {
        string ToJson(object data, IEnumerable<string> ignoredProperties = null, bool isFormatted = true);
        T FromJson<T>(string json);
        object FromJson(string json, Type type);
    }
}

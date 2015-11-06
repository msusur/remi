using System;

namespace ReMi.Queries
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class QueryCacheAttribute : Attribute
    {
        public bool IncludeAllProperties { get; set; }
        public string[] PropertiesNames { get; set; }
        public TimeSpan Duration { get; set; }
        public Type CustomCacheKeyType { get; set; }
    }
}

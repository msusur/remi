using System;

namespace ReMi.Contracts.Plugins.Data.CacheService
{
    public class CacheEntry
    {
        public string Group { get; set; }
        public string Key { get; set; }
        public byte[] Value { get; set; }
        public TimeSpan? Duration { get; set; }
    }
}

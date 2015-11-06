using ReMi.Contracts.Plugins.Data.CacheService;

namespace ReMi.Contracts.Plugins.Services.CacheService
{
    public interface  ICacheService : IPluginService
    {
        void SetKey(CacheEntry entry);
        byte[] GetKey(string key);
        void ResetCache();
        void ResetKey(string key);
        void ResetGroup(string group);
    }
}

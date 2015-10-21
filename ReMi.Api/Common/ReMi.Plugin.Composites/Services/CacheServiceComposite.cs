using ReMi.Contracts.Plugins.Data;
using ReMi.Contracts.Plugins.Data.CacheService;
using ReMi.Contracts.Plugins.Services.CacheService;

namespace ReMi.Plugin.Composites.Services
{
    public class CacheServiceComposite : BaseComposit<ICacheService>, ICacheService
    {
        public void SetKey(CacheEntry entry)
        {
            var service = GetPluginService(PluginType.CacheService);
            if (service != null) service.SetKey(entry);
        }

        public byte[] GetKey(string key)
        {
            var service = GetPluginService(PluginType.CacheService);
            return service == null ? null : service.GetKey(key);
        }

        public void ResetCache()
        {
            var service = GetPluginService(PluginType.CacheService);
            if (service != null) service.ResetCache();
        }

        public void ResetKey(string key)
        {
            var service = GetPluginService(PluginType.CacheService);
            if (service != null) service.ResetKey(key);
        }

        public void ResetGroup(string @group)
        {
            var service = GetPluginService(PluginType.CacheService);
            if (service != null) service.ResetGroup(@group);
        }
    }
}

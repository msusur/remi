using System;
using System.Collections.Generic;
using System.Linq;
using ReMi.Contracts.Plugins.Data;
using ReMi.Contracts.Plugins.Data.ReleaseContent;
using ReMi.Contracts.Plugins.Services.ReleaseContent;

namespace ReMi.Plugin.Composites.Services
{
    public class ReleaseContentComposite : BaseComposit<IReleaseContent>, IReleaseContent
    {
        public IEnumerable<ReleaseContentTicket> GetTickets(IEnumerable<Guid> packageIds)
        {
            return GetPackageServicesWithConfiguration(packageIds, PluginType.ReleaseContent)
                .Select(x => x.Service.GetTickets(x.Configurations.Select(c => c.PackageId)))
                .SelectMany(x => x)
                .ToArray();
        }

        public IEnumerable<ReleaseContentTicket> GetDefectTickets(IEnumerable<Guid> packageIds)
        {
            return GetPackageServicesWithConfiguration(packageIds, PluginType.ReleaseContent)
                .Select(x => x.Service.GetDefectTickets(x.Configurations.Select(c => c.PackageId)))
                .SelectMany(x => x)
                .ToArray();
        }

        public void UpdateTicket(IEnumerable<ReleaseContentTicket> tickets, Guid packageId)
        {
            var releaseContentRequest = GetPluginService(packageId, PluginType.ReleaseContent);
            if (releaseContentRequest == null) return;
            releaseContentRequest.UpdateTicket(tickets, packageId);
        }
    }
}

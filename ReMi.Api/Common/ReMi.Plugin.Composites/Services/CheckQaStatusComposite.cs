using System;
using System.Collections.Generic;
using System.Linq;
using ReMi.Contracts.Plugins.Data;
using ReMi.Contracts.Plugins.Data.QaStats;
using ReMi.Contracts.Plugins.Services.QaStats;

namespace ReMi.Plugin.Composites.Services
{
    public class CheckQaStatusComposite : BaseComposit<ICheckQaStatus>, ICheckQaStatus
    {
        public IEnumerable<QaStatusCheckItem> GetQaStatusCheckItems(IEnumerable<Guid> packageIds)
        {
            return GetPackageServicesWithConfiguration(packageIds, PluginType.QaStats)
                .SelectMany(x => x.Service.GetQaStatusCheckItems(x.Configurations.Select(c => c.PackageId)))
                .ToArray();
        }
    }
}

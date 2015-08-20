using ReMi.Contracts.Plugins.Data.QaStats;
using ReMi.Contracts.Plugins.Services.QaStats;
using ReMi.Plugin.Common;
using ReMi.Plugin.QaStats.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using ReMi.Contracts.Plugins.Services;
using System.Linq;
using System.Threading.Tasks;
using ReMi.Common.Utils;

namespace ReMi.Plugin.QaStats
{
    public class QaStatsRequest : RestApiRequest, ICheckQaStatus
    {
        public IPluginConfiguration<PluginConfigurationEntity> Configurations { get; set; }
        public IPluginPackageConfiguration<PluginPackageConfigurationEntity> PackageConfigurations { get; set; }

        public override string BaseUrl
        {
            get { return Configurations.GetPluginConfiguration().QaServiceUrl; }
        }

        public IEnumerable<QaStatusCheckItem> GetQaStatusCheckItems(IEnumerable<Guid> packageIds)
        {
            var result = new ConcurrentDictionary<Guid, IEnumerable<QaStatusCheckItem>>();
            Parallel.ForEach(packageIds, packageId => {
                var qaReleaseCriterion = Get<QaReleaseCriteriaList>(
                    PackageConfigurations.GetPluginPackageConfigurationEntity(packageId).PackagePath);
                if (qaReleaseCriterion != null && qaReleaseCriterion.Data != null)
                {
                    result.TryAdd(packageId, qaReleaseCriterion.Data.Select(x => new QaStatusCheckItem
                    {
                        Area = x.Area,
                        Comments = x.Comments.IsNullOrEmpty() ? string.Empty : string.Join("; ", x.Comments),
                        DetailsEvidence = x.DetailsEvidence,
                        LastStatusUpdate = x.LastStatusUpdate,
                        MetricControl = x.Name,
                        Owner = x.Owner,
                        Status = x.Status
                    }).ToList());
                }
            });
            return result.Values.SelectMany(x => x).ToArray();
        }
    }
}

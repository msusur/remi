using System;
using System.Collections.Generic;
using ReMi.Contracts.Plugins.Data.QaStats;

namespace ReMi.Contracts.Plugins.Services.QaStats
{
    public interface ICheckQaStatus : IPluginService
    {
        IEnumerable<QaStatusCheckItem> GetQaStatusCheckItems(IEnumerable<Guid> packageIds);
    }
}

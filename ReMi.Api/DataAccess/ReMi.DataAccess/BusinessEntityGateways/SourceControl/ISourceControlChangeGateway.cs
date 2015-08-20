using System;
using System.Collections.Generic;
using ReMi.Contracts.Plugins.Data.SourceControl;

namespace ReMi.DataAccess.BusinessEntityGateways.SourceControl
{
    public interface ISourceControlChangeGateway : IDisposable
    {
        IEnumerable<string> FilterExistingChangesByProduct(IEnumerable<string> changeIds, IEnumerable<Guid> productIds);
        IEnumerable<SourceControlChange> GetChanges(Guid releaseWindowId);

        void StoreChanges(IEnumerable<SourceControlChange> changes, Guid windowId);
        void RemoveChangesFromRelease(Guid releaseWindowId);
    }
}

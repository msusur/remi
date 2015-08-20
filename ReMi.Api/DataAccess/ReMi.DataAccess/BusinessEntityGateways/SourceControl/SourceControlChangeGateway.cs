using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Common.Logging;
using ReMi.Common.Utils;
using ReMi.Common.Utils.Repository;
using ReMi.DataAccess.Exceptions;
using ReMi.DataEntities.ReleaseCalendar;
using ReMi.DataEntities.SourceControl;
using DataChanges = ReMi.DataEntities.SourceControl.SourceControlChange;
using SourceControlChange = ReMi.Contracts.Plugins.Data.SourceControl.SourceControlChange;

namespace ReMi.DataAccess.BusinessEntityGateways.SourceControl
{
    public class SourceControlChangeGateway : BaseGateway, ISourceControlChangeGateway
    {
        public IRepository<DataChanges> SourceControlChangeRepository { get; set; }
        public IRepository<ReleaseWindow> ReleaseWindowRepository { get; set; }
        public IRepository<SourceControlChangeToReleaseWindow> SourceControlChangeToReleaseWindowRepository { get; set; }
        public IMappingEngine Mapper { get; set; }
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        public void StoreChanges(IEnumerable<SourceControlChange> changes, Guid releaseWindowId)
        {
            var releaseWindow = ReleaseWindowRepository.GetSatisfiedBy(r => r.ExternalId == releaseWindowId);
            if (releaseWindow == null)
            {
                throw new ReleaseWindowNotFoundException(releaseWindowId);
            }

            var storedChanges = EnsureChangesStored(changes);

            var products = releaseWindow.ReleaseProducts.Select(o => o.Product.ProductId).ToArray();
            var productChanges = SourceControlChangeToReleaseWindowRepository
                .GetAllSatisfiedBy(c => c.ReleaseWindow.ReleaseProducts.Any(o => products.Contains(o.Product.ProductId)))
                .Select(g => g.Change.Identifier);

            var changnesToReleases = storedChanges
                .Where(c => !productChanges.Contains(c.Identifier))
                .Select(c =>
                    new SourceControlChangeToReleaseWindow
                    {
                        ReleaseWindowId = releaseWindow.ReleaseWindowId,
                        SourceControlChangeId = c.SourceControlChangeId
                    })
                .ToList();

            if (changnesToReleases.Any())
                SourceControlChangeToReleaseWindowRepository.Insert(changnesToReleases);
        }

        public void RemoveChangesFromRelease(Guid releaseWindowId)
        {
            var changes = SourceControlChangeToReleaseWindowRepository
                .GetAllSatisfiedBy(x => x.ReleaseWindow.ExternalId == releaseWindowId)
                .ToArray();

            if (changes.IsNullOrEmpty()) return;

            SourceControlChangeToReleaseWindowRepository.Delete(changes);
        }

        public IEnumerable<string> FilterExistingChangesByProduct(IEnumerable<string> changeIds, IEnumerable<Guid> productIds)
        {
            Log.DebugFormat("Starting filter changes");

            var changes = SourceControlChangeToReleaseWindowRepository.Entities
                .Where(c => changeIds.Any(h => h == c.Change.Identifier))
                .ToArray();

            Log.DebugFormat("Got existing chagnes, count: {0}", changes.Length);
            var result = changes
                .Where(x => x.ReleaseWindow.ReleaseProducts.Any(p => productIds.Contains(p.Product.ExternalId)))
                .Select(c => c.Change.Identifier)
                .ToArray();
            Log.DebugFormat("Finish filtering changes");

            return result;
        }

        public IEnumerable<SourceControlChange> GetChanges(Guid releaseWindowId)
        {
            var changes =
                SourceControlChangeToReleaseWindowRepository.GetAllSatisfiedBy(w => w.ReleaseWindow.ExternalId == releaseWindowId)
                    .Select(w => w.Change)
                    .ToArray();

            return Mapper.Map<IEnumerable<DataChanges>, IEnumerable<SourceControlChange>>(changes);
        }

        private IEnumerable<DataChanges> EnsureChangesStored(IEnumerable<SourceControlChange> changes)
        {
            var inputChanges = changes as IList<SourceControlChange> ?? changes.ToList();

            var ids = inputChanges.Select(c => c.Identifier).ToArray();

            var existingChanges = SourceControlChangeRepository
                .GetAllSatisfiedBy(c => ids.Contains(c.Identifier))
                .ToList();

            var nonExistingChanges = inputChanges.Where(c => existingChanges.All(x => x.Identifier != c.Identifier)).ToList();
            if (nonExistingChanges.Any())
                SourceControlChangeRepository.Insert(
                    Mapper.Map<IEnumerable<SourceControlChange>, IEnumerable<DataChanges>>(nonExistingChanges));

            var nonExistingChangeIds = nonExistingChanges.Select(x => x.Identifier).ToArray();

            return SourceControlChangeRepository
                .GetAllSatisfiedBy(c => nonExistingChangeIds.Contains(c.Identifier))
                .Concat(existingChanges);
        }

        public override void OnDisposing()
        {
            ReleaseWindowRepository.Dispose();
            SourceControlChangeRepository.Dispose();
            SourceControlChangeToReleaseWindowRepository.Dispose();

            base.OnDisposing();
        }
    }
}

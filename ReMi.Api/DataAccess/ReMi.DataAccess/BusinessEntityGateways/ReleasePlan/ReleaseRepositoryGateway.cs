using AutoMapper;
using ReMi.Common.Utils.Repository;
using ReMi.DataAccess.Exceptions;
using ReMi.DataEntities.ReleaseCalendar;
using System;
using System.Collections.Generic;
using System.Linq;
using ReMi.DataEntities.SourceControl;
using DataReleaseRepository = ReMi.DataEntities.ReleasePlan.ReleaseRepository;
using ReleaseRepository = ReMi.Contracts.Plugins.Data.SourceControl.ReleaseRepository;

namespace ReMi.DataAccess.BusinessEntityGateways.ReleasePlan
{
    public class ReleaseRepositoryGateway : BaseGateway, IReleaseRepositoryGateway
    {
        public IRepository<ReleaseWindow> ReleaseWindowRepository { get; set; }
        public IRepository<DataReleaseRepository> ReleaseRepository { get; set; }
        public IRepository<SourceControlChange> SourceControlChangeRepository { get; set; }
        public IMappingEngine MappingEngine { get; set; }

        public ReleaseRepository GetReleaseRepository(Guid releaseWindowId, Guid repositoryId)
        {
            var releaseWindow = GetReleaseWindow(releaseWindowId);

            var releaseJob = ReleaseRepository
                .GetSatisfiedBy(o => o.ReleaseWindowId == releaseWindow.ReleaseWindowId && o.RepositoryId == repositoryId);

            return releaseJob == null ? null : MappingEngine.Map<DataReleaseRepository, ReleaseRepository>(releaseJob);
        }

        private ReleaseWindow GetReleaseWindow(Guid releaseWindowId)
        {
            var releaseWindow = ReleaseWindowRepository.GetSatisfiedBy(o => o.ExternalId == releaseWindowId);
            if (releaseWindow == null)
                throw new ReleaseWindowNotFoundException(releaseWindowId);
            return releaseWindow;
        }

        public IEnumerable<ReleaseRepository> GetReleaseRepositories(Guid releaseWindowId)
        {
            var releaseWindow = GetReleaseWindow(releaseWindowId);

            var items = ReleaseRepository
                .GetAllSatisfiedBy(o => o.ReleaseWindowId == releaseWindow.ReleaseWindowId)
                .ToArray();

            var result = MappingEngine.Map<IEnumerable<DataReleaseRepository>, IEnumerable<ReleaseRepository>>(items).ToArray();
            result.Where(x => x.LatestChange)
                .Each(x =>
                {
                    var latestCommit = SourceControlChangeRepository.Entities
                        .Where(c => c.Date.HasValue && c.Repository == x.Repository)
                        .OrderByDescending(c => c.Date.Value)
                        .FirstOrDefault();
                    x.ChangesFrom = latestCommit == null ? "" : latestCommit.Identifier;
                });

            return result;
        }

        public void AddRepositoryToRelease(ReleaseRepository repository, Guid releaseWindowId)
        {
            var releaseWindow = GetReleaseWindow(releaseWindowId);

            var dbReleaseRepository = ReleaseRepository.GetSatisfiedBy(
                    o => o.RepositoryId == repository.ExternalId && o.ReleaseWindowId == releaseWindow.ReleaseWindowId);
            if (dbReleaseRepository != null)
                throw new EntityAlreadyExistsException(typeof(DataReleaseRepository),
                    string.Format("ReleaseWindowId: {0}, RepositoryId: {1}", releaseWindowId, dbReleaseRepository.RepositoryId));

            var newReleaseRepository = MappingEngine.Map<ReleaseRepository, DataReleaseRepository>(repository);
            newReleaseRepository.ReleaseWindowId = releaseWindow.ReleaseWindowId;

            ReleaseRepository.Insert(newReleaseRepository);
        }

        public void UpdateRepository(ReleaseRepository repository, Guid releaseWindowId)
        {
            var releaseWindow = GetReleaseWindow(releaseWindowId);

            var dbReleaseRepository = ReleaseRepository.GetSatisfiedBy(
                    o => o.RepositoryId == repository.ExternalId && o.ReleaseWindowId == releaseWindow.ReleaseWindowId);
            if (dbReleaseRepository == null)
                throw new EntityNotFoundException(typeof(DataReleaseRepository),
                    string.Format("ReleaseWindowId: {0}, RepositoryId: {1}", releaseWindowId, repository.ExternalId));
            if (!string.IsNullOrEmpty(dbReleaseRepository.ChangesFrom) &&
                string.IsNullOrEmpty(repository.ChangesFrom))
            {
                repository.ChangesFrom = dbReleaseRepository.ChangesFrom;
            }
            MappingEngine.Map(repository, dbReleaseRepository);

            ReleaseRepository.Update(dbReleaseRepository);
        }

        public void RemoveRepositoriesFromRelease(Guid releaseWindowId)
        {
            var releaseWindow = GetReleaseWindow(releaseWindowId);

            var toRemove = ReleaseRepository.GetAllSatisfiedBy(x => x.ReleaseWindowId == releaseWindow.ReleaseWindowId);

            ReleaseRepository.Delete(toRemove);
        }

        public override void OnDisposing()
        {
            ReleaseRepository.Dispose();
            ReleaseWindowRepository.Dispose();
            SourceControlChangeRepository.Dispose();

            base.OnDisposing();
        }
    }
}

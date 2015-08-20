using AutoMapper;
using ReMi.BusinessEntities.Plugins;
using ReMi.Common.Constants.ReleaseCalendar;
using ReMi.Common.Constants.ReleaseExecution;
using ReMi.Common.Utils;
using ReMi.Common.Utils.Enums;
using ReMi.Common.Utils.Repository;
using ReMi.DataAccess.Exceptions;
using ReMi.DataAccess.Exceptions.Configuration;
using ReMi.DataEntities.Metrics;
using ReMi.DataEntities.Products;
using ReMi.DataEntities.ReleaseCalendar;
using System;
using System.Collections.Generic;
using System.Linq;
using ReleaseTypeDescription = ReMi.BusinessEntities.ReleaseCalendar.ReleaseTypeDescription;

namespace ReMi.DataAccess.BusinessEntityGateways.ReleaseCalendar
{
    public class ReleaseWindowGateway : BaseGateway, IReleaseWindowGateway
    {
        public IRepository<ReleaseWindow> ReleaseWindowRepository { get; set; }
        public IRepository<Product> ProductRepository { get; set; }
        public IRepository<Metric> MetricRepository { get; set; }
        public IRepository<DataEntities.Auth.Account> AccountRepository { get; set; }
        public IMappingEngine MappingEngine { get; set; }
        public Func<IReleaseProductGateway> ReleaseProductGatewayFactory { get; set; }

        public IEnumerable<BusinessEntities.ReleaseCalendar.ReleaseWindowView> GetAllStartingInTimeRange(DateTime startTime, DateTime endTime)
        {
            if (startTime > endTime)
            {
                throw new ArgumentOutOfRangeException(string.Format("startTime ({0}) must be prior to endTime ({1})", startTime, endTime));
            }

            var dataResults = ReleaseWindowRepository
                .GetAllSortedSatisfiedBy(
                    r => !r.Metrics.Any() ||
                        r.Metrics.Any(m => m.MetricType == MetricType.StartTime && m.ExecutedOn.HasValue && m.ExecutedOn.Value >= startTime && m.ExecutedOn.Value <= endTime),
                    q => q.OrderBy(r => r.Metrics.FirstOrDefault(m => m.MetricType == MetricType.StartTime && m.ExecutedOn.HasValue).ExecutedOn.Value))
                .ToList();

            return MappingEngine.Map<IEnumerable<ReleaseWindow>, IEnumerable<BusinessEntities.ReleaseCalendar.ReleaseWindowView>>(dataResults);
        }

        public IEnumerable<BusinessEntities.ReleaseCalendar.ReleaseWindow> GetAllByProduct(string product)
        {
            return MappingEngine
                .Map<IEnumerable<ReleaseWindow>, IEnumerable<BusinessEntities.ReleaseCalendar.ReleaseWindow>>(
                    ReleaseWindowRepository.GetAllSatisfiedBy(o => o.ReleaseProducts.Any(x => x.Product.Description == product)));
        }

        public BusinessEntities.ReleaseCalendar.ReleaseWindow GetUpcomingRelease(string product)
        {
            var result = ReleaseWindowRepository
                .GetAllSortedSatisfiedBy(
                   x => x.ReleaseProducts.Any(p => p.Product.Description == product)
                       && x.Metrics.Any(m => m.MetricType == MetricType.Close && !m.ExecutedOn.HasValue),
                   q => q.OrderBy(r => r.StartTime))
                .FirstOrDefault();

            return MappingEngine.Map<ReleaseWindow, BusinessEntities.ReleaseCalendar.ReleaseWindow>(result);
        }

        public BusinessEntities.ReleaseCalendar.ReleaseWindow GetCurrentRelease(string product)
        {
            var fromDate = SystemTime.Now.AddMinutes(15);
            var current = SystemTime.Now;
            var result = ReleaseWindowRepository.Entities
                .Where(x => x.ReleaseProducts.Any(p => p.Product.Description == product))
                .Where(x => x.Metrics.Any(m => m.MetricType == MetricType.StartTime && m.ExecutedOn.HasValue && m.ExecutedOn.Value <= fromDate))
                .FirstOrDefault(x => x.Metrics.Any(m => m.MetricType == MetricType.EndTime && m.ExecutedOn.HasValue && m.ExecutedOn.Value >= current));

            return MappingEngine.Map<ReleaseWindow, BusinessEntities.ReleaseCalendar.ReleaseWindow>(result);
        }

        public IEnumerable<BusinessEntities.ReleaseCalendar.ReleaseWindow> GetNearReleases(string product)
        {
            var productEntity = ProductRepository.GetSatisfiedBy(x => string.Equals(x.Description, product));
            if (productEntity == null)
                throw new ProductNotFoundException(product);

            const int numberClosed = 2;
            const int numberNonClosed = 3;

            var releases = ReleaseWindowRepository
                .GetAllSortedSatisfiedBy(x =>
                    x.ReleaseProducts.Any(o => o.Product.Description == product)
                    && x.Metrics.Any(m => m.MetricType == MetricType.Close && m.ExecutedOn.HasValue),
                   q => q.OrderByDescending(r => r.StartTime))
                .Take(numberClosed)
                .ToList();

            releases.AddRange(
                ReleaseWindowRepository
                    .GetAllSortedSatisfiedBy(x =>
                        x.ReleaseProducts.Any(o => o.Product.Description == product)
                        && (!x.Metrics.Any()
                            || !x.Metrics.Any(m => m.MetricType == MetricType.Close && m.ExecutedOn.HasValue)),
                       q => q.OrderBy(r => r.StartTime))
                    .Take(numberNonClosed)
                    .ToList()
                );

            return MappingEngine.Map<IEnumerable<ReleaseWindow>, IEnumerable<BusinessEntities.ReleaseCalendar.ReleaseWindow>>(
                releases.OrderBy(o => o.StartTime)
            );
        }

        public IEnumerable<BusinessEntities.ReleaseCalendar.ReleaseWindow> GetExpiredReleases()
        {
            var endPeriod = SystemTime.Now;

            var releases = ReleaseWindowRepository
                .GetAllSatisfiedBy(x =>
                    x.Metrics.Any(m => m.MetricType == MetricType.EndTime && m.ExecutedOn.HasValue && m.ExecutedOn.Value < endPeriod)
                    && x.Metrics.Any(m => m.MetricType == MetricType.Close && !m.ExecutedOn.HasValue));

            return MappingEngine.Map<IEnumerable<ReleaseWindow>, IEnumerable<BusinessEntities.ReleaseCalendar.ReleaseWindow>>(releases);
        }

        public BusinessEntities.ReleaseCalendar.ReleaseWindow FindFirstOverlappedRelease(string product,
            DateTime periodStart, DateTime periodEnd)
        {
            return FindFirstOverlappedRelease(product, periodStart, periodEnd, Guid.Empty);
        }

        public BusinessEntities.ReleaseCalendar.ReleaseWindow FindFirstOverlappedRelease(string product,
            DateTime periodStart, DateTime periodEnd, Guid currentExternalId)
        {
            var maintenanceTypes = EnumDescriptionHelper.GetEnumDescriptions<ReleaseType, ReleaseTypeDescription>()
                .Where(x => x.IsMaintenance)
                .Select(x => x.Id)
                .ToArray();

            var foundReleases = ReleaseWindowRepository
                .GetAllSatisfiedBy(
                    r => (
                        (
                            !r.Metrics.Any()
                            || r.Metrics.Any(m => m.MetricType == MetricType.StartTime && m.ExecutedOn.HasValue && periodStart <= m.ExecutedOn.Value && periodEnd > m.ExecutedOn.Value)
                            || r.Metrics.Any(m => m.MetricType == MetricType.EndTime && m.ExecutedOn.HasValue && periodStart < m.ExecutedOn.Value && periodEnd >= m.ExecutedOn.Value)
                            || (
                                r.Metrics.Any(m => m.MetricType == MetricType.StartTime && m.ExecutedOn.HasValue && periodStart <= m.ExecutedOn.Value)
                                && r.Metrics.Any(m => m.MetricType == MetricType.EndTime && m.ExecutedOn.HasValue && periodEnd >= m.ExecutedOn.Value)
                            )
                        )
                        && !r.Metrics.Any(m => m.MetricType == MetricType.Close && m.ExecutedOn.HasValue)
                        && r.ReleaseProducts.Any(x => x.Product.Description == product && x.Product.ReleaseTrack != ReleaseTrack.Automated)
                        && (currentExternalId.Equals(Guid.Empty) || !r.ExternalId.Equals(currentExternalId))
                    ))
                .ToList();

            var dataRelease = foundReleases.FirstOrDefault(r => !maintenanceTypes.Contains((int)r.ReleaseType));

            return MappingEngine.Map<ReleaseWindow, BusinessEntities.ReleaseCalendar.ReleaseWindow>(dataRelease);
        }

        public BusinessEntities.ReleaseCalendar.ReleaseWindow GetByExternalId(Guid externalId, bool forceCheck = false, bool getReleaseNote = false)
        {
            var result = ReleaseWindowRepository.GetSatisfiedBy(r => r.ExternalId == externalId);

            if (result == null && forceCheck)
                throw new ReleaseWindowNotFoundException(externalId);
            if (result == null) return null;

            var retval = MappingEngine.Map<ReleaseWindow, BusinessEntities.ReleaseCalendar.ReleaseWindow>(result);


            if (getReleaseNote && result.ReleaseNotes != null)
            {
                retval.ReleaseNotes = result.ReleaseNotes.ReleaseNotes;
                retval.Issues = result.ReleaseNotes.Issues;
            }
             
            retval.Plugins = result.ReleaseProducts
                .SelectMany(x => x.Product.PluginPackageConfiguration)
                .Where(x => x.PluginId.HasValue)
                .Select(x => x.Plugin)
                .Distinct()
                .Select(x => new PluginView
                {
                    PluginType = x.PluginType,
                    PluginKey = x.Key,
                    PluginId = x.ExternalId
                })
                .ToArray();

            return retval;
        }

        public void Create(BusinessEntities.ReleaseCalendar.ReleaseWindow releaseWindow, Guid creatorId)
        {
            var account = AccountRepository.GetSatisfiedBy(x => x.ExternalId == creatorId);
            if (account == null) throw new AccountNotFoundException(creatorId);

            var targetProducts = releaseWindow.Products.ToList();

            var products = ProductRepository.GetAllSatisfiedBy(p => targetProducts.Contains(p.Description)).ToList();
            if (products.IsNullOrEmpty())
            {
                throw new ProductNotFoundException(releaseWindow.Products);
            }
            if (products.Count != targetProducts.Count)
            {
                throw new ProductNotFoundException(targetProducts.Where(x => products.Any(p => p.Description == x)).ToArray());
            }

            Logger.DebugFormat("Found products: {0}", products.FormatElements());

            var dataReleaseWindow = MappingEngine.Map<BusinessEntities.ReleaseCalendar.ReleaseWindow, ReleaseWindow>(releaseWindow);

            dataReleaseWindow.ReleaseDecision = ReleaseDecision.NoGo;
            dataReleaseWindow.CreatedOn = SystemTime.Now;
            dataReleaseWindow.CreatedById = account.AccountId;

            ReleaseWindowRepository.Insert(dataReleaseWindow);

            using (var gateway = ReleaseProductGatewayFactory())
            {
                gateway.AssignProductsToRelease(releaseWindow.ExternalId, targetProducts);
            }
        }

        public void Cancel(BusinessEntities.ReleaseCalendar.ReleaseWindow releaseWindow)
        {
            var entity = ReleaseWindowRepository.GetSatisfiedBy(row => row.ExternalId == releaseWindow.ExternalId);

            ReleaseWindowRepository.Delete(entity);
        }

        public void Update(BusinessEntities.ReleaseCalendar.ReleaseWindow releaseWindow, bool updateOnlyDescription = false)
        {
            var targetProducts = releaseWindow.Products.ToList();

            var products = ProductRepository.GetAllSatisfiedBy(p => targetProducts.Contains(p.Description)).ToList();
            if (products == null)
            {
                throw new ProductNotFoundException(releaseWindow.Products);
            }
            if (products.Count != targetProducts.Count)
            {
                throw new ProductNotFoundException(targetProducts.Where(x => products.Any(p => p.Description == x)).ToArray());
            }

            var entity = ReleaseWindowRepository.GetSatisfiedBy(e => e.ExternalId == releaseWindow.ExternalId);

            entity.Description = releaseWindow.Description;

            if (!updateOnlyDescription)
            {
                entity.StartTime = releaseWindow.StartTime.ToUniversalTime();
                entity.ReleaseType = releaseWindow.ReleaseType;
                entity.RequiresDowntime = releaseWindow.RequiresDowntime;
                entity.Sprint = releaseWindow.Sprint;
            }

            ReleaseWindowRepository.Update(entity);

            using (var gateway = ReleaseProductGatewayFactory())
            {
                gateway.AssignProductsToRelease(releaseWindow.ExternalId, targetProducts);
            }

        }

        public void CloseRelease(string releaseNotes, Guid releaseWindowId)
        {
            var release = ReleaseWindowRepository.GetSatisfiedBy(w => w.ExternalId == releaseWindowId);

            if (release == null)
            {
                throw new ReleaseWindowNotFoundException(releaseWindowId);
            }

            if (release.ReleaseNotes == null)
            {
                release.ReleaseNotes = new ReleaseNote
                {
                    ReleaseNotes = releaseNotes
                };
            }
            else
            {
                release.ReleaseNotes.ReleaseNotes = releaseNotes;
            }

            if (!release.Metrics.Any() || release.Metrics.All(x => x.MetricType != MetricType.Close))
            {
                MetricRepository.Insert(new Metric
                {
                    ExecutedOn = SystemTime.Now,
                    ExternalId = Guid.NewGuid(),
                    MetricType = MetricType.Close,
                    ReleaseWindowId = release.ReleaseWindowId
                });
            }
            else
            {
                var metric =
                    MetricRepository.GetSatisfiedBy(x => x.ReleaseWindowId == release.ReleaseWindowId && x.MetricType == MetricType.Close);
                metric.ExecutedOn = SystemTime.Now;
                MetricRepository.Update(metric);
            }
            ReleaseWindowRepository.Update(release);
        }

        public void SaveIssues(BusinessEntities.ReleaseCalendar.ReleaseWindow window)
        {
            var release = ReleaseWindowRepository.GetSatisfiedBy(w => w.ExternalId == window.ExternalId);

            if (release == null)
            {
                throw new ReleaseWindowNotFoundException(window.ExternalId);
            }

            if (release.ReleaseNotes == null)
            {
                release.ReleaseNotes = new ReleaseNote
                {
                    Issues = window.Issues
                };
            }
            else
            {
                release.ReleaseNotes.Issues = window.Issues;
            }

            ReleaseWindowRepository.Update(release);
        }

        public void CloseFailedRelease(Guid releaseWindowId, string issues)
        {
            var releaseWindow = ReleaseWindowRepository.GetSatisfiedBy(w => w.ExternalId == releaseWindowId);
            if (releaseWindow == null)
                throw new ReleaseWindowNotFoundException(releaseWindowId);

            if (releaseWindow.ReleaseNotes == null)
                releaseWindow.ReleaseNotes = new ReleaseNote
                {
                    Issues = issues
                };
            else
            {
                if (string.IsNullOrWhiteSpace(releaseWindow.ReleaseNotes.Issues))
                    releaseWindow.ReleaseNotes.Issues = issues;
                else
                    releaseWindow.ReleaseNotes.Issues =
                        string.Format("{0}{1}{2}",
                        releaseWindow.ReleaseNotes.Issues.Trim().TrimEnd(Environment.NewLine.ToCharArray()),
                        Environment.NewLine + Environment.NewLine,
                        issues);
            }

            releaseWindow.IsFailed = true;

            ReleaseWindowRepository.Update(releaseWindow);


            var metrics = MetricRepository.GetAllSatisfiedBy(x => x.ReleaseWindow.ExternalId == releaseWindowId && !x.ExecutedOn.HasValue);
            foreach (var metric in metrics)
            {
                metric.ExecutedOn = SystemTime.Now;
                MetricRepository.Update(metric);

                Logger.InfoFormat("Set metric as completed for failed release. ReleaseWindowId={0}, MetricId={1}, MetricType={2}",
                    releaseWindow.ExternalId, metric.MetricId, metric.MetricType);
            }
        }

        public bool IsClosed(Guid releaseWindowId)
        {
            var release = ReleaseWindowRepository.GetSatisfiedBy(w => w.ExternalId == releaseWindowId);

            if (release == null)
            {
                throw new ReleaseWindowNotFoundException(releaseWindowId);
            }

            return release.Metrics.Any() && release.Metrics.Any(x => x.MetricType == MetricType.Close && x.ExecutedOn.HasValue);
        }

        public void UpdateReleaseDecision(Guid releaseWindowId, ReleaseDecision releaseDecision)
        {
            var releaseWindow = ReleaseWindowRepository.GetSatisfiedBy(w => w.ExternalId == releaseWindowId);

            if (releaseWindow == null)
            {
                throw new ReleaseWindowNotFoundException(releaseWindowId);
            }

            releaseWindow.ReleaseDecision = releaseDecision;
            ReleaseWindowRepository.Update(releaseWindow);
        }

        public void ApproveRelease(Guid releaseWindowId)
        {
            var release = ReleaseWindowRepository.GetSatisfiedBy(w => w.ExternalId == releaseWindowId);

            if (release == null)
            {
                throw new ReleaseWindowNotFoundException(releaseWindowId);
            }

            if (!release.Metrics.Any() || release.Metrics.All(x => x.MetricType != MetricType.Approve))
            {
                MetricRepository.Insert(new Metric
                {
                    ExecutedOn = SystemTime.Now,
                    ExternalId = Guid.NewGuid(),
                    MetricType = MetricType.Approve,
                    ReleaseWindowId = release.ReleaseWindowId
                });
            }
            else
            {
                var metric =
                    MetricRepository.GetSatisfiedBy(x => x.ReleaseWindowId == release.ReleaseWindowId && x.MetricType == MetricType.Approve);
                metric.ExecutedOn = SystemTime.Now;
                MetricRepository.Update(metric);
            }
        }

        public override void OnDisposing()
        {
            ReleaseWindowRepository.Dispose();
            ProductRepository.Dispose();
            MetricRepository.Dispose();
            MappingEngine.Dispose();

            base.OnDisposing();
        }
    }
}

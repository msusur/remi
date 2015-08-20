using ReMi.BusinessEntities.ReleaseCalendar;
using System;
using System.Collections.Generic;
using ReMi.Common.Constants.ReleaseCalendar;

namespace ReMi.DataAccess.BusinessEntityGateways.ReleaseCalendar
{
	public interface IReleaseWindowGateway : IDisposable
	{
        IEnumerable<ReleaseWindowView> GetAllStartingInTimeRange(DateTime startTime, DateTime endTime);
        IEnumerable<ReleaseWindow> GetAllByProduct(string product);
        IEnumerable<ReleaseWindow> GetNearReleases(string product);
        IEnumerable<ReleaseWindow> GetExpiredReleases();

        ReleaseWindow GetUpcomingRelease(string product);
        ReleaseWindow GetCurrentRelease(string product);

        ReleaseWindow GetByExternalId(Guid externalId, bool forceCheck = false, bool getReleaseNote = false);

        ReleaseWindow FindFirstOverlappedRelease(string product, DateTime startTime, DateTime endTime);
        ReleaseWindow FindFirstOverlappedRelease(string product, DateTime startTime, DateTime endTime, Guid currentExternalId);

		void Create(ReleaseWindow releaseWindow, Guid creatorId);
        void Cancel(ReleaseWindow releaseWindow);
        void Update(ReleaseWindow releaseWindow, bool updateOnlyDescription);

        void ApproveRelease(Guid releaseWindowId);

        bool IsClosed(Guid releaseWindowId);
        void CloseRelease(string releaseNotes, Guid releaseWindowId);
        void CloseFailedRelease(Guid releaseWindowId, string issues);
        
        void SaveIssues(ReleaseWindow window);

	    void UpdateReleaseDecision(Guid releaseWindowId, ReleaseDecision releaseDecision);
	}
}

using System;

namespace ReMi.Common.WebApi.Tracking
{
    public interface ITracker<T>
	{
        void CreateTracker(Guid externalId, string description);
        void Started(Guid externalId);
        void Finished(Guid externalId, string error = null);
	}
}

using System;
using System.Collections.Generic;
using ReMi.BusinessEntities.Auth;

namespace ReMi.BusinessLogic.ReleasePlan
{
    public interface IReleaseWindowStateUpdater
    {
        void CloseRelease(Guid releaseWindowId, string releaseNotes, IEnumerable<Account> recipients, Guid userId);
    }
}

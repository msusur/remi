using System;
using System.Collections.Generic;
using ReMi.BusinessEntities.Auth;
using ReMi.BusinessEntities.ReleaseCalendar;
using ReMi.BusinessEntities.ReleasePlan;

namespace ReMi.DataAccess.BusinessEntityGateways.ReleasePlan
{
    public interface IReleaseParticipantGateway : IDisposable
    {
        void AddReleaseParticipants(List<ReleaseParticipant> releaseParticipant, Guid authorId);
        void AddReleaseParticipants(List<ReleaseParticipant> releaseParticipant);
        void RemoveReleaseParticipant(ReleaseParticipant releaseParticipant);
        IEnumerable<ReleaseParticipant> GetReleaseParticipants(Guid releaseWindowId);
        List<Account> GetReleaseMembers(Guid releaseWindowId);
        ReleaseWindow GetReleaseWindow(Guid releaseParticipantId);
        void ApproveReleaseParticipation(Guid releaseParticipantId);
        void ClearParticipationApprovements(Guid releaseWindowId, Guid authorId);
    }
}

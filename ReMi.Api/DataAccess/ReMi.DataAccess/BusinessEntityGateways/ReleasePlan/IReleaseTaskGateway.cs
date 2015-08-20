using ReMi.BusinessEntities;
using ReMi.BusinessEntities.ReleasePlan;
using System;
using System.Collections.Generic;

namespace ReMi.DataAccess.BusinessEntityGateways.ReleasePlan
{
    public interface IReleaseTaskGateway : IDisposable
    {
        IEnumerable<EnumEntry> GetReleaseTaskTypes();
        IEnumerable<EnumEntry> GetReleaseTaskRisks();
        IEnumerable<EnumEntry> GetReleaseTaskEnvironments();
        
        ReleaseTaskView GetReleaseTaskView(Guid releaseTaskId);
        ReleaseTask GetReleaseTask(Guid releaseTaskId);

        IEnumerable<ReleaseTaskView> GetReleaseTaskViews(Guid releaseWindowId);
        IEnumerable<ReleaseTask> GetReleaseTasks(Guid releaseWindowId);

        ReleaseTaskAttachment GetReleaseTaskAttachment(Guid releaseTaskAttachmentId);
        string GetHelpDeskTicketReference(Guid releaseTaskId);

        void CreateReleaseTask(ReleaseTask releaseTask);
        bool UpdateReleaseTask(ReleaseTask releaseTask);
        void DeleteReleaseTask(Guid releaseTaskId);
        void AssignHelpDeskTicket(ReleaseTask releaseTask, string ticketId, string ticketUrl);

        void AddReleaseTaskAttachment(ReleaseTaskAttachment attachment);
        void DeleteReleaseTaskAttachment(Guid releaseTaskAttachmentId);
        void ConfirmReleaseTaskReceipt(Guid releaseTaskId);
        void ClearReleaseTaskReceipt(Guid releaseTaskId);
        void CompleteTask(Guid releaseTaskId, Guid completedById);
        void UpdateReleaseTasksOrder(IDictionary<Guid, short> tasksOrder);
    }
}

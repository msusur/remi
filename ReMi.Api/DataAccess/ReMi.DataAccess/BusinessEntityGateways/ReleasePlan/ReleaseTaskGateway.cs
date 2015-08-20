using System.Linq;
using AutoMapper;
using ReMi.BusinessEntities;
using ReMi.BusinessEntities.ReleasePlan;
using ReMi.Common.Utils;
using ReMi.DataAccess.Exceptions;
using ReMi.DataEntities.Auth;
using ReMi.DataEntities.ReleaseCalendar;
using System;
using System.Collections.Generic;
using ReMi.Common.Utils.Repository;
using ReMi.DataEntities.ReleasePlan;
using ReleaseTask = ReMi.BusinessEntities.ReleasePlan.ReleaseTask;
using ReleaseTaskAttachment = ReMi.BusinessEntities.ReleasePlan.ReleaseTaskAttachment;
using ReleaseTaskAttachmentDTO = ReMi.DataEntities.ReleasePlan.ReleaseTaskAttachment;
using ReleaseTaskDTO = ReMi.DataEntities.ReleasePlan.ReleaseTask;
using ReleaseTaskTypeDescription = ReMi.DataEntities.ReleasePlan.ReleaseTaskTypeDescription;

namespace ReMi.DataAccess.BusinessEntityGateways.ReleasePlan
{
    public class ReleaseTaskGateway : BaseGateway, IReleaseTaskGateway
    {
        public IRepository<Account> AccountRepository { get; set; }
        public IRepository<ReleaseTaskDTO> ReleaseTaskRepository { get; set; }
        public IRepository<ReleaseTaskAttachmentDTO> ReleaseTaskAttachmentRepository { get; set; }
        public IRepository<ReleaseWindow> ReleaseWindowRepository { get; set; }
        public IRepository<ReleaseTaskTypeDescription> ReleaseTaskTypeRepository { get; set; }
        public IRepository<ReleaseTaskRiskDescription> ReleaseTaskRiskRepository { get; set; }
        public IRepository<ReleaseTaskEnvironmentDescription> ReleaseTaskEnvironmentRepository { get; set; }
        public IMappingEngine Mapper { get; set; }

        public IEnumerable<EnumEntry> GetReleaseTaskTypes()
        {
            return ReleaseTaskTypeRepository.Entities.Select(x => new EnumEntry
                {
                    Value = x.Name,
                    Text = x.Description
                }).ToList();
        }

        public IEnumerable<EnumEntry> GetReleaseTaskRisks()
        {
            return ReleaseTaskRiskRepository.Entities.Select(x => new EnumEntry
            {
                Value = x.Name,
                Text = x.Description
            }).ToList();
        }

        public IEnumerable<EnumEntry> GetReleaseTaskEnvironments()
        {
            return ReleaseTaskEnvironmentRepository.Entities.Select(x => new EnumEntry
            {
                Value = x.Name,
                Text = x.Description
            }).ToList();
        }

        public IEnumerable<ReleaseTaskView> GetReleaseTaskViews(Guid releaseWindowId)
        {
            return LoadTasks<ReleaseTaskView>(releaseWindowId);
        }

        public IEnumerable<ReleaseTask> GetReleaseTasks(Guid releaseWindowId)
        {
            return LoadTasks<ReleaseTask>(releaseWindowId);
        }

        public string GetHelpDeskTicketReference(Guid releaseTaskId)
        {
            var releaseTask = ReleaseTaskRepository
                .GetSatisfiedBy(x => x.ExternalId == releaseTaskId);

            if (releaseTask == null)
                throw new ReleaseTaskNotFoundException(releaseTaskId);

            return releaseTask.HelpDeskReference;
        }

        public ReleaseTaskView GetReleaseTaskView(Guid releaseTaskId)
        {
            return LoadTask<ReleaseTaskView>(releaseTaskId);
        }

        public ReleaseTask GetReleaseTask(Guid releaseTaskId)
        {
            var result = LoadTask<ReleaseTask>(releaseTaskId);
            if (result == null)
                throw new ReleaseTaskNotFoundException(releaseTaskId);

            return result;
        }

        public ReleaseTaskAttachment GetReleaseTaskAttachment(Guid releaseTaskAttachmentId)
        {
            var attachment = ReleaseTaskAttachmentRepository
                .GetSatisfiedBy(x => x.ExternalId == releaseTaskAttachmentId);

            if (attachment == null)
                throw new ReleaseTaskAttachmentNotFoundException(releaseTaskAttachmentId);

            return Mapper.Map<ReleaseTaskAttachmentDTO, ReleaseTaskAttachment>(attachment);
        }

        public bool UpdateReleaseTask(ReleaseTask releaseTask)
        {
            var dataReleaseTask = Mapper.Map<ReleaseTask, ReleaseTaskDTO>(releaseTask);

            var changeList = ReleaseTaskRepository.Update(
                entity => entity.ExternalId == releaseTask.ExternalId,
                entity =>
                {
                    if (entity.CompletedOn != null)
                    {
                        throw new ReleaseTaskAlreadyCompletedException(entity.ExternalId);
                    }

                    entity.Type = dataReleaseTask.Type;
                    entity.Description = dataReleaseTask.Description;
                    entity.AssigneeAccountId = GetAccountId(releaseTask.AssigneeExternalId);

                    entity.Risk = dataReleaseTask.Risk;
                    entity.WhereTested = dataReleaseTask.WhereTested;
                    entity.RequireSiteDown = dataReleaseTask.RequireSiteDown;
                    entity.LengthOfRun = dataReleaseTask.LengthOfRun;
                });

            return changeList.Any();
        }

        public void UpdateReleaseTasksOrder(IDictionary<Guid, short> tasksOrder)
        {
            foreach (var taskOrder in tasksOrder)
            {
                var order = taskOrder;
                ReleaseTaskRepository.Update(x => x.ExternalId == order.Key, x => x.Order = order.Value);
            }
        }

        public void CreateReleaseTask(ReleaseTask releaseTask)
        {
            var dataReleaseTask = Mapper.Map<ReleaseTask, ReleaseTaskDTO>(releaseTask);

            var releaseWindow = ReleaseWindowRepository.GetSatisfiedBy(x => x.ExternalId == releaseTask.ReleaseWindowId);
            if (releaseWindow == null)
                throw new ReleaseWindowNotFoundException(releaseTask.ReleaseWindowId);

            dataReleaseTask.ReleaseWindowId = releaseWindow.ReleaseWindowId;
            dataReleaseTask.CreatedOn = SystemTime.Now;
            // ReSharper disable once PossibleInvalidOperationException
            dataReleaseTask.CreatedByAccountId = GetAccountId(releaseTask.CreatedByExternalId).Value;
            dataReleaseTask.AssigneeAccountId = GetAccountId(releaseTask.AssigneeExternalId);
            dataReleaseTask.Order = GetLastOrderNumber(releaseWindow);
            ReleaseTaskRepository.Insert(dataReleaseTask);

            releaseTask.CreatedOn = dataReleaseTask.CreatedOn;
        }

        public void DeleteReleaseTask(Guid releaseTaskId)
        {
            var releaseTask = ReleaseTaskRepository
                .GetSatisfiedBy(x => x.ExternalId == releaseTaskId);

            if (releaseTask == null)
                throw new ReleaseTaskNotFoundException(releaseTaskId);

            ReleaseTaskRepository.Delete(releaseTask);
        }

        public void AssignHelpDeskTicket(ReleaseTask releaseTask, string ticketId, string ticketUrl)
        {
            ReleaseTaskRepository.Update(
                entity => entity.ExternalId == releaseTask.ExternalId,
                entity =>
                {
                    entity.HelpDeskReference = ticketId;
                    entity.HelpDeskUrl = ticketUrl;
                });
        }

        public void AddReleaseTaskAttachment(ReleaseTaskAttachment attachment)
        {
            var newAttachment = Mapper.Map<ReleaseTaskAttachment, ReleaseTaskAttachmentDTO>(attachment);

            var releaseTask = ReleaseTaskRepository.GetSatisfiedBy(x => x.ExternalId == attachment.ReleaseTaskId);

            if (releaseTask == null)
                throw new ReleaseTaskNotFoundException(attachment.ReleaseTaskId);

            newAttachment.ReleaseTaskId = releaseTask.ReleaseTaskId;

            ReleaseTaskAttachmentRepository.Insert(newAttachment);
        }

        public void DeleteReleaseTaskAttachment(Guid releaseTaskAttachmentId)
        {
            var attachment = ReleaseTaskAttachmentRepository
                .GetSatisfiedBy(x => x.ExternalId == releaseTaskAttachmentId);

            if (attachment == null)
                throw new ReleaseTaskAttachmentNotFoundException(releaseTaskAttachmentId);

            ReleaseTaskRepository.Delete(attachment);
        }

        public void ConfirmReleaseTaskReceipt(Guid releaseTaskId)
        {
            var task = ReleaseTaskRepository.GetSatisfiedBy(rt => rt.ExternalId == releaseTaskId);
            task.ReceiptConfirmedOn = SystemTime.Now;
            ReleaseTaskRepository.Update(task);
        }

        public void ClearReleaseTaskReceipt(Guid releaseTaskId)
        {
            var task = ReleaseTaskRepository.GetSatisfiedBy(rt => rt.ExternalId == releaseTaskId);
            task.ReceiptConfirmedOn = null;
            ReleaseTaskRepository.Update(task);
        }

        public void CompleteTask(Guid releaseTaskId, Guid completedById)
        {
            var releaseTask = ReleaseTaskRepository.GetSatisfiedBy(t => t.ExternalId == releaseTaskId);
            if (releaseTask.CompletedOn.HasValue)
            {
                throw new ReleaseTaskAlreadyCompletedException(releaseTaskId);
            }

            var completerId = AccountRepository.GetSatisfiedBy(a => a.ExternalId == completedById).AccountId;
            releaseTask.AssigneeAccountId = completerId;
            releaseTask.CompletedOn = SystemTime.Now;
            ReleaseTaskRepository.Update(releaseTask);
        }

        private int GetAccountId(Guid externalId)
        {
            var acc = AccountRepository.GetSatisfiedBy(o => o.ExternalId == externalId);

            if (acc != null)
            {
                if (acc.IsBlocked)
                    throw new AccountIsBlockedException(acc.ExternalId);

                return acc.AccountId;
            }

            throw new AccountNotFoundException(externalId);
        }

        private IEnumerable<TTask> LoadTasks<TTask>(Guid releaseWindowId)
        {
            var releaseWindow = ReleaseWindowRepository.GetSatisfiedBy(x => x.ExternalId == releaseWindowId);

            if (releaseWindow == null)
                throw new ReleaseWindowNotFoundException(releaseWindowId);

            var tasks = releaseWindow.ReleaseTasks
                    .OrderBy(x => x.Order)
                    .Select(x => Mapper.Map<ReleaseTaskDTO, TTask>(x))
                    .ToList();

            ////TODO: needto get this from navigation properties
            //foreach (var releaseTaskView in tasks)
            //{
            //    var attachments =
            //        ReleaseTaskAttachmentRepository.GetAllSortedSatisfiedBy(
            //            o => o.ReleaseTaskId == releaseTaskView.ReleaseTaskId);

            //    releaseTaskView.Attachments =
            //        attachments.Select(o => Mapper.Map<ReleaseTaskAttachmentDTO, ReleaseTaskAttachmentView>(o))
            //            .ToList();
            //}

            return tasks;
        }

        private TTask LoadTask<TTask>(Guid releaseTaskId)
        {
            var task = ReleaseTaskRepository.GetSatisfiedBy(x => x.ExternalId == releaseTaskId);

            if (task == null)
                throw new ReleaseTaskNotFoundException(releaseTaskId);

            return Mapper.Map<ReleaseTaskDTO, TTask>(task);

            ////TODO: needto get this from navigation properties
            //foreach (var releaseTaskView in tasks)
            //{
            //    var attachments =
            //        ReleaseTaskAttachmentRepository.GetAllSortedSatisfiedBy(
            //            o => o.ReleaseTaskId == releaseTaskView.ReleaseTaskId);

            //    releaseTaskView.Attachments =
            //        attachments.Select(o => Mapper.Map<ReleaseTaskAttachmentDTO, ReleaseTaskAttachmentView>(o))
            //            .ToList();
            //}
        }

        private int? GetAccountId(Guid? externalId)
        {
            if (externalId.HasValue)
                return GetAccountId(externalId.Value);

            return null;
        }

        private static short GetLastOrderNumber(ReleaseWindow releaseWindow)
        {
            return (short)(releaseWindow.ReleaseTasks.Any()
                ? releaseWindow.ReleaseTasks.Max(x => x.Order) + 1
                : 1);
        }
    }
}

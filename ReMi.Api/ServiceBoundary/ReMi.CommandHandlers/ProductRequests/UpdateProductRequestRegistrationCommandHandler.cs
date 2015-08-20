using System;
using System.Collections.Generic;
using System.Linq;
using ReMi.BusinessEntities.ProductRequests;
using ReMi.Commands.ProductRequests;
using ReMi.Common.Utils;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Events;
using ReMi.DataAccess.BusinessEntityGateways.ProductRequests;
using ReMi.Events.ProductRequests;

namespace ReMi.CommandHandlers.ProductRequests
{
    public class UpdateProductRequestRegistrationCommandHandler : IHandleCommand<UpdateProductRequestRegistrationCommand>
    {
        public Func<IProductRequestRegistrationGateway> ProductRequestRegistrationGatewayFactory { get; set; }
        public IPublishEvent PublishEvent { get; set; }

        public void Handle(UpdateProductRequestRegistrationCommand command)
        {
            using (var gateway = ProductRequestRegistrationGatewayFactory())
            {
                command.Registration.CreatedByAccountId = command.CommandContext.UserId;

                var existing = gateway.GetRegistration(command.Registration.ExternalId);

                gateway.UpdateProductRequestRegistration(command.Registration);

                var updatedTasks = GetUpdatedTasks(existing.Tasks, command.Registration.Tasks);
                if (updatedTasks != null)
                {
                    var updatedTasksList = updatedTasks.ToList();

                    if (updatedTasksList.Any())
                        PublishEvent.Publish(
                            new ProductRequestRegistrationUpdatedEvent
                            {
                                Context = command.CommandContext.CreateChild<EventContext>(),
                                Registration = command.Registration,
                                ChangedTasks = updatedTasksList
                            });
                }
            }
        }

        private IEnumerable<Guid> GetUpdatedTasks(IEnumerable<ProductRequestRegistrationTask> existingTasks, IEnumerable<ProductRequestRegistrationTask> incomeTasks)
        {
            var existingTasksList = (existingTasks ?? Enumerable.Empty<ProductRequestRegistrationTask>()).ToList();
            var incomeTasksList = (incomeTasks ?? Enumerable.Empty<ProductRequestRegistrationTask>()).ToList();

            if (!existingTasksList.Any() && !incomeTasksList.Any()) return Enumerable.Empty<Guid>();

            if ((incomeTasksList.Any() && !existingTasksList.Any())) return incomeTasksList.Select(o => o.ProductRequestTaskId).ToArray();

            var result = new List<Guid>();
            var crossed = existingTasksList
                .Join(incomeTasksList,
                    r => r.ProductRequestTaskId,
                    r => r.ProductRequestTaskId,
                    (existT, incomeT) =>
                        new Tuple<Guid, bool>(
                            existT.ProductRequestTaskId,
                            existT.IsCompleted != incomeT.IsCompleted || !string.Equals(existT.Comment, incomeT.Comment, StringComparison.CurrentCulture)
                            ))
                .ToList();

            result.AddRange(crossed.Where(o => o.Item2).Select(o => o.Item1));

            return result;
        }
    }
}

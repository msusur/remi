using System;
using System.Linq;
using AutoMapper;
using ReMi.Common.Utils.Repository;
using ReMi.DataAccess.Exceptions;
using ReMi.DataEntities.ExecPoll;
using CommandStateType = ReMi.BusinessEntities.ExecPoll.CommandStateType;

namespace ReMi.DataAccess.BusinessEntityGateways
{
    public class CommandPollGateway : BaseGateway, ICommandPollGateway
    {
        public IRepository<CommandExecution> CommandRepository { get; set; }

        public IRepository<CommandHistory> CommandHistoryRepository { get; set; }

        public IMappingEngine MappingEngine { get; set; }
        
        public BusinessEntities.ExecPoll.CommandExecution GetByExternalId(Guid externalId)
        {
            CommandExecution result = CommandRepository
                .GetAllSatisfiedBy(r => r.ExternalId == externalId)
                .FirstOrDefault();

            return MappingEngine.Map<CommandExecution, BusinessEntities.ExecPoll.CommandExecution>(result);
        }

        public void Create(Guid externalId, string description)
        {
            var command = new CommandExecution
            {
                ExternalId = externalId,
                Description = description,
            };

            CommandRepository.Insert(command);

            var newEntry = new CommandHistory
            {
                CommandExecutionId = command.CommandExecutionId,
                State = DataEntities.ExecPoll.CommandStateType.Waiting,
            };

            CommandHistoryRepository.Insert(newEntry);
        }

        public void SetState(Guid externalId, CommandStateType businessState, string details)
        {
            var dataState = MappingEngine.Map<CommandStateType, DataEntities.ExecPoll.CommandStateType>(businessState);

            CommandExecution command = CommandRepository.GetSatisfiedBy(p => p.ExternalId == externalId);
            if(command == null)
                throw new CommandExecutionNotFoundException(externalId);

            var newEntry = new CommandHistory
            {
                CommandExecutionId = command.CommandExecutionId,
                State = dataState,
                Details = details
            };

            CommandHistoryRepository.Insert(newEntry);
        }

        public override void OnDisposing()
        {
            CommandRepository.Dispose();
            CommandHistoryRepository.Dispose();

            base.OnDisposing();
        }
    }
}

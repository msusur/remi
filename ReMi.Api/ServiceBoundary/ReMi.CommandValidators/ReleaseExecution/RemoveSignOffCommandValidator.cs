using System;
using FluentValidation;
using ReMi.Commands.ReleaseExecution;
using ReMi.Common.Cqrs.FluentValidation;

namespace ReMi.CommandValidators.ReleaseExecution
{
    public class RemoveSignOffCommandValidator : RequestValidatorBase<RemoveSignOffCommand>
    {
        public RemoveSignOffCommandValidator()
        {
            RuleFor(x => x.SignOffId).Must(x => x != Guid.Empty);
        }
    }
}

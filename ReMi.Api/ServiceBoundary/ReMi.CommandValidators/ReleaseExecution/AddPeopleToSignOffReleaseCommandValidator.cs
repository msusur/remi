using System;
using System.Linq;
using FluentValidation;
using ReMi.Commands.ReleaseExecution;
using ReMi.Common.Cqrs.FluentValidation;

namespace ReMi.CommandValidators.ReleaseExecution
{
    public class AddPeopleToSignOffReleaseCommandValidator : RequestValidatorBase<AddPeopleToSignOffReleaseCommand>
    {
        public AddPeopleToSignOffReleaseCommandValidator()
        {
            RuleFor(x => x.ReleaseWindowId).Must(x => x != Guid.Empty);
            Unless(x => x.IsBackground, () => RuleFor(x => x.SignOffs)
                .Must(x => x.Count > 0 && x.All(a => a.ExternalId != Guid.Empty && a.Signer.ExternalId != Guid.Empty)));

        }
    }
}

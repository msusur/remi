using FluentValidation;
using ReMi.Commands.SourceControl;
using ReMi.Common.Cqrs.FluentValidation;

namespace ReMi.CommandValidators.SourceControl
{
    public class UpdateReleaseRepositoryCommandValidator : RequestValidatorBase<UpdateReleaseRepositoryCommand>
    {
        public UpdateReleaseRepositoryCommandValidator()
        {
            RuleFor(x => x.ReleaseWindowId).NotEmpty();
            RuleFor(x => x.Repository).NotNull();
            When(x => x.Repository != null, () =>
            {
                RuleFor(x => x.Repository.ChangesFrom).NotNull();
                RuleFor(x => x.Repository.ChangesTo).NotNull();
                RuleFor(x => x.Repository.ExternalId).NotEmpty();
                RuleFor(x => x.Repository.Repository).NotEmpty();
            });
        }
    }
}

using System;
using System.Linq;
using FluentValidation;
using ReMi.Commands.Auth;
using ReMi.Common.Cqrs.FluentValidation;
using ReMi.Common.Utils;

namespace ReMi.CommandValidators.Auth
{
    public class UpdateAccountPackagesCommandValidator : RequestValidatorBase<UpdateAccountPackagesCommand>
    {
        public UpdateAccountPackagesCommandValidator()
        {
            RuleFor(request => request.AccountId).NotEmpty();

            RuleFor(request => request.DefaultPackageId).NotEmpty();

            RuleFor(request => request.PackageIds).NotEmpty();
            When(x => !x.PackageIds.IsNullOrEmpty() && x.DefaultPackageId != Guid.Empty, () =>
            {
                RuleForEach(request => request.PackageIds).NotEmpty();
                RuleFor(x => x.PackageIds)
                    .Must((c, p) => p.Any(x => x == c.DefaultPackageId))
                    .WithMessage("DefaultPackageId must have been included into PackageIds");
            });
        }
    }
}

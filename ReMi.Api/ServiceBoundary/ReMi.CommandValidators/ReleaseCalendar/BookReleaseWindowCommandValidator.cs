using System.Linq;
using FluentValidation;
using ReMi.CommandValidators.Common;
using ReMi.Common.Constants.ReleaseCalendar;
using ReMi.Common.Cqrs.FluentValidation;
using ReMi.Commands.ReleaseCalendar;
using ReMi.Common.Utils;

namespace ReMi.CommandValidators.ReleaseCalendar
{
    public class BookReleaseWindowCommandValidator : RequestValidatorBase<BookReleaseWindowCommand>
    {
        public BookReleaseWindowCommandValidator()
        {
            RuleFor(request => request.ReleaseWindow).NotNull();

            RuleFor(request => request.ReleaseWindow.Products).NotEmpty();

            RuleFor(request => request.ReleaseWindow.Products)
                .SetCollectionValidator(new StringCollectionValidator("Products"))
                .WithMessage("At least one product should be specified")
                .WithName("Products");

            RuleFor(request => request.ReleaseWindow.Sprint)
                .NotEmpty()
                .Length(1, 128);

            RuleFor(request => request.ReleaseWindow.Description)
                .Length(0, 1024);

            RuleFor(request => request.ReleaseWindow.ExternalId).NotEmpty();

            RuleFor(request => request.ReleaseWindow.StartTime)
                .Must((instance, startTime) => startTime > SystemTime.Now)
                .WithMessage("StartTime ({0}) must be in the future", request => request.ReleaseWindow.StartTime);

            RuleFor(request => request.ReleaseWindow.EndTime)
                .Must((instance, endTime) => endTime > instance.ReleaseWindow.StartTime)
                .WithMessage("StartTime ({0}) must be less then EndTime ({1})", request => request.ReleaseWindow.StartTime, request => request.ReleaseWindow.EndTime);

            RuleFor(request => request.ReleaseWindow)
                .Must(releaseWindow => releaseWindow.ReleaseType != ReleaseType.Hotfix || !releaseWindow.RequiresDowntime)
                .WithMessage("Hotfix release cannot require down time");

            RuleFor(request => request.ReleaseWindow.ReleaseType)
                .Must(releaseType => releaseType != ReleaseType.Automated)
                .WithMessage("Cannot manually create Automated release");
        }
    }
}

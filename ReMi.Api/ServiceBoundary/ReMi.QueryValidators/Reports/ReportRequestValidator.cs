using FluentValidation;
using ReMi.Common.Cqrs.FluentValidation;
using ReMi.Queries.Reports;

namespace ReMi.QueryValidators.Reports
{
    public class ReportRequestValidator : RequestValidatorBase<ReportRequest>
    {
        public ReportRequestValidator()
        {
            RuleFor(x => x.ReportName).NotEmpty();
        }
    }
}

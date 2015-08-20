using FluentValidation;
using FluentValidation.Results;
using System.Collections.Generic;
using System.Linq;
using ReMi.Contracts.Cqrs;

namespace ReMi.Common.Cqrs.FluentValidation
{
	public class RequestValidatorBase<TRequest> : AbstractValidator<TRequest>, IValidateRequest<TRequest>
    {
		public IEnumerable<ValidationError> ValidateRequest(TRequest request)
		{
			ValidationResult result = Validate(request);

			if (result.IsValid)
			{
				return Enumerable.Empty<ValidationError>();
			}

			return BuildValidationErrors(result.Errors);
		}

		private IEnumerable<ValidationError> BuildValidationErrors(IEnumerable<ValidationFailure> failures)
		{
			foreach (ValidationFailure failure in failures)
			{
				yield return failure.ToValidationError();
			}
		}
	}
}

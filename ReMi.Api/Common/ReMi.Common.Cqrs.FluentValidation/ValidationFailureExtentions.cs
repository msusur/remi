
using FluentValidation.Results;
using ReMi.Contracts.Cqrs;

namespace ReMi.Common.Cqrs.FluentValidation
{
	public static class ValidationFailureExtentions
	{
		public static ValidationError ToValidationError(this ValidationFailure failure)
		{
			return new ValidationError(failure.ErrorMessage, failure.PropertyName);
		}
	}
}

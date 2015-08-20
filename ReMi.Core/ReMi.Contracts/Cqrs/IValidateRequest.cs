using System.Collections.Generic;

namespace ReMi.Contracts.Cqrs
{
	public interface IValidateRequest<TRequest>
	{
		/// <summary>
		/// Validates the incoming request and returns collection of errors
		/// </summary>
		/// <param name="request">the request to validate</param>
		/// <returns>empty validation errors if request is valid</returns>
		IEnumerable<ValidationError> ValidateRequest(TRequest request);
	}
}

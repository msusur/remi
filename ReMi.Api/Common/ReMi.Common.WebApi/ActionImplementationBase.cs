using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Autofac.Core.Registration;
using Common.Logging;
using ReMi.Common.WebApi.Exceptions;
using ReMi.Contracts.Cqrs;
using ReMi.Contracts.Cqrs.Queries;

namespace ReMi.Common.WebApi
{
	public abstract class ActionImplementationBase<TRequest> where TRequest : IQuery
	{
		protected readonly ILog Logger;

        public IValidateRequest<TRequest> Validator { get; set; }

		protected readonly bool _allowUndefinedValidator;

		static ActionImplementationBase()
		{
			//cant dispose it right after usage because this is only created at application start up 
		}

		protected ActionImplementationBase(bool allowUndefinedValidator = true)
		{
			//making sure we log with the real caller name (the derived class)
			Logger = LogManager.GetLogger(GetType());
			_allowUndefinedValidator = allowUndefinedValidator;
		}

		protected void ThrowHttpInternalServerErrorAndLog(Exception exception)
		{
			Logger.FatalFormat("Exception while executing service action: {0}.{1}{2}", GetType().Name, Environment.NewLine, exception);

			ThrowHttpInternalServerError(exception);
		}

		protected void ThrowHttpInternalServerError(Exception exception)
		{
			var message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
			message.ReasonPhrase = string.Format("Fatal Exception for {0}", GetType().Name);
			message.Content = new StringContent(exception.Message);		//do not return internal details
            
			var ex = new HttpResponseException(message);
            Logger.Error(message.Content, exception);
		    throw ex;
		}

        protected void ThrowHttpNotAcceptableAndLog(IEnumerable<ValidationError> validationErrors)
		{
			Logger.ErrorFormat("Invalid request: {0}", FormatValidationErrors(validationErrors));

            ThrowHttpNotAcceptable(validationErrors);
		}

        protected void ThrowHttpNotAcceptable(IEnumerable<ValidationError> validationErrors)
        {
            var message = new HttpResponseMessage(HttpStatusCode.NotAcceptable);
            message.ReasonPhrase = string.Format("Invalid request for {0}", GetType().Name);
            message.Content = new StringContent(FormatValidationErrors(validationErrors));

            var ex = new HttpResponseException(message);
            Logger.Error(ex);
            throw ex;
        }

        protected void ThrowHttpStatusError(HttpStatusCode statusCode, string errorMessege)
        {
            var message = new HttpResponseMessage(statusCode);
            message.ReasonPhrase = errorMessege;

            var ex = new HttpResponseException(message);
            Logger.Error(ex);
            throw ex;
        }

		private string FormatValidationErrors(IEnumerable<ValidationError> validationErrors)
		{
			return string.Join("; ", validationErrors.Select( e => string.Format("{0}: {1}", e.PropertyName, e.ErrorMessage)));
		}

		protected void AssertRequestNotNull(TRequest request)
		{
			if (request == null)
			{
				throw new ArgumentNullException("request");
			}
		}

		protected IEnumerable<ValidationError> ResolveAndInvokeValidateRequest(TRequest request)
		{
            try
            {
                if (Validator == null)
                {
                    return ProcessUndefinedValidator();
                }
            }
            catch (ComponentNotRegisteredException ex)
            {
                return ProcessUndefinedValidator(ex);
            }

			return Validator.ValidateRequest(request);
		}

        protected IEnumerable<ValidationError> ProcessUndefinedValidator(Exception innerException = null)
        {
            if (_allowUndefinedValidator)
            {
                return Enumerable.Empty<ValidationError>();
            }

            throw new RequestValidatorNotImplementedException<TRequest>(innerException);
        }
	}
}

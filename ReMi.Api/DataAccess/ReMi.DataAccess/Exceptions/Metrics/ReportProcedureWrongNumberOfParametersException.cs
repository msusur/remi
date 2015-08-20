using System;

namespace ReMi.DataAccess.Exceptions.Metrics
{
    public class ReportProcedureWrongNumberOfParametersException : ApplicationException
    {
        public ReportProcedureWrongNumberOfParametersException(string procedureName)
            : base(FormatMessage(procedureName))
        {
        }

       public ReportProcedureWrongNumberOfParametersException(string procedureName, Exception innerException)
            : base(FormatMessage(procedureName), innerException)
        {
        }

       private static string FormatMessage(String procedureName)
       {
           return string.Format("Report procedure: '{0}' has wrong number of parameters", procedureName);
       }
    }
}

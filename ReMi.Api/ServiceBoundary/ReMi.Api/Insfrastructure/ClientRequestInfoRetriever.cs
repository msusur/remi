using System.Web;

namespace ReMi.Api.Insfrastructure
{
    public class ClientRequestInfoRetriever : IClientRequestInfoRetriever
    {
        public string UserHostAddress
        {
            get
            {
                return HttpContext.Current == null
                    ? string.Empty
                    : HttpContext.Current.Request.UserHostAddress;
            }
        }

        public string UserHostName
        {
            get
            {
                return HttpContext.Current == null
                    ? string.Empty
                    : HttpContext.Current.Request.UserHostName;
            }
        }
    }
}

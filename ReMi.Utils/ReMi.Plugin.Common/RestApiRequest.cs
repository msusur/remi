using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using ReMi.Plugin.Common.Serialization;
using RestSharp;

namespace ReMi.Plugin.Common
{
    public abstract class RestApiRequest
    {
        //protected static readonly ILog Log = LogManager.GetCurrentClassLogger();

        public abstract string BaseUrl { get; }

        protected string ApiKey { get; set; }
        protected virtual string UserName { get; set; }
        protected virtual string Password { get; set; }

        protected virtual RestClient Client
        {
            get
            {
                var client = new RestClient { BaseUrl = BaseUrl };

                client.RemoveHandler("application/json");
                client.RemoveHandler("text/json");

                client.AddHandler("application/json", new JsonSerializationImpl());
                client.AddHandler("text/json", new JsonSerializationImpl());

                return client;
            }
        }

        protected virtual string GetAuthorizationHeader()
        {
            return String.IsNullOrEmpty(ApiKey)
                ? String.Format("Basic {0}", GetAuthToken())
                : String.Format("Token token={0}", ApiKey);
        }

        protected virtual string GetAuthToken()
        {
            return
                Convert.ToBase64String(
                    Encoding.UTF8.GetBytes(
                        string.Format("{0}:{1}", UserName, Password)
                        )
                    );
        }

        protected virtual Dictionary<String, String> DefaultParameters
        {
            get { return new Dictionary<string, string>(); }
        }

        protected virtual DataFormat ContentType
        {
            get { return DataFormat.Json; }
        }

        public virtual IRestResponse<T> Get<T>(string resource, Dictionary<String, String> parameters = null)
            where T : class, new()
        {
            return ExecuteRequest<T>(resource, Method.GET, null, parameters);
        }

        public virtual T GetData<T>(string resource, Dictionary<string, string> parameters = null)
            where T : class, new()
        {
            var response = Get<T>(resource, parameters);
            if (response.StatusCode == HttpStatusCode.OK)
                return response.Data;

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                //if (!string.IsNullOrWhiteSpace(response.Content))
                //Log.Debug("Returned response:\n" + response.Content);

                return null;
            }

            throw new RestApiRequestException("Requests is failed!", response.Content, response.ErrorException);
        }

        public virtual IRestResponse Get(string resource, Dictionary<String, String> parameters = null)
        {
            return ExecuteRequest(resource, Method.GET, null, parameters);
        }

        public virtual IRestResponse<T> Post<T>(string resource, object data, Dictionary<String, String> parameters = null)
            where T : class, new()
        {
            return ExecuteRequest<T>(resource, Method.POST, data, parameters);
        }

        public virtual IRestResponse<T> PostWithHeaders<T>(string resource, object data, Dictionary<String, String> headers = null)
            where T : class, new()
        {
            return ExecuteRequestWithHeaders<T>(resource, Method.POST, data, headers);
        }

        public virtual IRestResponse<T> UploadFile<T>(string resource, object data, IDictionary<string, byte[]> files, Dictionary<String, String> parameters = null)
            where T : class, new()
        {
            return ExecuteRequest<T>(resource, Method.POST, data, parameters, files);
        }

        public virtual IRestResponse<T> Put<T>(string resource, object data, Dictionary<String, String> parameters = null)
            where T : class, new()
        {
            return ExecuteRequest<T>(resource, Method.PUT, data, parameters);
        }

        public virtual IRestResponse<T> Delete<T>(string resource, Dictionary<String, String> parameters = null)
            where T : class, new()
        {
            return ExecuteRequest<T>(resource, Method.DELETE, null, parameters);
        }

        protected virtual IRestResponse ExecuteRequest(string resource, Method method, object data,
            Dictionary<String, String> parameters = null, IDictionary<string, byte[]> files = null)
        {
            var request = CreateRestRequest(resource, method, data, parameters, files);

            var response = Client.Execute(request);

            return response;
        }

        protected virtual IRestResponse<T> ExecuteRequest<T>(string resource, Method method, object data,
            Dictionary<String, String> parameters = null, IDictionary<string, byte[]> files = null)
            where T : class, new()
        {
            var request = CreateRestRequest(resource, method, data, parameters, files);

            //Log.DebugFormat("Sending request by Url={0}, Method={1}", Client.BuildUri(request), request.Method);

            var response = Client.Execute<T>(request);

            //Log.DebugFormat("Response received. Status={0}[{1}]", (int)response.StatusCode, response.StatusCode);

            return response;
        }

        protected virtual IRestResponse<T> ExecuteRequestWithHeaders<T>(string resource, Method method, object data, Dictionary<String, String> headers = null)
            where T : class, new()
        {
            var request = CreateRestRequest(resource, method, data, null, null);

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.AddHeader(header.Key, header.Value);
                }
            }

            var response = Client.Execute<T>(request);

            return response;
        }

        private RestRequest CreateRestRequest(string resource, Method method, object data,
            Dictionary<string, string> parameters,
            IDictionary<string, byte[]> files)
        {
            var request = new RestRequest(resource, method)
            {
                RequestFormat = ContentType,
                JsonSerializer = new JsonSerializationImpl()
            };

            if (data != null)
                request.AddBody(data);

            if (!string.IsNullOrWhiteSpace(GetAuthorizationHeader()))
                request.AddHeader("Authorization", GetAuthorizationHeader());
            if (files != null && files.Any())
            {
                foreach (var file in files)
                {
                    request.AddFile(file.Key, file.Value, file.Key);
                }
                request.AddHeader("Accept", "application/json");
            }

            var requestParameters =
                (DefaultParameters ?? new Dictionary<string, string>())
                    .Concat(parameters ?? new Dictionary<string, string>())
                    .Where(item => !String.IsNullOrEmpty(item.Key) && !String.IsNullOrEmpty(item.Value))
                    .ToList();

            foreach (var requestParameter in requestParameters)
                request.AddParameter(requestParameter.Key, requestParameter.Value);

            return request;
        }
    }
}

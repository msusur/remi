using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using Common.Logging;
using ReMi.Common.Utils;

namespace ReMi.Api.Controllers
{
    [RoutePrefix("upload")]
    [EnableCors(origins: "*", headers: "*", methods: "*", exposedHeaders: "X-Custom-Header", SupportsCredentials = true)]
    public class UploadController : ApiController
    {
        private static readonly ILog Logger = LogManager.GetCurrentClassLogger();

        public ISerialization Serialization { get; set; }

        [HttpPost]
        [Route]
        public async Task<HttpResponseMessage> Post()
        {
            // Check if the request contains multipart/form-data.
            if (!Request.Content.IsMimeMultipartContent())
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);

            Logger.InfoFormat("Files for upload received.");

            string root = HttpContext.Current.Server.MapPath("~/App_Data");
            var provider = new MultipartFormDataStreamProvider(root);

            try
            {
                await Request.Content.ReadAsMultipartAsync(provider);

                var items = new List<dynamic>();

                foreach (MultipartFileData file in provider.FileData)
                {
                    items.Add(new { name = Path.GetFileName(file.LocalFileName), size = new FileInfo(file.LocalFileName).Length });
                }

                Logger.InfoFormat("Uploaded files: {0}", Serialization.ToJson(items));

                return Request.CreateResponse(HttpStatusCode.OK, items);
            }
            catch (System.Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e);
            }
        }
    }
}

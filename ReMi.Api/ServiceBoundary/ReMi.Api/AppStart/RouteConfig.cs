using System.Web.Mvc;
using System.Web.Routing;

namespace ReMi.Api
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Uploader", // Route name
                "upload", // URL with parameters
                new { controller = "Upload", action = "Post" });
        }
    }
}

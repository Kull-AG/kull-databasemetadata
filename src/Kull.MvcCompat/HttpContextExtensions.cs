using System.Web;

namespace Kull.MvcCompat
{
    /// <summary>
    /// Some extension methods that are identical to .net Core
    /// </summary>
    public static class HttpContextExtensions
    {
        public static object GetRouteValue(this HttpContext httpContext, string key)
        {
            return httpContext.Request.RequestContext.RouteData.GetRequiredString(key);
        }

        public static object GetRouteValue(this HttpContextBase httpContext, string key)
        {
            return httpContext.Request.RequestContext.RouteData.GetRequiredString(key);
        }
        public static System.IO.Stream OpenReadStream(this HttpPostedFileBase file)
        {
            return file.InputStream;
        }
    }
}

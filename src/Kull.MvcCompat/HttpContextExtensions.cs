using System.Web;

namespace Kull.MvcCompat
{
    public static class HttpContextExtensions
    {
        public static object GetRouteValue(this HttpContext httpContext, string key)
        {
            return httpContext.Request.RequestContext.RouteData.GetRequiredString(key);
        }
    }
}

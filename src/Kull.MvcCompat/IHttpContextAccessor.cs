using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Kull.MvcCompat
{
    // see https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-context?view=aspnetcore-5.0
    public interface IHttpContextAccessor
    {
        HttpContextBase ? HttpContext { get; }
    }

    public class HttpContextAccessor : IHttpContextAccessor
    {
        private HttpContextBase? _context;
        public HttpContextBase? HttpContext
        {
            get
            {
                _context = _context ?? (System.Web.HttpContext.Current == null ? null : new System.Web.HttpContextWrapper(System.Web.HttpContext.Current));
                return _context;
            }
        }
    }
}

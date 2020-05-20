using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Kull.MvcCompat
{
    public interface IHostingEnvironment
    {
        string ContentRootPath { get; }
    }
    public class HostingEnvironment : IHostingEnvironment
    {
        public string ContentRootPath
        {
            get
            {
                return HttpRuntime.AppDomainAppVirtualPath != null ?
    HttpRuntime.AppDomainAppPath :
    System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            }
        }
    }
}

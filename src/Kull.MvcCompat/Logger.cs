using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kull.MvcCompat
{
    public class Logger<T> : ILogger<T>
    {
        public void LogInformation(string v, params object[] args)
        {
            Trace.TraceInformation(v, args);
        }
        public void LogError(string v, params object[] args)
        {
            Trace.TraceError(v, args);
        }
        public void LogError(Exception err, string v, params object[] args)
        {
            Trace.TraceError(err.ToString () + Environment.NewLine + string.Format(v, args));
        }
        public void LogWarning(string v, params object[] args)
        {
            Trace.TraceWarning(v, args);
        }
    }
}

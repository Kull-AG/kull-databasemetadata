using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kull.MvcCompat
{
    public class TraceLogger<T> : ILogger<T>
    {
        private TraceSource ts = new TraceSource(typeof(T).FullName, SourceLevels.Warning);

        public void LogInformation(string v, params object[] args)
        {
            ts.TraceInformation(v, args);
        }
        public void LogError(string v, params object[] args)
        {
            ts.TraceEvent(TraceEventType.Error, 0, v, args);
        }
        public void LogError(Exception err, string v, params object[] args)
        {
            ts.TraceEvent(TraceEventType.Error, 0, err.ToString () + Environment.NewLine + string.Format(v, args));
        }
        public void LogWarning(string v, params object[] args)
        {
            ts.TraceEvent(TraceEventType.Warning,0, v, args);
        }
    }
}

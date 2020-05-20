using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kull.MvcCompat
{
    public interface ILogger
    {
        void LogWarning(string v, params object[] messages);
        void LogInformation(string v, params object[] args);
        void LogError(string v, params object[] args);
        void LogError(Exception err, string v, params object[] args);
    }
    public interface ILogger<T> : ILogger { }
}

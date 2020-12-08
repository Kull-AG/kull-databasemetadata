using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace Kull.MvcCompat.Test
{
    public class DisposeTracker : IDisposable
    {
        private Guid guid = Guid.NewGuid();

        public DisposeTracker()
        {
            Debug.WriteLine($"{guid.ToString("N").Substring(0, 5)} created");
        }
        public void Dispose()
        {
            Debug.WriteLine($"{guid.ToString("N").Substring(0, 5)} disposed");
        }

        public string GetGuidString() => guid.ToString("N").Substring(0, 5);
    }

    public class DisposeTracker2 : IDisposable
    {
        private Guid guid = Guid.NewGuid();

        public DisposeTracker2()
        {
            Debug.WriteLine($"DisposeTracker2 {guid.ToString("N").Substring(0, 5)} created");
        }
        public void Dispose()
        {
            Debug.WriteLine($"DisposeTracker2 {guid.ToString("N").Substring(0, 5)} disposed");
        }

        public string GetGuidString() => guid.ToString("N").Substring(0, 5);
    }
}
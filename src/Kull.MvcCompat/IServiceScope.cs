using System;

namespace Kull.MvcCompat
{

    public interface IServiceScope : IDisposable
    {
        public IServiceProvider ServiceProvider { get; }
    }

    public interface IServiceScopeFactory
    {
        public IServiceScope CreateScope();
    }
}
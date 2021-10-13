using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kull.DatabaseMetadata.Test
{
    class TestLogger<T> : Microsoft.Extensions.Logging.ILogger<T>
    {
        private readonly TestContext context;

        public TestLogger(TestContext context)
        {
            this.context = context;
        }

        class NoOpDisposable : IDisposable
        {
            public void Dispose()
            {
                throw new NotImplementedException();
            }
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return new NoOpDisposable();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            context.WriteLine(formatter(state, exception));
        }
    }
}

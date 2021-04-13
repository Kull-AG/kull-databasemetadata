using Kull.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Kull.DatabaseMetadata
{
    public sealed class SPParameterProviderMemoryCache : ISPParameterProviderCache
    {
        private readonly ConcurrentDictionary<string, IReadOnlyCollection<SPParameter>> spParameters = new();

        public Task<IReadOnlyCollection<SPParameter>?> TryGetValue(DBObjectName sp)
        {
            if (spParameters.TryGetValue(sp.ToString(false), out var value))
            {
                return Task.FromResult<IReadOnlyCollection<SPParameter>?>(value);
            }
            return Task.FromResult<IReadOnlyCollection<SPParameter>?>(null);
        }
        public Task<bool> TryAdd(DBObjectName sp, IReadOnlyCollection<SPParameter> sPParameters)
            => Task.FromResult(spParameters.TryAdd(sp.ToString(), sPParameters));
    }
}

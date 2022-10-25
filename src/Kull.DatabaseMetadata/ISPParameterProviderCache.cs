using System.Collections.Generic;
using System.Threading.Tasks;
using Kull.Data;

namespace Kull.DatabaseMetadata;

public interface ISPParameterProviderCache
{
    Task<IReadOnlyCollection<SPParameter>?> TryGetValue(DBObjectName sp);
    Task<bool> TryAdd(DBObjectName sp, IReadOnlyCollection<SPParameter> sPParameters);
}

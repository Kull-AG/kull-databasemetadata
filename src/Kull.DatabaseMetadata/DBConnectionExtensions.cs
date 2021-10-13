using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace Kull.DatabaseMetadata
{
    internal static class DBConnectionExtensions
    {
        public static bool IsSQLite(this DbConnection dbConnection) => dbConnection.GetType().Name.Equals("SqliteConnection", StringComparison.OrdinalIgnoreCase);
    }
}

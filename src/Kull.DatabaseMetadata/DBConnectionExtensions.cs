using System;
using System.Collections.Generic;
using System.Data.Common;
using Kull.Data;

namespace Kull.DatabaseMetadata;

internal static class DBConnectionExtensions
{
    public static bool IsSQLite(this DbConnection dbConnection) => dbConnection.GetType().Name.Equals("SqliteConnection", StringComparison.OrdinalIgnoreCase);
    public static bool IsMSSqlServer(this DbConnection dbConnection) =>
            dbConnection.GetType().FullName!.StartsWith("System.Data.SqlClient") || dbConnection.GetType().FullName!.StartsWith("Microsoft.Data.SqlClient");
}


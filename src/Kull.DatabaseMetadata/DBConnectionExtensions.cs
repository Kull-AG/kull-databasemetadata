using System;
using System.Collections.Generic;
using System.Data.Common;
using Kull.Data;

namespace Kull.DatabaseMetadata;

public static class DBConnectionExtensions
{
    public static string GetParameterSqlChar(this DbConnection dbConnection) => IsDuckDB(dbConnection) ? "$" : "@";
    public static string GetParameterNameChar(this DbConnection dbConnection) => IsDuckDB(dbConnection) ? "" : "@";

    public static bool IsDuckDB(this DbConnection dbConnection) => dbConnection.GetType().Name.Equals("DuckDBConnection", StringComparison.OrdinalIgnoreCase);
    public static bool IsSQLite(this DbConnection dbConnection) => dbConnection.GetType().Name.Equals("SqliteConnection", StringComparison.OrdinalIgnoreCase);
    public static bool IsMSSqlServer(this DbConnection dbConnection) =>
            dbConnection.GetType().FullName!.StartsWith("System.Data.SqlClient") || dbConnection.GetType().FullName!.StartsWith("Microsoft.Data.SqlClient");
}

internal static class CommandExtensions
{

    public static DbCommand FixParameterChars(this DbCommand cmd)
    {
        if (cmd.Parameters.Count == 0)
        {
            return cmd;
        }
        if (!cmd.Connection.IsDuckDB())
        {
            return cmd;
        }
        var text = cmd.CommandText;
        foreach (DbParameter item in cmd.Parameters)
        {
            text = text.Replace("@" + item.ParameterName, "$" + item.ParameterName);
        }
        cmd.CommandText = text;
        return cmd;
    }
}


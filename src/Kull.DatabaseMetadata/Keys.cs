using Kull.Data;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Linq;
using System.Threading.Tasks;

namespace Kull.DatabaseMetadata;

public class Keys
{
    /// <summary>
    /// Gets the columns which are in the primary key. Can be multiple per table or zero
    /// </summary>
    /// <param name="con">The Db Connection</param>
    /// <param name="tableName">The Table Name</param>
    /// <returns>A list of columns per Table</returns>
    public async Task<IReadOnlyCollection<(DBObjectName Table, string ColumnName)>> GetPrimaryKeyColumns(DbConnection con, DBObjectName? tableName = null)
    {
        if (con.IsSQLite())
        {
            if (tableName != null)
            {
                return (await GetSqlitePrimaryKeys(con, tableName)).Select(s => (tableName, s)).ToArray();
            }
            else
            {
                List<(DBObjectName Table, string columnName)> pks = new();
                var allTables = await (new DBObjects()).GetTablesAndViews(con, DBObjects.TableOrViewType.Table);
                foreach (var t in allTables)
                {
                    pks.AddRange((await GetSqlitePrimaryKeys(con, t.Name)).Select(s => (t.Name, s)));
                }
                return pks;
            }
        }

        string sql = @"SELECT t.TABLE_SCHEMA, t.TABLE_NAME, k.column_name AS PrimaryKeyColumn
FROM information_schema.table_constraints t
JOIN information_schema.key_column_usage k
 ON k.constraint_name = t.CONSTRAINT_NAME and k.table_schema=t.TABLE_SCHEMA and k.table_name=t.TABLE_name
WHERE t.constraint_type='PRIMARY KEY'
    AND (t.TABLE_SCHEMA=@Schema OR @Schema is null) AND (t.TABLE_NAME=@TableName OR @TableName is null)";
        var cmd = con.CreateCommand();
        cmd.CommandText = sql;
        cmd.CommandType = System.Data.CommandType.Text;
        cmd.AddCommandParameter("Schema", tableName?.Schema);
        cmd.AddCommandParameter("TableName", tableName?.Name);
        cmd.FixParameterChars();
        using (var rdr = await cmd.ExecuteReaderAsync())
        {
            if (!rdr.HasRows) return Array.Empty<(DBObjectName Table, string columnName)>();
            List<(DBObjectName Table, string columnName)> list = new();
            while (rdr.Read())
            {
                list.Add((new DBObjectName(rdr.GetNString("TABLE_SCHEMA"),
                        rdr.GetNString("TABLE_NAME")!),
                        rdr.GetNString("PrimaryKeyColumn")!));
            }
            return list;
        }
    }


    private static async Task<IReadOnlyCollection<string>> GetSqlitePrimaryKeys(DbConnection dbConnection, DBObjectName tableOrView)
    {
        await dbConnection.AssureOpenAsync();
        string sql = $"PRAGMA table_info({tableOrView.ToString(false, true)}) ";
        var cmd = dbConnection.CreateCommand();
        cmd.CommandText = sql;
        cmd.CommandType = System.Data.CommandType.Text;
        using (var rdr = await cmd.ExecuteReaderAsync())
        {
            if (!rdr.HasRows) return Array.Empty<string>();
            List<string> list = new List<string>();
            while (rdr.Read())
            {
                bool isPk = rdr.GetBoolean("pk");
                if (isPk)
                {
                    list.Add(rdr.GetNString("name")!);
                }

            }
            return list;
        }
    }
}

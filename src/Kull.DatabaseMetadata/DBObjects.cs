using Kull.Data;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;

namespace Kull.DatabaseMetadata
{
    public class DBObjects
    {
        public enum TableOrViewType { Table, View }
        public async Task<IReadOnlyCollection<(DBObjectName Name, TableOrViewType Type)>> GetTablesAndViews(DbConnection dbConnection, TableOrViewType? typeFilter = null)
        {
            await dbConnection.AssureOpenAsync();
            bool sqlite = dbConnection.IsSQLite();
            string sql = sqlite ? $"SELECT name, type from sqlite_master" : "SELECT TABLE_NAME, TABLE_TYPE, TABLE_SCHEMA FROM INFORMATION_SCHEMA.TABLES";
            if (sqlite && typeFilter == null)
            {
                sql += " WHERE type = 'view' OR type = 'table'";
            }
            else if (sqlite)
            {
                sql += " WHERE type = @type";
            }
            if (!sqlite && typeFilter != null)
            {
                sql += " WHERE TABLE_TYPE=@Type";
            }
            string? filterType = null;
            if (sqlite && typeFilter == TableOrViewType.Table)
            {
                filterType = "table";
            }
            else if (sqlite && typeFilter == TableOrViewType.View)
            {
                filterType = "view";
            }
            else if (typeFilter == TableOrViewType.Table)
            {
                filterType = "BASE TABLE";
            }
            else if (typeFilter == TableOrViewType.View)
            {
                filterType = "VIEW";
            }
            var cmd = dbConnection.CreateCommand();
            cmd.CommandText = sql;
            cmd.CommandType = System.Data.CommandType.Text;
            cmd.AddCommandParameter("@type", filterType);

            using (var rdr = await cmd.ExecuteReaderAsync())
            {
                if (!rdr.HasRows) return Array.Empty<(DBObjectName name, TableOrViewType type)>();
                List<(DBObjectName name, TableOrViewType type)> list = new();
                while (rdr.Read())
                {
                    string name = rdr.GetString(0);
                    string type = rdr.GetString(1).ToUpper();
                    string? schema = sqlite ? null : rdr.GetString(2);
                    TableOrViewType typeT = type == "VIEW" ? TableOrViewType.View : TableOrViewType.Table;
                    list.Add((new DBObjectName(schema, name), typeT));

                }
                return list;
            }
        }

    }
}

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

        public enum TableHistoryType 
        {
            NoHistory=0,
            HistoryTable=1,
            SystemVersionedTemporalTable=2
        }

        public enum HistoryRetentionUnitType {
            None=0,
            Day=3,
            Week=4,
            Month=5,
            Year=6
        }

        public record TableInformation
        {
            public TableInformation(TableOrViewType type, DBObjectName name, TableHistoryType historyType, int historyRetentionPeriod, HistoryRetentionUnitType historyRetentionUnit, int objectId, int? historyTableId)
            {
                Type = type;
                Name = name;
                HistoryType = historyType;
                HistoryRetentionPeriod = historyRetentionPeriod;
                HistoryRetentionUnit = historyRetentionUnit;
                ObjectId = objectId;
                HistoryTableId = historyTableId;
            }

            public TableOrViewType Type {get;init;}
            public DBObjectName Name {get;init;}
            public TableHistoryType HistoryType {get;init;}

            public int HistoryRetentionPeriod {get; init;}
            public HistoryRetentionUnitType HistoryRetentionUnit {get; init;}

            public int ObjectId {get;init;}
            public int? HistoryTableId{get;init;}

            
        }

        public async Task<IReadOnlyCollection<TableInformation>> GetTables(DbConnection dbConnection, 
            DBObjectName? dBObjectName = null)
        {
            await dbConnection.AssureOpenAsync();
            string sql = @"
SELECT sc.name AS SCHEMA_NAME, t.name aS TABLE_NAME, 'BASE TABLE' AS TABLE_TYPE, temporal_type, 
	history_retention_period,
	history_retention_period_unit,
	t.object_id,
	t.history_table_id
	FROM sys.tables t
		inner join sys.schemas sc on sc.schema_id=t.schema_id";
            if (dBObjectName != null )
            {
                sql += " WHERE t.name = @Name AND (sc.Name=@Schema OR @Schema IS NULL)";
            }
            var cmd = dbConnection.CreateCommand();
            cmd.CommandText = sql;
            cmd.CommandType = System.Data.CommandType.Text;
            cmd.AddCommandParameter("@Name", dBObjectName?.Name);
            cmd.AddCommandParameter("@Schema", dBObjectName?.Schema);

            using (var rdr = await cmd.ExecuteReaderAsync())
            {
                if (!rdr.HasRows) return Array.Empty<TableInformation>();
                List<TableInformation> list = new();
                while (rdr.Read())
                {
                    string schema = rdr.GetString(0);
                    string name = rdr.GetString(1);
                    string type = rdr.GetString(2).ToUpper();
                    int temporalType = rdr.GetInt32(3);
                    int history_retention_period = rdr.GetInt32(4);
                    int history_retention_period_unit = rdr.GetInt32(4);
                    int object_id = rdr.GetInt32(5);
                    int? history_table_id = rdr.GetNInt32(6);
                    TableOrViewType typeT = type == "VIEW" ? TableOrViewType.View : TableOrViewType.Table;
                    list.Add(new TableInformation(typeT, new DBObjectName(schema, name),
                        (TableHistoryType)temporalType,
                        history_retention_period, history_retention_period_unit <= 0 ? HistoryRetentionUnitType.None:
                            (HistoryRetentionUnitType)history_retention_period_unit,
                            object_id,
                            history_table_id));

                }
                return list;
            }
        }


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

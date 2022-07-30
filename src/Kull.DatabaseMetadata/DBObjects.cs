using Kull.Data;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace Kull.DatabaseMetadata
{
    public class DBObjects
    {
        public enum TableOrViewType { Table, View }

        public enum TableHistoryType : byte
        {
            NoHistory = 0,
            HistoryTable = 1,
            SystemVersionedTemporalTable = 2
        }

        public enum HistoryRetentionUnitType
        {
            None = 0,
            Day = 3,
            Week = 4,
            Month = 5,
            Year = 6
        }

        public record TableInformation
        {

            public TableInformation(TableOrViewType type, DBObjectName name, TableHistoryType historyType,
                int historyRetentionPeriod, HistoryRetentionUnitType historyRetentionUnit, int objectId, int? historyTableId,
                string? historyStartCol, string? historyEndCol)
            {
                Type = type;
                Name = name;
                HistoryType = historyType;
                HistoryRetentionPeriod = historyRetentionPeriod;
                HistoryRetentionUnit = historyRetentionUnit;
                ObjectId = objectId;
                HistoryTableId = historyTableId;
                HistoryTableStartColumnName = historyStartCol;
                HistoryTableEndColumnName = historyEndCol;
            }
            public TableInformation(TableOrViewType type, DBObjectName name)
                : this(type, name, TableHistoryType.NoHistory, -1, HistoryRetentionUnitType.None, -1, null, null, null)
            { }

            public TableOrViewType Type { get; init; }
            public DBObjectName Name { get; init; }
            public TableHistoryType HistoryType { get; init; }

            public int HistoryRetentionPeriod { get; init; }
            public HistoryRetentionUnitType HistoryRetentionUnit { get; init; }

            public int ObjectId { get; init; }
            public int? HistoryTableId { get; init; }

            public string? HistoryTableStartColumnName { get; set; }

            public string? HistoryTableEndColumnName { get; set; }

        }

        public Task<IReadOnlyCollection<TableInformation>> GetTables(DbConnection dbConnection,
            DBObjectName? dBObjectName = null, System.Threading.CancellationToken cancellationToken = default)
        {
            return GetTablesAndViewsWithDetails(dbConnection: dbConnection, dBObjectName: dBObjectName, filterType: TableOrViewType.Table, cancellationToken: cancellationToken);
        }
        public async Task<IReadOnlyCollection<TableInformation>> GetTablesAndViewsWithDetails(DbConnection dbConnection,
            DBObjectName? dBObjectName = null, TableOrViewType? filterType = null, System.Threading.CancellationToken cancellationToken = default)
        {
            await dbConnection.AssureOpenAsync(cancellationToken);
            if (dbConnection.IsSQLite())
            {
                return (await GetTablesAndViews(dbConnection, filterType, cancellationToken)).Where(t => dBObjectName == null ? true : dBObjectName == t.Name)
                        .Select(s => new TableInformation(s.Type, s.Name)).ToList();
            }
            string sql = @"
    
SELECT sc.name AS SCHEMA_NAME, t.name aS TABLE_NAME, TABLE_TYPE, temporal_type, 
	history_retention_period,
	history_retention_period_unit,
	t.object_id,
	t.history_table_id,
	sc1.name AS HistoryTableStartColumnName,	
	sc2.name AS HistoryTableEndColumnName
	FROM (SELECT 'BASE TABLE' AS TABLE_TYPE, ts.schema_id, ts.name, ts.object_id , ts.type, ts.temporal_type, ts.history_retention_period, 
        ts.history_retention_period_unit, ts.history_table_id FROM sys.tables ts
        UNION ALL 
        SELECT 'VIEW' AS TABLE_TYPE,ts.schema_id, ts.name, ts.object_id , ts.type, CONVERT(TINYINT,0) temporal_type, NULL history_retention_period, 
        NULL history_retention_period_unit, NULL history_table_id FROM sys.views ts) t
		inner join sys.schemas sc on sc.schema_id=t.schema_id
		left join sys.periods p on p.period_type=1 and p.object_id=t.object_id
		left join sys.columns sc1 ON sc1.column_id=p.start_column_id and sc1.object_id=t.object_id
		left join sys.columns sc2 ON sc2.column_id=p.end_column_id and sc2.object_id=t.object_id
        WHERE t.type IN ('S','U','V') AND (@type=t.TABLE_TYPE OR @type IS NULL)";
            if (dBObjectName != null)
            {
                sql += " AND t.name = @Name AND (sc.Name=@Schema OR @Schema IS NULL)";
            }
            var cmd = dbConnection.CreateCommand();
            cmd.CommandText = sql;
            cmd.CommandType = System.Data.CommandType.Text;
            cmd.AddCommandParameter("@Name", dBObjectName?.Name);
            cmd.AddCommandParameter("@Schema", dBObjectName?.Schema);
            cmd.AddCommandParameter<string?>("@Type", filterType == null ? null :
                filterType == TableOrViewType.Table ? "BASE TABLE" : "VIEW");

            using (var rdr = await cmd.ExecuteReaderAsync(cancellationToken))
            {
                if (!rdr.HasRows) return Array.Empty<TableInformation>();
                List<TableInformation> list = new();
                while (rdr.Read())
                {
                    string schema = rdr.GetString(0);
                    string name = rdr.GetString(1);
                    string type = rdr.GetString(2).ToUpper();
                    byte temporalType = rdr.GetByte(3);
                    int? history_retention_period = rdr.GetNInt32(4);
                    int? history_retention_period_unit = rdr.GetNInt32(5);
                    int object_id = rdr.GetInt32(6);
                    int? history_table_id = rdr.GetNInt32(7);
                    string? historyTableStartColumnName = rdr.GetNString(8);
                    string? historyTableEndColumnName = rdr.GetNString(9);
                    TableOrViewType typeT = type == "VIEW" ? TableOrViewType.View : TableOrViewType.Table;
                    list.Add(new TableInformation(typeT, new DBObjectName(schema, name),
                        (TableHistoryType)temporalType,
                        history_retention_period ?? -1, history_retention_period_unit <= 0 ? HistoryRetentionUnitType.None :
                            (HistoryRetentionUnitType)(history_retention_period_unit ?? 0),
                            object_id,
                            history_table_id, historyTableStartColumnName, historyTableEndColumnName));

                }
                return list;
            }
        }


        public async Task<IReadOnlyCollection<(DBObjectName Name, TableOrViewType Type)>> GetTablesAndViews(DbConnection dbConnection, TableOrViewType? typeFilter = null, System.Threading.CancellationToken cancellationToken = default)
        {
            await dbConnection.AssureOpenAsync(cancellationToken);
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

            using (var rdr = await cmd.ExecuteReaderAsync(cancellationToken))
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

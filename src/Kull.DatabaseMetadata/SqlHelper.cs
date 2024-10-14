using Kull.Data;
#if NETFX 
using Kull.MvcCompat;
#else
using Microsoft.Extensions.Logging;
#endif
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Kull.DatabaseMetadata;

public enum ResultSource
{
    Metadata = 1,
    Execution = 2,
    PersistedData = 3,
    None = 0
}

/// <summary>
/// A utility class for getting Types of User Defined Types and for getting the expected result from  query
/// </summary>
public class SqlHelper
{
    Regex validNameRegex = new Regex("[a-zA-Z][a-zA-Z-_ 0-9]*");

    private readonly ILogger<SqlHelper> logger;

    /// <summary>
    /// Creates a SQL Helper
    /// </summary>
    /// <param name="logger"></param>
    public SqlHelper(ILogger<SqlHelper> logger)
    {
        this.logger = logger;
    }
    private int? getMaxLength(int binaryMaxLength, SqlType dbType)
    {
        if (dbType == null) return null;
        if (binaryMaxLength == -1) return -1;
        if (dbType.NetType == typeof(string) || dbType.NetType == typeof(byte[]))
        {
            return binaryMaxLength / dbType.BytesPerChar;
        }
        return null;
    }
    public async Task<IReadOnlyCollection<SqlFieldDescription>> GetTableTypeFields(DbConnection dbConnection, DBObjectName tableType)
    {
        string sql = $@"
SELECT c.name as ColumnName,
	CASE WHEN t.name ='sysname' THEN 'nvarchar' ELSE t.name END AS TypeName,
	c.is_nullable,
c.max_length,
c.collation_name
FROM sys.columns c
	inner join sys.types t ON t.user_type_id=c.user_type_id
WHERE object_id IN (
  SELECT tt.type_table_object_id
  FROM sys.table_types tt 
	inner join sys.schemas sc ON sc.schema_id=tt.schema_id
  WHERE tt.name = @Name and sc.name=isnull(@Schema, schema_NAME())
);";
        await dbConnection.AssureOpenAsync();
        var cmd = dbConnection.CreateCommand();
        cmd.CommandText = sql;
        cmd.CommandType = System.Data.CommandType.Text;
        cmd.AddCommandParameter("Name", tableType.Name);
        cmd.AddCommandParameter("Schema", tableType.Schema);
        cmd.FixParameterChars();
        List<SqlFieldDescription> list = new List<SqlFieldDescription>();
        using (var rdr = await cmd.ExecuteReaderAsync())
        {
            while (rdr.Read())
            {
                var dbt = SqlType.GetSqlType(rdr.GetNString("TypeName")!);
                var binlength = Convert.ToInt32(rdr.GetValue(3));
                list.Add(new SqlFieldDescription(

                    isNullable: rdr.GetBoolean("is_nullable"),
                    name: rdr.GetNString("ColumnName")!,
                    dbType: dbt,
                    maxLength: getMaxLength(binlength, dbt),
                    collation: rdr.GetNString("collation_name")
                ));
            }
        }
        return list.ToArray();
    }

    public Task<IReadOnlyCollection<SqlFieldDescription>> GetTableOrViewFields(DbConnection dbConnection, DBObjectName tableOrView)
    {
        if (dbConnection.IsSQLite()) return GetSqliteColumns(dbConnection, tableOrView);
        return GetDatabaseColumnMetadata(dbConnection, tableOrView, "COLUMNS");
    }

    public Task<IReadOnlyCollection<SqlFieldDescription>> GetFunctionFields(DbConnection dbConnection, DBObjectName tableOrView)
    {
        return GetDatabaseColumnMetadata(dbConnection, tableOrView, "ROUTINE_COLUMNS");
    }



    private static async Task<IReadOnlyCollection<SqlFieldDescription>> GetSqliteColumns(DbConnection dbConnection, DBObjectName tableOrView)
    {
        await dbConnection.AssureOpenAsync();
        string sql = $"PRAGMA table_info({tableOrView.ToString(false, true)}) ";
        var cmd = dbConnection.CreateCommand();
        cmd.CommandText = sql;
        cmd.CommandType = System.Data.CommandType.Text;
        using (var rdr = await cmd.ExecuteReaderAsync())
        {
            if (!rdr.HasRows) return Array.Empty<SqlFieldDescription>();
            List<SqlFieldDescription> list = new List<SqlFieldDescription>();
            while (rdr.Read())
            {
                list.Add(new SqlFieldDescription(
                    isNullable: !rdr.GetBoolean("notnull"),
                    name: rdr.GetNString("name")!,
                    dbType: SqlType.GetSqlType(rdr.GetNString("type")!),
                    maxLength: -1,
                    collation: null
                ));

            }
            return list;
        }
    }
    private static async Task<IReadOnlyCollection<SqlFieldDescription>> GetDatabaseColumnMetadata(DbConnection dbConnection, DBObjectName tableOrView,
            string informationschema_table)
    {

        string sql = $@"
SELEcT IS_NULLABLE AS is_nullable,  
	COLUMN_NAME as col_name,
	DATA_TYPE as type_name,
	CHARACTER_MAXIMUM_LENGTH as max_length,
    table_schema,
    collation_name
	FROM INFORMATION_SCHEMA.{informationschema_table} 
	WHERE TABLE_NAME= @Name AND (TABLE_SCHEMA=@Schema OR @Schema is null)";

        await dbConnection.AssureOpenAsync();
        var cmd = dbConnection.CreateCommand();
        cmd.CommandText = sql;
        cmd.CommandType = System.Data.CommandType.Text;

        cmd.AddCommandParameter("Name", tableOrView.Name);
        cmd.AddCommandParameter("Schema", tableOrView.Schema);
        cmd.FixParameterChars();

        using (var rdr = await cmd.ExecuteReaderAsync())
        {
            if (!rdr.HasRows) return Array.Empty<SqlFieldDescription>();
            List<SqlFieldDescription> list = new List<SqlFieldDescription>();
            int schemaOrdinal = rdr.GetOrdinal("table_schema");
            int maxLengthOrdinal = rdr.GetOrdinal("max_length");
            string? lastSchema = null;
            while (rdr.Read())
            {
                string? schema = rdr.GetString(schemaOrdinal);
                if (lastSchema == null)
                {
                    lastSchema = schema;
                }
                else if (schema != lastSchema)
                {
                    throw new InvalidOperationException("Schema Name is not given");
                }
                object nullable = rdr.GetValue(rdr.GetOrdinal("is_nullable"));
                object? maxLength = rdr.IsDBNull(maxLengthOrdinal) ? null : rdr.GetValue(maxLengthOrdinal);
                list.Add(new SqlFieldDescription(
                    isNullable: (nullable as string)?.ToUpperInvariant().Trim() == "YES" || nullable as Boolean? == true || nullable as int? == 1,
                    name: rdr.GetNString("col_name")!,
                    dbType: SqlType.GetSqlType(rdr.GetNString("type_name")!),
                    maxLength:
                    maxLength == null ? null :
                        maxLength is int i ? i :
                        maxLength is long l && l > (long)int.MaxValue ? null :
                        maxLength is ulong l2 && l2 > (ulong)int.MaxValue ? null :
                        Convert.ToInt32(maxLength),
                    collation: rdr.GetNString("collation_name")
                ));

            }
            return list;
        }
    }

    public async Task<SqlFieldDescription[]> GetSPResultSetByUsingExecute(DbConnection dbConnection, DBObjectName model,
       IReadOnlyDictionary<string, object?> fallBackExecutionParameters)
    {
        await dbConnection.AssureOpenAsync();
        ValidateNoSuspicousSql(model.Schema, false);
        ValidateNoSuspicousSql(model.Name, false);
        string procName = model.ToString(false, true);
        string paramText = string.Join(", ", fallBackExecutionParameters.Select(s => "@" + ValidateNoSuspicousSql(s.Key, true) + "=@" + s.Key));
        string commandText = (dbConnection.IsMSSqlServer() ? "set xact_abort on " : " ") +
$@"
begin tran
exec {procName} {paramText}
rollback";
        var cmd = dbConnection.CreateCommand();
        cmd.CommandText = commandText;
        cmd.CommandType = System.Data.CommandType.Text;
        foreach (var prms in fallBackExecutionParameters)
        {
            cmd.AddCommandParameter(prms.Key, prms.Value);
        }
        using (var res = await cmd.ExecuteReaderAsync())
        {
            res.Read();
            return Enumerable.Range(0, res.FieldCount)
                .Select(i => new SqlFieldDescription(
                    dbType: SqlType.GetSomeSqlType(res.GetFieldType(i)),
                    name: res.GetName(i),
                    isNullable: true,
                    maxLength: null
                )).ToArray();
        }
    }

    private string? ValidateNoSuspicousSql(string? name, bool forParam)
    {
        if (name == null) return null;
        if (name.Contains("--") || name.Contains("/") || name.Contains("*")) throw new ArgumentException(name);
        if (forParam && !validNameRegex.IsMatch(name))
        {
            throw new ArgumentException(name);
        }
        return name;
    }

    /// <summary>
    /// Gets the return fields of the first result set of a procecure
    /// </summary>
    /// <param name="model">The procedure</param>
    /// <param name="persistSPResultSetPath">True to save those result sets in ResultSets Folder</param>
    /// <param name="fallBackSPExecutionParameters">If you set this parameter and sp_describe_first_result_set does not work,
    /// the procedure will get executed to retrieve results. Pay attention to provide wise options!</param>
    /// <returns></returns>
    public async Task<(ResultSource source, IReadOnlyCollection<SqlFieldDescription> result)> GetResultSet2(DbConnection dbConnection,
       DBObjectName model,
       DBObjectType dBObjectType,
       string? persistSPResultSetPath,
       IReadOnlyDictionary<string, object?>? fallBackSPExecutionParameters)
    {
        switch (dBObjectType)
        {
            case DBObjectType.StoredProcedure:
                return await GetSPResultSet2(dbConnection, model, persistSPResultSetPath, fallBackSPExecutionParameters);
            case DBObjectType.TableOrView:
                return (ResultSource.Metadata, await GetTableOrViewFields(dbConnection, model));
            case DBObjectType.TableType:
                return (ResultSource.Metadata, await GetTableTypeFields(dbConnection, model));
            case DBObjectType.TableValuedFunction:
                return (ResultSource.Metadata, await GetFunctionFields(dbConnection, model));
            default:
                throw new NotSupportedException($"Db Type {dBObjectType} not supported");
        }
    }


    /// <summary>
    /// Gets the return fields of the first result set of a procecure
    /// </summary>
    /// <param name="model">The procedure</param>
    /// <param name="persistSPResultSetPath">True to save those result sets in ResultSets Folder</param>
    /// <param name="fallBackSPExecutionParameters">If you set this parameter and sp_describe_first_result_set does not work,
    /// the procedure will get executed to retrieve results. Pay attention to provide wise options!</param>
    /// <returns></returns>
    public Task<IReadOnlyCollection<SqlFieldDescription>> GetResultSet(DbConnection dbConnection,
       DBObjectName model,
       DBObjectType dBObjectType,
       string? persistSPResultSetPath,
       IReadOnlyDictionary<string, object?>? fallBackSPExecutionParameters = null)
    {
        switch (dBObjectType)
        {
            case DBObjectType.StoredProcedure:
                return GetSPResultSet(dbConnection, model, persistSPResultSetPath, fallBackSPExecutionParameters);
            case DBObjectType.TableOrView:
                return GetTableOrViewFields(dbConnection, model);
            case DBObjectType.TableType:
                return GetTableTypeFields(dbConnection, model);
            case DBObjectType.TableValuedFunction:
                return GetFunctionFields(dbConnection, model);
            default:
                throw new NotSupportedException($"Db Type {dBObjectType} not supported");
        }
    }



    /// <summary>
    /// Gets the return fields of the first result set of a procecure
    /// </summary>
    /// <param name="model">The procedure</param>
    /// <param name="persistResultSets">True to save those result sets in ResultSets Folder</param>
    /// <param name="fallBackExecutionParameters">If you set this parameter and sp_describe_first_result_set does not work,
    /// the procedure will get executed to retrieve results. Pay attention to provide wise options!</param>
    /// <returns></returns>
    public async Task<(ResultSource source, IReadOnlyCollection<SqlFieldDescription> result)> GetSPResultSet2(DbConnection dbConnection,
       DBObjectName model,
       string? persistResultSetPath,
       IReadOnlyDictionary<string, object?>? fallBackExecutionParameters = null)
    {
        SqlFieldDescription[]? dataToWrite = null;
        var cachejsonFile = persistResultSetPath != null ? System.IO.Path.Combine(persistResultSetPath,
            model.ToString() + ".json") : null;
        ResultSource resultSource;
        try
        {
            List<SqlFieldDescription> resultSet = new List<SqlFieldDescription>();
            await dbConnection.AssureOpenAsync();
            using (var rdr = await dbConnection.CreateSPCommand("sp_describe_first_result_set")
                .AddCommandParameter("tsql", model.ToString())
                .ExecuteReaderAsync())
            {
                while (rdr.Read())
                {
                    var dbt = SqlType.GetSqlType(rdr.GetNString("system_type_name")!);
                    resultSet.Add(new SqlFieldDescription(
                        name: rdr.GetNString("name")!,
                        dbType: dbt,
                        isNullable: rdr.GetBoolean("is_nullable"),
                        maxLength: getMaxLength(Convert.ToInt32(rdr.GetValue(rdr.GetOrdinal("max_length"))!),
                            dbt),
                        collation: rdr.GetNString("collation_name")
                    ));
                }
            }
            resultSource = ResultSource.Metadata;
            if (persistResultSetPath != null)
            {
                if (resultSet.Count > 0)
                {
                    try
                    {

                        if (!System.IO.Directory.Exists(persistResultSetPath))
                        {
                            System.IO.Directory.CreateDirectory(persistResultSetPath);
                        }

                        var jsAr = resultSet.Select(s => s.Serialize()).ToArray();
                        string json = SerializeJson(jsAr);
                        System.IO.File.WriteAllText(cachejsonFile!, json);

                    }
                    catch (Exception ercache)
                    {
                        logger.LogWarning("Could not cache Results set of {0}. Reason:\r\n{1}", model, ercache);
                    }
                }
            }
            dataToWrite = resultSet
                .Cast<SqlFieldDescription>()
                .ToArray();

        }
        catch (Exception err)
        {
            if ((err.GetType().FullName ?? "").Contains("SqlException"))
            {
                logger.LogWarning($"Error getting result set from {model}: {err.Message}");
            }
            else
            {
                logger.LogError(err, $"Error getting result set from {model}");
            }

            if (fallBackExecutionParameters != null)
            {
                dataToWrite = await GetSPResultSetByUsingExecute(dbConnection, model, fallBackExecutionParameters);
                resultSource = ResultSource.Execution;
                if (cachejsonFile != null)
                {
                    if (!System.IO.Directory.Exists(persistResultSetPath))
                    {
                        System.IO.Directory.CreateDirectory(persistResultSetPath!);
                    }
                    var jsAr = dataToWrite.Select(s => s.Serialize()).ToArray();
                    var json = SerializeJson(jsAr);
                    try
                    {
                        System.IO.File.WriteAllText(cachejsonFile, json);
                    }
                    catch
                    {
                        logger.LogWarning($"Cound not write cache file {cachejsonFile}");
                    }
                }
                return (resultSource, dataToWrite);
            }
            else
            {
                dataToWrite = null;
                resultSource = ResultSource.None;
            }
        }

        if (dataToWrite == null && persistResultSetPath != null && System.IO.File.Exists(cachejsonFile))
        {
            try
            {
                // Not Sucessfully gotten data
                var json = System.IO.File.ReadAllText(cachejsonFile);
                var resJS = DeserializeJson<List<Dictionary<string, object>>>(json);
                var res = resJS.Select(s => SqlFieldDescription.FromJson((IReadOnlyDictionary<string, object?>)s)).ToArray();
                resultSource = ResultSource.PersistedData;
                return (ResultSource.PersistedData, res.Cast<SqlFieldDescription>().ToArray());
            }
            catch (Exception err)
            {
                resultSource = ResultSource.None;
                logger.LogWarning("Could not get cache {0}. Reason:\r\n{1}", model, err);
            }
        }
        else
        {
            resultSource = ResultSource.None;
        }
        return (resultSource, dataToWrite ?? Array.Empty<SqlFieldDescription>());
    }


    /// <summary>
    /// Gets the return fields of the first result set of a procecure
    /// </summary>
    /// <param name="model">The procedure</param>
    /// <param name="persistResultSets">True to save those result sets in ResultSets Folder</param>
    /// <param name="fallBackExecutionParameters">If you set this parameter and sp_describe_first_result_set does not work,
    /// the procedure will get executed to retrieve results. Pay attention to provide wise options!</param>
    /// <returns></returns>
    public async Task<IReadOnlyCollection<SqlFieldDescription>> GetSPResultSet(DbConnection dbConnection,
       DBObjectName model,
       string? persistResultSetPath,
       IReadOnlyDictionary<string, object?>? fallBackExecutionParameters = null)
    {
        return (await GetSPResultSet2(dbConnection, model, persistResultSetPath, fallBackExecutionParameters)).result;
    }

    private static string SerializeJson(object jsAr)
    {
#if NET48 || NETSTANDARD1_0_OR_GREATER
        return Newtonsoft.Json.JsonConvert.SerializeObject(jsAr, Newtonsoft.Json.Formatting.Indented);
#else
        return System.Text.Json.JsonSerializer.Serialize(jsAr, jsAr.GetType(), new System.Text.Json.JsonSerializerOptions()
        {
            WriteIndented = true
        });
#endif
    }
    private static T DeserializeJson<T>(string json)
    {
#if NET48 || NETSTANDARD1_0_OR_GREATER
        return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json)!;
#else
        return System.Text.Json.JsonSerializer.Deserialize<T>(json)!;
#endif
    }
}

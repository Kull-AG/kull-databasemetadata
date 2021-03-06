﻿using Kull.Data;
#if NETFX 
using Kull.MvcCompat;
#else
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
#endif
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Kull.DatabaseMetadata
{
    /// <summary>
    /// A utility class for getting Types of User Defined Types and for getting the expected result from  query
    /// </summary>
    public class SqlHelper
    {
        Regex validNameRegex = new Regex("[a-zA-Z][a-zA-Z-_ 0-9]*");
        
        private readonly ILogger<SqlHelper> logger;

        public SqlHelper(ILogger<SqlHelper> logger)
        {
            this.logger = logger;
        }
        private int? getMaxLength(int binaryMaxLength, string? typeName)
        {
            if (typeName == null) return null;
            if (typeName == "varchar") return binaryMaxLength;
            if (typeName == "varbinary") return binaryMaxLength;
            if (typeName == "char") return binaryMaxLength;
            if (typeName == "binary") return binaryMaxLength;
            if (typeName == "nvarchar") return binaryMaxLength * 2;
            if (typeName == "nchar") return binaryMaxLength * 2;
            return null;
        }
        public async Task<IReadOnlyCollection<SqlFieldDescription>> GetTableTypeFields(DbConnection dbConnection, DBObjectName tableType)
        {
            string sql = $@"
SELECT c.name as ColumnName,
	CASE WHEN t.name ='sysname' THEN 'nvarchar' ELSE t.name END AS TypeName,
	c.is_nullable,
c.max_length
FROM sys.columns c
	inner join sys.types t ON t.user_type_id=c.user_type_id
WHERE object_id IN (
  SELECT tt.type_table_object_id
  FROM sys.table_types tt 
	inner join sys.schemas sc ON sc.schema_id=tt.schema_id
  WHERE tt.name = @Name and sc.name=@Schema
);";
            await dbConnection.AssureOpenAsync();
            var cmd = dbConnection.CreateCommand();
            cmd.CommandText = sql;
            cmd.CommandType = System.Data.CommandType.Text;
            cmd.AddCommandParameter("@Name", tableType.Name);
            cmd.AddCommandParameter("@Schema", tableType.Schema);
            List<SqlFieldDescription> list = new List<SqlFieldDescription>();
            using (var rdr = await cmd.ExecuteReaderAsync())
            {
                while (rdr.Read())
                {
                    list.Add(new SqlFieldDescription(

                        isNullable: rdr.GetBoolean("is_nullable"),
                        name: rdr.GetNString("ColumnName")!,
                        dbType: SqlType.GetSqlType(rdr.GetNString("TypeName")!),
                        maxLength: getMaxLength(Convert.ToInt32(rdr.GetValue(3)), rdr.GetNString("TypeName"))
                    ));
                }
            }
            return list.ToArray();
        }


        public async Task<SqlFieldDescription[]> GetSPResultSetByUsingExecute(DbConnection dbConnection, DBObjectName model,
           IReadOnlyDictionary<string, object?> fallBackExecutionParameters)
        {
            await dbConnection.AssureOpenAsync();
            ValidateNoSuspicousSql(model.Schema);
            ValidateNoSuspicousSql(model.Name);
            string procName = model.ToString();
            string paramText = string.Join(", ", fallBackExecutionParameters.Select(s => "@" + ValidateNoSuspicousSql(s.Key) + "=@" + s.Key));
            string commandText =
$@"set xact_abort on
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

        private string ValidateNoSuspicousSql(string name)
        {
            if (name.Contains("--") || name.Contains("/") || name.Contains("*")) throw new ArgumentException(name);
            if (!validNameRegex.IsMatch(name))
            {
                throw new ArgumentException(name);
            }
            return name;
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
            SqlFieldDescription[]? dataToWrite = null;
            var cachejsonFile = persistResultSetPath != null ? System.IO.Path.Combine(persistResultSetPath,
                model.ToString() + ".json") : null;
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
                        resultSet.Add(new SqlFieldDescription(
                            name: rdr.GetNString("name")!,
                            dbType: SqlType.GetSqlType(rdr.GetNString("system_type_name")!),
                            isNullable: rdr.GetBoolean("is_nullable"),
                            maxLength: getMaxLength(Convert.ToInt32(rdr.GetValue(rdr.GetOrdinal("max_length"))!),
                                SqlType.GetSqlType(rdr.GetNString("system_type_name")!).DbType)
                        ));
                    }
                }
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
                            var jsAr = new JArray(resultSet.Select(s => s.Serialize()).ToArray());
                            var json = JsonConvert.SerializeObject(jsAr, Formatting.Indented);

                            System.IO.File.WriteAllText(cachejsonFile, json);

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
                logger.LogError(err, $"Error getting result set from {model}");
                if (fallBackExecutionParameters != null)
                {
                    dataToWrite = await GetSPResultSetByUsingExecute(dbConnection, model, fallBackExecutionParameters);
                    if (cachejsonFile != null)
                    {
                        if (!System.IO.Directory.Exists(persistResultSetPath))
                        {
                            System.IO.Directory.CreateDirectory(persistResultSetPath);
                        }
                        var jsAr = new JArray(dataToWrite.Select(s => s.Serialize()).ToArray());
                        var json = JsonConvert.SerializeObject(jsAr, Formatting.Indented);
                        System.IO.File.WriteAllText(cachejsonFile, json);
                    }
                }
                else
                {
                    dataToWrite = null;
                }
            }

            if (dataToWrite == null && persistResultSetPath != null && System.IO.File.Exists(cachejsonFile))
            {
                try
                {
                    // Not Sucessfully gotten data
                    var json = System.IO.File.ReadAllText(cachejsonFile);
                    var resJS = JsonConvert.DeserializeObject<JArray>(json);
                    var res = resJS.Select(s => SqlFieldDescription.FromJObject((JObject)s)).ToArray();
                    return res.Cast<SqlFieldDescription>().ToArray();
                }
                catch (Exception err)
                {
                    logger.LogWarning("Could not get cache {0}. Reason:\r\n{1}", model, err);
                }
            }
            return dataToWrite ?? new SqlFieldDescription[] { };
        }
    }
}

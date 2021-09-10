using Kull.Data;
#if NETFX 
using Kull.MvcCompat;
#else 
using Microsoft.Extensions.Logging;
#endif
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace Kull.DatabaseMetadata
{
    /// <summary>
    /// Provider for getting parameters for a Stored Procedure
    /// </summary>
    public class SPParametersProvider
    {
        private readonly ILogger<SPParametersProvider> logger;
        private readonly ISPParameterProviderCache cache;

        public SPParametersProvider(ILogger<SPParametersProvider> logger,
            ISPParameterProviderCache cache)
        {
            this.logger = logger;
            this.cache = cache;
        }

        /// <summary>
        /// Get all parameter names of a Stored Procedure
        /// </summary>
        /// <returns></returns>
        public async Task<IReadOnlyCollection<SPParameter>> GetSPParameters(DBObjectName storedProcedure, DbConnection con)
        {
            var cachedValue = await cache.TryGetValue(storedProcedure);
            if (cachedValue != null)
                return cachedValue;


            string command = @"SELECT PARAMETER_NAME, DATA_TYPE, USER_DEFINED_TYPE_SCHEMA,
	USER_DEFINED_TYPE_NAME, PARAMETER_MODE, CHARACTER_MAXIMUM_LENGTH
FROM information_schema.parameters 
WHERE SPECIFIC_NAME = @SPName  AND SPECIFIC_SCHEMA=isnull(@Schema, schema_NAME()) AND PARAMETER_NAME<>''";
            await con.AssureOpenAsync();
            DbCommand cmd = con.CreateCommand();
            cmd.CommandText = command;
            cmd.AddCommandParameter("@SPName", storedProcedure.Name)
                .AddCommandParameter("@Schema", storedProcedure.Schema);
            List<SPParameter> resultL = new List<SPParameter>();

            using (var reader = await cmd.ExecuteReaderAsync())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var name = reader.GetString(0);
                        name = name.StartsWith("@") ? name.Substring(1) : name;
                        string type = reader.GetString(1);
                        (string userDefinedSchema, string userDefinedName) = (reader.GetNString(2)!, reader.GetNString(3)!);
                        DBObjectName? userDefinedType = type == "table type" ?
                            new DBObjectName(userDefinedSchema, userDefinedName) : null;
                        string parameterMode = reader.GetString(4);
                        System.Data.ParameterDirection? parameterDirection = parameterMode.Equals("IN", System.StringComparison.CurrentCultureIgnoreCase)
                                ? System.Data.ParameterDirection.Input:
                                parameterMode.Equals("OUT", System.StringComparison.CurrentCultureIgnoreCase) ? System.Data.ParameterDirection.Output:
                                parameterMode.Equals("INOUT", System.StringComparison.CurrentCultureIgnoreCase) ? System.Data.ParameterDirection.InputOutput
                                : (System.Data.ParameterDirection?) null;
                        if(parameterDirection == null)
                        {
                            logger.LogWarning($"Cannot parse Parameter mode {parameterMode} of {name} of {storedProcedure}");
                        }
                        int? maxLength = reader.GetNInt32(5);
                        resultL.Add(new SPParameter(name, type, userDefinedType, parameterDirection ?? System.Data.ParameterDirection.Input, maxLength));

                    }
                }
            }
            var result = resultL.ToArray();

            await cache.TryAdd(storedProcedure.ToString(), result);

            return result;
        }
    }
}

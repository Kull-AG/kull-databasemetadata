using Kull.Data;
#if NETFX 
using Kull.MvcCompat;
#else 
using Microsoft.Extensions.Logging;
#endif
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;

namespace Kull.DatabaseMetadata
{
    /// <summary>
    /// Provider for getting parameters for a Stored Procedure
    /// </summary>
    public class SPParametersProvider
    {
        private readonly ConcurrentDictionary<string, SPParameter[]> spParameters = new ConcurrentDictionary<string, SPParameter[]>();
        private readonly ILogger<SPParametersProvider> logger;

        public SPParametersProvider(ILogger<SPParametersProvider> logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Get all parameter names of a Stored Procedure
        /// </summary>
        /// <returns></returns>
        public SPParameter[] GetSPParameters(DBObjectName storedProcedure, DbConnection con)
        {
            if (spParameters.TryGetValue(storedProcedure.ToString(), out var spPrms))
                return spPrms;


            string command = @"SELECT PARAMETER_NAME, DATA_TYPE, USER_DEFINED_TYPE_SCHEMA,
	USER_DEFINED_TYPE_NAME, PARAMETER_MODE
FROM information_schema.parameters 
WHERE SPECIFIC_NAME = @SPName  AND SPECIFIC_SCHEMA=@Schema AND PARAMETER_NAME<>''";
            con.AssureOpen();
            DbCommand cmd = con.CreateCommand();
            cmd.CommandText = command;
            cmd.AddCommandParameter("@SPName", storedProcedure.Name)
                .AddCommandParameter("@Schema", storedProcedure.Schema ?? DBObjectName.DefaultSchema);
            List<SPParameter> resultL = new List<SPParameter>();

            using (var reader = cmd.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var name = reader.GetString(0);
                        name = name.StartsWith("@") ? name.Substring(1) : name;
                        string type = reader.GetString(1);
                        (string userDefinedSchema, string userDefinedName) = (reader.GetNString(2), reader.GetNString(3));
                        DBObjectName userDefinedType = type == "table type" ?
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
                        resultL.Add(new SPParameter(name, type, userDefinedType, parameterDirection ?? System.Data.ParameterDirection.Input));

                    }
                }
            }
            var result = resultL.ToArray();

            spParameters.TryAdd(storedProcedure.ToString(), result);

            return result;
        }
    }
}

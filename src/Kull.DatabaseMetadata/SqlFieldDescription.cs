using Kull.Data;
using System;
using System.Collections.Generic;
using System.Data;

namespace Kull.DatabaseMetadata
{
    /// <summary>
    /// A representation for a field in a table
    /// </summary>
    public record SqlFieldDescription
    {
        public string Name { get; init; }
        
        public SqlType DbType { get; init; }
        
        public bool IsNullable { get; init; }

        public int? MaxLength { get; init; }

        public SqlFieldDescription(string name, SqlType dbType, bool isNullable, int? maxLength)
        {
            this.Name = name;
            this.DbType = dbType;
            this.IsNullable = isNullable;
            this.MaxLength = maxLength == -1 ? null: maxLength;
        }
       
        public static SqlFieldDescription FromJson(IReadOnlyDictionary<string, object?> jObject)
        {
            var obj = new SqlFieldDescription(
                Convert.ToString(jObject.ContainsKey("name")?jObject["name"]:jObject["Name"])!,
                SqlType.GetSqlType(Convert.ToString(jObject.ContainsKey("system_type_name") ? jObject["system_type_name"] :  
                jObject["TypeName"])!),
                 Convert.ToBoolean(jObject.ContainsKey("is_nullable") ? jObject["is_nullable"] : jObject["IsNullable"]),

                 jObject.ContainsKey("max_length") && jObject["max_length"] != null ? Convert.ToInt32(jObject["max_length"]):
                 jObject.ContainsKey("MaxLength") && jObject["MaxLength"] != null ? Convert.ToInt32(jObject["MaxLength"]) : null);
            return obj;
        }

        public IReadOnlyDictionary<string, object?> Serialize()
        {
            var dict = new Dictionary<string, object?>()
            {
                { "Name", this.Name },
                { "TypeName", this.DbType.DbType },
                { "IsNullable", this.IsNullable },
                { "MaxLength", this.MaxLength }
            };
            return dict;
        }

    }

}

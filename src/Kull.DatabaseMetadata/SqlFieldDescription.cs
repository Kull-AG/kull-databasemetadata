﻿using Kull.Data;
using Newtonsoft.Json.Linq;
using System.Data;

namespace Kull.DatabaseMetadata
{
    /// <summary>
    /// A representation for a field in a table
    /// </summary>
    public class SqlFieldDescription
    {
        public string Name { get; }
        
        public SqlType DbType { get;  }
        
        public bool IsNullable { get;  }

        public int? MaxLength { get; }

        public SqlFieldDescription(string name, SqlType dbType, bool isNullable, int? maxLength)
        {
            this.Name = name;
            this.DbType = dbType;
            this.IsNullable = isNullable;
            this.MaxLength = maxLength;
        }
       
        public static SqlFieldDescription FromJObject(JObject jObject)
        {
            var obj = new SqlFieldDescription(
                jObject["name"]?.Value<string>() ?? jObject["Name"].Value<string>(),
                SqlType.GetSqlType(jObject["system_type_name"]?.Value<string>() ?? jObject["TypeName"].Value<string>()),
                jObject["is_nullable"]?.Value<bool>() ?? jObject["IsNullable"].Value<bool>(),
                jObject["max_length"]?.Value<int?>() ?? jObject["MaxLength"]?.Value<int?>()
                );
            return obj;
        }

        public JObject Serialize()
        {
            return new JObject(
                new JProperty("Name", this.Name),
                new JProperty("TypeName", this.DbType.DbType),
                new JProperty("IsNullable", this.IsNullable),
                new JProperty("MaxLength", this.MaxLength));
        }

    }

}

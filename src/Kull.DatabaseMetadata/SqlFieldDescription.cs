using Kull.Data;
using System;
using System.Collections.Generic;
using System.Data;

namespace Kull.DatabaseMetadata;

/// <summary>
/// A representation for a field in a table
/// </summary>
public record SqlFieldDescription
{
    public string Name { get; init; }

    public SqlType DbType { get; init; }

    public bool IsNullable { get; init; }

    public int? MaxLength { get; init; }

    public string? Collation { get; set; }

    public SqlFieldDescription(string name, SqlType dbType, bool isNullable, int? maxLength,
        string? collation)
    {
        this.Name = name;
        this.DbType = dbType;
        this.IsNullable = isNullable;
        this.MaxLength = maxLength == -1 ? null : maxLength;
        this.Collation = collation;
    }

    public SqlFieldDescription(string name, SqlType dbType, bool isNullable, int? maxLength)
    {
        this.Name = name;
        this.DbType = dbType;
        this.IsNullable = isNullable;
        this.MaxLength = maxLength == -1 ? null : maxLength;
    }

    private static string? ToString(object? o)
    {
        if (o == null) return null;
#if !NET48 && !NETSTANDARD1_1_OR_GREATER
        if (o is System.Text.Json.JsonElement je)
        {
            return je.GetString();
        }
#endif
        return Convert.ToString(o);
    }

    private static bool? ToBoolean(object? o)
    {
        if (o == null) return null;
#if !NET48 && !NETSTANDARD1_1_OR_GREATER
        if (o is System.Text.Json.JsonElement je)
        {
            if (je.ValueKind == System.Text.Json.JsonValueKind.Null) return null;
            return je.GetBoolean();
        }
#endif
        return Convert.ToBoolean(o);
    }
    private static int? ToInt32(object? o)
    {
        if (o == null) return null;
#if !NET48 && !NETSTANDARD1_1_OR_GREATER
        if (o is System.Text.Json.JsonElement je)
        {
            if (je.ValueKind == System.Text.Json.JsonValueKind.Null) return null;
            return je.GetInt32();
        }
#endif
        return Convert.ToInt32(o);
    }

    public static SqlFieldDescription FromJson(IReadOnlyDictionary<string, object?> jObject)
    {
        var obj = new SqlFieldDescription(
            name: ToString(jObject.ContainsKey("name") ? jObject["name"] : jObject["Name"])!,
            dbType: SqlType.GetSqlType(ToString(jObject.ContainsKey("system_type_name") ? jObject["system_type_name"] :
            jObject["TypeName"])!),
            isNullable: ToBoolean(jObject.ContainsKey("is_nullable") ? jObject["is_nullable"] : jObject["IsNullable"]) ?? true,

            maxLength: jObject.ContainsKey("max_length") && jObject["max_length"] != null ? ToInt32(jObject["max_length"]) :
              jObject.ContainsKey("MaxLength") && jObject["MaxLength"] != null ? ToInt32(jObject["MaxLength"]) : null,
            collation: ToString(jObject.ContainsKey("collation") ? jObject["collation"] : null)
            );
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


using Kull.Data;
using System;
using System.Data;
using System.Linq;

namespace Kull.DatabaseMetadata;

/// <summary>
/// Represents a parameter for a Stored Proc
/// </summary>
public class SPParameter
{
    public string SqlName { get; }

    /// <summary>
    /// Represents the type without length
    /// </summary>
    public SqlType DbType { get; }

    /// <summary>
    /// Gets the maximum length allowed. -1 is for unlimited
    /// It's the character_maximum_length not the binary one
    /// </summary>
    public int? MaxLength { get; }

    public bool IsNullable => true; // A parameter is always nullable

    public DBObjectName? UserDefinedType { get; }

    public ParameterDirection ParameterDirection { get; }

    internal SPParameter(string prmName, string db_type,
            DBObjectName? userDefinedType,
            ParameterDirection parameterDirection,
            int? maxLength)
    {
        this.SqlName = prmName;
        this.DbType = SqlType.GetSqlType(db_type);
        this.UserDefinedType = userDefinedType;
        this.ParameterDirection = parameterDirection;
        this.MaxLength = maxLength;
        if (this.SqlName.EndsWith("Json", StringComparison.CurrentCultureIgnoreCase)
                && this.DbType.JsType == "string")
        {
            this.DbType = SqlType.GetSqlType("json");
        }
    }

    public (string name, string? format) GetJSType()
    {
        return (this.DbType.JsType, this.DbType.JsFormat);
    }

}

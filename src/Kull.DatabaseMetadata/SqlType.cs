using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kull.DatabaseMetadata;

/// <summary>
/// A class representing a type of Sql server
/// Does not include the type
/// </summary>
public sealed class SqlType : IEquatable<SqlType>
{
    private readonly string dbType;
    private readonly System.Type netType;
    private readonly string jsType;
    private readonly string? jsFormat;

    private static List<SqlType>
        allTypes = new List<SqlType>(30);

    /// <summary>
    /// The name of the type in the Database System, eg nvarchar or bigint
    /// </summary>
    public string DbType => dbType;

    /// <summary>
    /// The Db type as in .Net
    /// </summary>
    /// <value></value>
    public System.Data.DbType DataDbType { get; }

    /// <summary>
    /// The (mostly primitive) .Net Type, eg System.String
    /// </summary>
    public Type NetType => netType;

    /// <summary>
    /// The Type in JavaScript
    /// <seealso href="https://swagger.io/docs/specification/data-models/data-types"/>
    /// </summary>
    public string JsType => jsType;

    /// <summary>
    /// The Format in OpenApi
    /// <seealso href="https://swagger.io/docs/specification/data-models/data-types"/>
    /// </summary>
    public string? JsFormat => jsFormat;

    /// <summary>
    /// A number indicating how many bytes this uses when used on MS SQL Server. 2 for nvarchar/ntext/nchar etc
    /// </summary>
    /// <value></value>
    public byte BytesPerChar { get; } = 1;


    static SqlType()
    {

        RegisterSqlType<System.Array>(System.Data.DbType.Object, "table type", "array", null);
        RegisterSqlType<string>(System.Data.DbType.String, "varchar", "string", bytesPerChar: 1);
        RegisterSqlType<string>(System.Data.DbType.String, "nvarchar", "string", bytesPerChar: 2);
        RegisterSqlType<string>(System.Data.DbType.String, "sysname", "string", bytesPerChar: 2);
        RegisterSqlType<string>(System.Data.DbType.StringFixedLength, "nchar", "string", bytesPerChar: 2);
        RegisterSqlType<string>(System.Data.DbType.StringFixedLength, "char", "string", bytesPerChar: 1);
        RegisterSqlType<string>(System.Data.DbType.String, "text", "string", bytesPerChar: 1);
        RegisterSqlType<string>(System.Data.DbType.String, "ntext", "string", bytesPerChar: 2);
        RegisterSqlType<string>(System.Data.DbType.String, "longtext", "string", bytesPerChar: 1);
        RegisterSqlType<string>(System.Data.DbType.String, "tinytext", "string", bytesPerChar: 1);
        RegisterSqlType<string>(System.Data.DbType.String, "mediumtext", "string", bytesPerChar: 1);
        RegisterSqlType<Guid>(System.Data.DbType.Guid, "uniqueidentifier", "string", "uuid");
        RegisterSqlType<System.DateTime>(System.Data.DbType.Date, "date", "string", "date");
        RegisterSqlType<System.DateTime>(System.Data.DbType.Time, "time", "string", "time");
        RegisterSqlType<System.DateTime>(System.Data.DbType.DateTime, "datetime", "string", "date-time");
        RegisterSqlType<System.DateTime>(System.Data.DbType.DateTime2, "datetime2", "string", "date-time");
        RegisterSqlType<System.DateTime>(System.Data.DbType.DateTime, "smalldatetime", "string", "date-time");
        RegisterSqlType<System.DateTimeOffset>(System.Data.DbType.DateTimeOffset, "datetimeoffset", "string", "date-time");
        RegisterSqlType<int>(System.Data.DbType.Int32, "int", "integer", "int32");
        RegisterSqlType<int>(System.Data.DbType.Int32, "mediumint", "integer", "int32");//MySql Type. Nobody uses this :)
        RegisterSqlType<long>(System.Data.DbType.Int64, "bigint", "integer", "int64");
        RegisterSqlType<short>(System.Data.DbType.Int16, "smallint", "integer");
        RegisterSqlType<byte>(System.Data.DbType.Byte, "tinyint", "integer");
        RegisterSqlType<double>(System.Data.DbType.Double, "float", "number", "double");//MySql actually treats float as float, but MSSQL treats float as double
        RegisterSqlType<float>(System.Data.DbType.Single, "real", "number", "float");
        RegisterSqlType<double>(System.Data.DbType.Double, "double", "number", "double");
        RegisterSqlType<decimal>(System.Data.DbType.Decimal, "numeric", "number");
        RegisterSqlType<decimal>(System.Data.DbType.Currency, "money", "number");
        RegisterSqlType<decimal>(System.Data.DbType.Currency, "smallmoney", "number");
        RegisterSqlType<decimal>(System.Data.DbType.Decimal, "decimal", "number");
        RegisterSqlType<bool>(System.Data.DbType.Boolean, "bit", "boolean");
        RegisterSqlType<byte[]>(System.Data.DbType.Binary, "varbinary", "string", "binary");
        RegisterSqlType<byte[]>(System.Data.DbType.Binary, "tinyblob", "string", "binary");//MySql
        RegisterSqlType<byte[]>(System.Data.DbType.Binary, "longblob", "string", "binary");//MySql
        RegisterSqlType<byte[]>(System.Data.DbType.Binary, "mediumblob", "string", "binary");//MySql
        RegisterSqlType<byte[]>(System.Data.DbType.Binary, "blob", "string", "binary");//MySql
        RegisterSqlType<byte[]>(System.Data.DbType.Binary, "binary", "string", "binary");
        RegisterSqlType<byte[]>(System.Data.DbType.Binary, "image", "string", "binary");
        RegisterSqlType<byte[]>(System.Data.DbType.Binary, "timestamp", "string", "binary");
        RegisterSqlType<byte[]>(System.Data.DbType.Binary, "rowversion", "string", "binary");
        RegisterSqlType<string>(System.Data.DbType.Xml, "xml", "object", null);
        RegisterSqlType<object>(System.Data.DbType.Object, "geography", "object", null);
        RegisterSqlType<object>(System.Data.DbType.Object, "hierarchyid", "object", null);
        RegisterSqlType<object>(System.Data.DbType.Object, "geometry", "object", null);
        RegisterSqlType<object>(System.Data.DbType.Object, "sql_variant", "object", null);
        // There is actually no json type in SQL Server,
        // we use it to model Json parameters
        RegisterSqlType<string>(System.Data.DbType.String, "json", "object", null);

    }

    private SqlType(System.Data.DbType dataDbType, string dbType, Type type, string jsType, string? jsFormat, byte bytesPerChar = 1)
    {
        this.dbType = dbType;
        this.netType = type;
        this.jsType = jsType;
        this.jsFormat = jsFormat;
        this.BytesPerChar = bytesPerChar;
        this.DataDbType = dataDbType;
    }

    /// <summary>
    /// Use this method to register a custom Sql Type
    /// Do call this method as early as possible in your code
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="dbType"></param>
    /// <param name="jsType"></param>
    /// <param name="jsFormat"></param>
    /// <param name="bytesPerChar">2 for nvarchar/ntext etc</param>
    /// <returns></returns>
    public static SqlType RegisterSqlType<T>(System.Data.DbType dataDbType, string dbType, string jsType, string? jsFormat = null, byte bytesPerChar = 1)
    {
        var st = new SqlType(dataDbType, dbType, typeof(T), jsType, jsFormat, bytesPerChar);
        allTypes.Add(st);
        return st;
    }


    /// <summary>
    /// Get the Type for the given db Type
    /// </summary>
    /// <param name="dbType"></param>
    /// <returns></returns>
    public static SqlType GetSqlType(string dbType)
    {
        if (dbType.Contains("("))
            return GetSqlType(dbType.Substring(0, dbType.IndexOf("(")));
        var type = allTypes.FirstOrDefault(f => f.DbType.Equals(dbType, StringComparison.CurrentCultureIgnoreCase));
        return type ?? GetSqlType("nvarchar");
    }

    /// <summary>
    /// Gets any mapped that that conforms to the C# type provided
    /// </summary>
    /// <param name="netType"></param>
    /// <returns></returns>
    public static SqlType GetSomeSqlType(Type netType)
    {
        var type = allTypes.FirstOrDefault(f => f.NetType == netType);
        return type ?? GetSqlType("nvarchar");
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return DbType.ToString();
    }


    /// <inheritdoc />
    public override int GetHashCode()
    {
        return DbType.GetHashCode();
    }


    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj is SqlType sqlType)
        {
            return Equals(sqlType);
        }
        return false;
    }


    /// <inheritdoc />
    public bool Equals(SqlType? other)
    {
        return other != null && other.DbType == this.DbType;
    }


}

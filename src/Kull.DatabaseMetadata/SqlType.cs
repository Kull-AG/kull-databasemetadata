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

        RegisterSqlType<System.Array>("table type", "array", null);
        RegisterSqlType<string>("varchar", "string", bytesPerChar: 1);
        RegisterSqlType<string>("nvarchar", "string", bytesPerChar: 2);
        RegisterSqlType<string>("sysname", "string", bytesPerChar: 2);
        RegisterSqlType<string>("nchar", "string", bytesPerChar: 2);
        RegisterSqlType<string>("char", "string", bytesPerChar: 1);
        RegisterSqlType<string>("text", "string", bytesPerChar: 1);
        RegisterSqlType<string>("ntext", "string", bytesPerChar: 2);
        RegisterSqlType<string>("longtext", "string", bytesPerChar: 1);
        RegisterSqlType<string>("tinytext", "string", bytesPerChar: 1);
        RegisterSqlType<string>("mediumtext", "string", bytesPerChar: 1);
        RegisterSqlType<Guid>("uniqueidentifier", "string", "uuid");
        RegisterSqlType<System.DateTime>("date", "string", "date");
        RegisterSqlType<System.DateTime>("time", "string", "time");
        RegisterSqlType<System.DateTime>("datetime", "string", "date-time");
        RegisterSqlType<System.DateTime>("datetime2", "string", "date-time");
        RegisterSqlType<System.DateTime>("smalldatetime", "string", "date-time");
        RegisterSqlType<System.DateTimeOffset>("datetimeoffset", "string", "date-time");
        RegisterSqlType<int>("int", "integer", "int32");
        RegisterSqlType<int>("mediumint", "integer", "int32");//MySql Type. Nobody uses this :)
        RegisterSqlType<long>("bigint", "integer", "int64");
        RegisterSqlType<short>("smallint", "integer");
        RegisterSqlType<byte>("tinyint", "integer");
        RegisterSqlType<double>("float", "number", "double");//MySql actually treats float as float, but MSSQL treats float as double
        RegisterSqlType<float>("real", "number", "float");
        RegisterSqlType<double>("double", "number", "double");
        RegisterSqlType<decimal>("numeric", "number");
        RegisterSqlType<decimal>("money", "number");
        RegisterSqlType<decimal>("smallmoney", "number");
        RegisterSqlType<decimal>("decimal", "number");
        RegisterSqlType<bool>("bit", "boolean");
        RegisterSqlType<byte[]>("varbinary", "string", "binary");
        RegisterSqlType<byte[]>("tinyblob", "string", "binary");//MySql
        RegisterSqlType<byte[]>("longblob", "string", "binary");//MySql
        RegisterSqlType<byte[]>("mediumblob", "string", "binary");//MySql
        RegisterSqlType<byte[]>("blob", "string", "binary");//MySql
        RegisterSqlType<byte[]>("binary", "string", "binary");
        RegisterSqlType<byte[]>("image", "string", "binary");
        RegisterSqlType<byte[]>("timestamp", "string", "binary");
        RegisterSqlType<byte[]>("rowversion", "string", "binary");
        RegisterSqlType<string>("xml", "object", null);
        RegisterSqlType<object>("geography", "object", null);
        RegisterSqlType<object>("hierarchyid", "object", null);
        RegisterSqlType<object>("geometry", "object", null);
        RegisterSqlType<object>("sql_variant", "object", null);
        // There is actually no json type in SQL Server,
        // we use it to model Json parameters
        RegisterSqlType<string>("json", "object", null);

    }

    private SqlType(string dbType, Type type, string jsType, string? jsFormat, byte bytesPerChar = 1)
    {
        this.dbType = dbType;
        this.netType = type;
        this.jsType = jsType;
        this.jsFormat = jsFormat;
        this.BytesPerChar = bytesPerChar;
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
    public static SqlType RegisterSqlType<T>(string dbType, string jsType, string? jsFormat = null, byte bytesPerChar = 1)
    {
        var st = new SqlType(dbType, typeof(T), jsType, jsFormat, bytesPerChar);
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

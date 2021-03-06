﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kull.DatabaseMetadata
{
    /// <summary>
    /// A class representing a type of Sql server
    /// Does not include the type
    /// </summary>
    public sealed class SqlType: IEquatable<SqlType>
    {
        private readonly string dbType;
        private readonly System.Type netType;
        private readonly string jsType;
        private readonly string? jsFormat;

        private static List<SqlType>
            allTypes = new List<SqlType>(30);

        public string DbType => dbType;

        public Type NetType => netType;

        public string JsType => jsType;

        public string? JsFormat => jsFormat;

        static SqlType()
        {

            RegisterSqlType<System.Array>("table type", "array", null);
            RegisterSqlType<string>("varchar", "string");
            RegisterSqlType<string>("nvarchar", "string");
            RegisterSqlType<string>("nchar", "string");
            RegisterSqlType<string>("char", "string");
            RegisterSqlType<string>("text", "string");
            RegisterSqlType<Guid>("uniqueidentifier", "string", "uuid");
            RegisterSqlType<System.DateTime>("date", "string", "date");
            RegisterSqlType<System.DateTime>("time", "string", "time");
            RegisterSqlType<System.DateTime>("datetime", "string", "date-time");
            RegisterSqlType<System.DateTime>("datetime2", "string", "date-time");
            RegisterSqlType<System.DateTime>("smalldatetime", "string", "date-time");
            RegisterSqlType<System.DateTimeOffset>("datetimeoffset", "string", "date-time");
            RegisterSqlType<int>("int", "integer");
            RegisterSqlType<long>("bigint", "integer");
            RegisterSqlType<short>("smallint", "integer");
            RegisterSqlType<byte>("tinyint", "integer");
            RegisterSqlType<double>("float", "number");
            RegisterSqlType<float>("real", "number");
            RegisterSqlType<double>("double", "number");
            RegisterSqlType<decimal>("numeric", "number");
            RegisterSqlType<decimal>("money", "number");
            RegisterSqlType<decimal>("smallmoney", "number");
            RegisterSqlType<decimal>("decimal", "number");
            RegisterSqlType<bool>("bit", "boolean");
            RegisterSqlType<byte[]>("varbinary", "string", "binary");
            RegisterSqlType<byte[]>("binary", "string", "binary");
            RegisterSqlType<byte[]>("timestamp", "string", "binary");
            RegisterSqlType<byte[]>("rowversion", "string", "binary");
            RegisterSqlType<string>("xml", "object", null);

            // There is actually no json type in SQL Server,
            // we use it to model Json parameters
            RegisterSqlType<string>("json", "object", null);
        }

        private SqlType(string dbType, Type type, string jsType, string? jsFormat)
        {
            this.dbType = dbType;
            this.netType = type;
            this.jsType = jsType;
            this.jsFormat = jsFormat;
        }

        /// <summary>
        /// Use this method to register a custom Sql Type
        /// Do call this method as early as possible in your code
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dbType"></param>
        /// <param name="jsType"></param>
        /// <param name="jsFormat"></param>
        /// <returns></returns>
        public static SqlType RegisterSqlType<T>(string dbType, string jsType, string? jsFormat = null)
        {
            var st = new SqlType(dbType, typeof(T), jsType, jsFormat);
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

        public override string ToString()
        {
            return DbType.ToString();
        }
        public override int GetHashCode()
        {
            return DbType.GetHashCode();
        }
        public override bool Equals(object? obj)
        {
            if(obj is SqlType sqlType)
            {
                return Equals(sqlType);
            }
            return false;
        }

        public bool Equals(SqlType? other)
        {
            return other != null && other.DbType == this.DbType;
        }

        
    }
}

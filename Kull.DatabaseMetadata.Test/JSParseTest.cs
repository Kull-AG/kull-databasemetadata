using Microsoft.Data.Sqlite;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kull.DatabaseMetadata.Test
{
    [TestClass]
    public class JSParseTest
    {
        [TestMethod]
    
        public void Test()
        {

            var json = System.IO.File.ReadAllText("dbo.spSearchPets.json");
            var resJS = DeserializeJson<List<Dictionary<string, object>>>(json);
            var res = resJS.Select(s => SqlFieldDescription.FromJson((IReadOnlyDictionary<string, object?>)s)).ToArray();
            var ar = res.Cast<SqlFieldDescription>().ToArray();
            Assert.AreEqual(4, ar.Length);
            Assert.AreEqual("PetId", ar[0].Name);
            Assert.AreEqual(SqlType.GetSqlType("int"), ar[0].DbType);
        }
        private static T DeserializeJson<T>(string json)
        {
#if NET48 || NETSTANDARD1_0_OR_GREATER
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json)!;
#else
            return System.Text.Json.JsonSerializer.Deserialize<T>(json)!;
#endif
        }
    }
}

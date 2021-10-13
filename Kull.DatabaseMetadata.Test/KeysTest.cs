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
    public class KeysTest
    {
        [TestMethod]
        public async Task TestGetPrimaryKeys()
        {
            using (var db = new SqliteConnection("Data Source=:memory:"))
            {
                db.Open();
                var sql = System.IO.File.ReadAllText("sample.sql");
                var sqls = sql.Split("GO", System.StringSplitOptions.RemoveEmptyEntries);
                foreach (var s in sqls)
                {
                    var cmd = db.CreateCommand();
                    cmd.CommandText = s;
                    cmd.ExecuteNonQuery();
                }
                Kull.DatabaseMetadata.Keys sqlHelper = new Keys();
                var keys = (await sqlHelper.GetPrimaryKeyColumns(db)).OrderBy(s=>s.Table.Name).ThenBy(s=>s.ColumnName).ToArray();
                Assert.AreEqual(2, keys.Length);
                Assert.AreEqual("customer", keys[0].Table);
                Assert.AreEqual("employeeId", keys[1].ColumnName);

            }
        }
    }
}

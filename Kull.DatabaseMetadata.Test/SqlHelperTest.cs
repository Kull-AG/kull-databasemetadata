using Microsoft.Data.Sqlite;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System.Linq;

namespace Kull.DatabaseMetadata.Test
{
    [TestClass]
    public class SqlHelperTest
    {
        public TestContext? TestContext { get; set; }

        [TestMethod]
        public async Task TestGetTableOrViewFields()
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
                var logger = new TestLogger<SqlHelper>(TestContext!);
                Kull.DatabaseMetadata.SqlHelper sqlHelper = new SqlHelper(logger);
                var fields = (await sqlHelper.GetTableOrViewFields(db, "employee")).ToArray();
                Assert.AreEqual(2, fields.Length);
                Assert.AreEqual("employeeId", fields[0].Name);
                Assert.AreEqual("employeeName", fields[1].Name);
            }
        }


    }
}

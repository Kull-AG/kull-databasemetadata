using Microsoft.Data.Sqlite;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System.Linq;

namespace Kull.DatabaseMetadata.Test;

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

    [TestMethod]
    public async Task TestSQLProcedureNaming()
    {
        using (var db = new Microsoft.Data.SqlClient.SqlConnection("Server=127.0.0.1;user id=sa;password=abcDEF123#;MultipleActiveResultSets=true;TrustServerCertificate=True;"))
        {
            db.Open();
            var sql = System.IO.File.ReadAllText("sql_server/sqlscript.sql");
            var sqls = sql.Split("GO", System.StringSplitOptions.RemoveEmptyEntries);
            foreach (var s in sqls)
            {
                var cmd = db.CreateCommand();
                cmd.CommandText = s;
                cmd.ExecuteNonQuery();
            }
            var logger = new TestLogger<SqlHelper>(TestContext!);
            Kull.DatabaseMetadata.SqlHelper sqlHelper = new SqlHelper(logger);
            var(_, fields) = (await sqlHelper.GetSPResultSet2(db, new Data.DBObjectName("Sales.DataDelivery", "spReturnDataDelivery"),null,null));
            
            Assert.AreEqual(2, fields.Count);
            Assert.AreEqual("PetId", fields.ElementAt(0).Name);
            Assert.AreEqual("PetName", fields.ElementAt(1).Name);

            var (_, fields_dbo) = (await sqlHelper.GetSPResultSet2(db, new Data.DBObjectName("dbo", "spGetName"), null, null));

            Assert.AreEqual(2, fields_dbo.Count);
            Assert.AreEqual("PetId", fields_dbo.ElementAt(0).Name);
            Assert.AreEqual("PetName", fields_dbo.ElementAt(1).Name);

            var (_, fields_null) = (await sqlHelper.GetSPResultSet2(db, new Data.DBObjectName(null,"spGetName"), null, null));

            Assert.AreEqual(2, fields_null.Count);
            Assert.AreEqual("PetId", fields_null.ElementAt(0).Name);
            Assert.AreEqual("PetName", fields_null.ElementAt(1).Name);

            var (_, fields_noschema) = (await sqlHelper.GetSPResultSet2(db, new Data.DBObjectName("dbo", "spNoSchema"), null, null));

            Assert.AreEqual(2, fields_noschema.Count);
            Assert.AreEqual("PetId", fields_noschema.ElementAt(0).Name);
            Assert.AreEqual("PetName", fields_noschema.ElementAt(1).Name);
        }
    }


}

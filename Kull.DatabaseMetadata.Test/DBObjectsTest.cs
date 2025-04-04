using Microsoft.Data.Sqlite;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kull.DatabaseMetadata.Test;

[TestClass]
public class DBObjectsTest
{
    public TestContext? TestContext { get; set; }

    [TestMethod]
    public async Task TestGetTableAndViews()
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
            Kull.DatabaseMetadata.DBObjects sqlHelper = new DBObjects();
            var tablesAndViews = (await sqlHelper.GetTablesAndViews(db)).OrderBy(s => s.Name.Name).ToArray();
            Assert.AreEqual(3, tablesAndViews.Length);
            Assert.AreEqual("customer", tablesAndViews[0].Name);
            Assert.AreEqual(DBObjects.TableOrViewType.Table, tablesAndViews[0].Type);
            Assert.AreEqual("employee", tablesAndViews[1].Name);
            Assert.AreEqual("V_Customers", tablesAndViews[2].Name);
            Assert.AreEqual(DBObjects.TableOrViewType.View, tablesAndViews[2].Type);


            var tablesOnly = (await sqlHelper.GetTablesAndViews(db, DBObjects.TableOrViewType.Table));
            Assert.AreEqual(2, tablesOnly.Count);
        }
    }

    [TestMethod]
    public async Task TestGetTableAndViewsDuck()
    {
        using (var db = new DuckDB.NET.Data.DuckDBConnection("Data Source=:memory:"))
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
            Kull.DatabaseMetadata.DBObjects dbo = new DBObjects();
            Kull.DatabaseMetadata.SqlHelper sqlHelper = new SqlHelper(logger);
            var tablesAndViews = (await dbo.GetTablesAndViews(db)).OrderBy(s => s.Name.Name).ToArray();
            Assert.AreEqual(3, tablesAndViews.Length);
            Assert.AreEqual("customer", tablesAndViews[0].Name.Name);
            Assert.AreEqual(DBObjects.TableOrViewType.Table, tablesAndViews[0].Type);
            Assert.AreEqual("employee", tablesAndViews[1].Name.Name);
            Assert.AreEqual("V_Customers", tablesAndViews[2].Name.Name);
            Assert.AreEqual(DBObjects.TableOrViewType.View, tablesAndViews[2].Type);

            var fields = (await sqlHelper.GetTableOrViewFields(db, tablesAndViews[0].Name)).ToList();
            var names = fields.Select(s => s.Name).OrderBy(s => s).ToArray();
            Assert.AreEqual(3, fields.Count);
            Assert.AreEqual(names[0], "customerId");
            Assert.AreEqual(names[1], "customerName");
            Assert.AreEqual(names[2], "employeeId");

            var tablesOnly = (await dbo.GetTablesAndViews(db, DBObjects.TableOrViewType.Table));
            Assert.AreEqual(2, tablesOnly.Count);
        }
    }



    [TestMethod]
    public async Task TestGetTables()
    {
        using (var db = new Microsoft.Data.SqlClient.SqlConnection("Data Source=cn413.kull.ch;Database=BMS_App_MYPage;Integrated Security=True;TrustServerCertificate=True;"))
        {
            db.Open();
            await new DBObjects().GetTables(db, null);
        }
    }
}

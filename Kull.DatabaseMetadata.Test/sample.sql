CREATE TABLE "employee"("employeeId" INT PRIMARY KEY,
	"employeeName" TEXT)
GO
CREATE TABLE "customer"("customerId" INT PRIMARY KEY,
	"employeeId" INT,
	"customerName" TEXT)
GO
CREATE VIEW "V_Customers" AS SELECT *FROM "customer" c
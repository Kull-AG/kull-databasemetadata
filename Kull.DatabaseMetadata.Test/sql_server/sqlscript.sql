IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'CodeGenTestDb')
BEGIN
    CREATE DATABASE CodeGenTestDb;
END
GO
USE CodeGenTestDb
GO
IF OBJECT_ID('dbo.Pets', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Pets (
        PetId int PRIMARY KEY IDENTITY(1,1), 
        PetName varchar(100) NOT NULL, 
        PetHeight decimal, 
        IsNice bit, 
        ts timestamp
    );
END
GO
TRUNCATE TABLE dbo.Pets
GO
INSERT INTO dbo.Pets(PetName, PetHeight, IsNice)
SELECT 'Dog', 12.2445, 0
UNION ALL 
SELECT 'Dog 2', 0.2345, 1
GO


-- Testing with specific schema naming
-- explicit with "." in the name

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Sales.DataDelivery')
BEGIN
    EXEC('CREATE SCHEMA [Sales.DataDelivery]');
END
GO
DROP PROCEDURE IF EXISTS [Sales.DataDelivery].spReturnDataDelivery
GO
CREATE PROCEDURE [Sales.DataDelivery].spReturnDataDelivery
AS
BEGIN
		SELECT PetId, PetName
		--geography::STGeomFromText('POINT(0 0)',4326) AS PetPosition Not Supported on .Net Core
		-- see https://github.com/dotnet/SqlClient/issues/30
	FROM dbo.Pets
		WHERE IsNice=1
		ORDER BY PetId;
END
GO
DROP PROCEDURE IF EXISTS [dbo].spGetName
GO
CREATE PROCEDURE [dbo].spGetName
AS
BEGIN
		SELECT PetId, PetName
		--geography::STGeomFromText('POINT(0 0)',4326) AS PetPosition Not Supported on .Net Core
		-- see https://github.com/dotnet/SqlClient/issues/30
	FROM dbo.Pets
		WHERE IsNice=1
		ORDER BY PetId;
END
GO
DROP PROCEDURE IF EXISTS spNoSchema
GO
CREATE PROCEDURE spNoSchema
AS
BEGIN
		SELECT PetId, PetName
		--geography::STGeomFromText('POINT(0 0)',4326) AS PetPosition Not Supported on .Net Core
		-- see https://github.com/dotnet/SqlClient/issues/30
	FROM dbo.Pets
		WHERE IsNice=1
		ORDER BY PetId;
END
GO

# Database Metadata Library

A library that exposes useful SQL Server Features like sp_describe_first_result set and the like.

It currently a number of Services
- SqlHelper: Allows for querying fields of a user defined table value type (GetTableTypeFields) and for querying the expected result type of a procedure/view/table(GetResultSet2)
- SPParametersProvider: Allows to query parameters of a procedure
- DBObjects: Allows to query tables and views in a database
- Keys: Get Primary Keys of a Table

Currently, the Code only works with Microsoft SQL Server and partially for SqLite and MySql, everything else is untested. 
The SQL should conform as much as possible to Standard SQL which should make porting it to other Database Systems easy. Therefore, we use Information_Schema Tables whenever possible.

## Testing with MSSQL
This project uses `Testcontainers` to facilitate testing with MSSQL.
### Prerequisites

To run the tests, you need to have [Docker](https://www.docker.com/) installed and running on your system.

### Running Tests

The tests can be executed directly using the configured `Testcontainers` setup.

Alternatively, if you prefer to run the MSSQL server manually or want to inspect the database, you can use the provided PowerShell script:

```powershell
sql_server/start_container.ps1
```
This script starts a local MSSQL container with the complete database setup defined in the same folder.

### Modifying the SQL Script
If you need to add database objects to the sqlscript.sql file, please do so with care. Ensure that any new objects are created conditionally — check if they already exist, and drop them beforehand if necessary. This helps avoid conflicts.

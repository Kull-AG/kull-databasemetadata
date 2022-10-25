# Database Metadata Library

A library that exposes useful SQL Server Features like sp_describe_first_result set and the like.

It currently a number of Services
- SqlHelper: Allows for querying fields of a user defined table value type (GetTableTypeFields) and for querying the expected result type of a procedure/view/table(GetResultSet2)
- SPParametersProvider: Allows to query parameters of a procedure
- DBObjects: Allows to query tables and views in a database
- Keys: Get Primary Keys of a Table

Currently, the Code only works with Microsoft SQL Server and partially for SqLite and MySql, everything else is untested. 

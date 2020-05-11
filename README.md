# Database Metadata Library

A library that exposes useful SQL Server Features like sp_describe_first_result set and the like.

It currently exposes two Services
- SqlHelper: Allows for querying fields of a user defined table value type (GetTableTypeFields) and for querying the expected result type of a procedure (GetSPResultSet)
- SPParametersProvider: Allows to query parameters of a procedure

Currently, the Code only works with Microsoft SQL Server, everything else is untested. 

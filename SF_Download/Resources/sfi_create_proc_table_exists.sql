create procedure [sfi].[sfi_table_exists]

	@schema_name varchar(50)
,	@table_name varchar(255)

as

if OBJECT_ID(@schema_name + '.' + @table_name, 'U') is null return 0 else return 1

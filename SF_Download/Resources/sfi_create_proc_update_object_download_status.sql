

/*

SF Download – Integrating Salesforce Downloads to SQL Server

Copyright (C) 2021 Kevin Chadney


This program is free software: you can redistribute it and/or modify

it under the terms of the GNU General Public License as published by

the Free Software Foundation, either version 3 of the License, or

(at your option) any later version.


This program is distributed in the hope that it will be useful,

but WITHOUT ANY WARRANTY; without even the implied warranty of

MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the

GNU General Public License for more details.


You should have received a copy of the GNU General Public License

along with this program.  If not, see <https://www.gnu.org/licenses/>.

*/

CREATE procedure [sfi].[sfi_update_object_download_status]

	@log_task_id int
,	@object_name varchar(255)
,	@cant_query bit = null
,	@cant_query_error varchar(8000) = null
,	@is_empty bit = null
,	@download bit = null
,	@download_method varchar(50) = null
,	@integration_method varchar(50) = null
,	@bulk_query_batch_size int = null


as


declare @change_type_id int = (select change_type_id from sfi.sfi_lookup_change_types where name = 'Update Object Download Status')
declare @download_method_id int = (select download_method_id from sfi.sfi_lookup_download_methods where download_method = @download_method)
declare @integration_method_id int = (select integration_method_id from sfi.sfi_lookup_integration_methods where integration_method = @integration_method)




insert into sfm.sfm_objects_history

select	@log_task_id
,		@change_type_id
,		key_prefix
,		@object_name
,		label
,		is_custom
,		is_custom_setting
,		is_queryable
,		isnull(@cant_query, cant_query)
,		isnull(@cant_query_error, cant_query_error)
,		is_deleted
,		isnull(@is_empty, is_empty)
,		isnull(@download, download)
,		isnull(@download_method_id, download_method)
,		isnull(@integration_method_id, integration_method)
,		isnull(@bulk_query_batch_size, bulk_query_batch_size)
from	sfm.sfm_objects
where	name = @object_name


update	sfm.sfm_objects
set		cant_query = isnull(@cant_query, cant_query)
,		cant_query_error = isnull(@cant_query_error, cant_query_error) 
,		is_empty = isnull(@is_empty, is_empty) 
,		download = isnull(@download, download) 
,		download_method = isnull(@download_method_id, download_method)
,		integration_method = isnull(@integration_method_id, integration_method)
,		bulk_query_batch_size = isnull(@bulk_query_batch_size, bulk_query_batch_size)
where	name = @object_name





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

create procedure [sfi].[sfi_select_object_settings]

	@object_name varchar(255)

as

select	o.object_id	
,		o.name object_name
,		o.cant_query
,		o.cant_query_error
,		o.is_deleted
,		o.is_empty
,		o.download
,		dm.download_method
,		im.integration_method
,		o.is_deleted
from	sfm.sfm_objects o
		join sfi.sfi_lookup_download_methods dm
			on o.download_method = dm.download_method_id
		join sfi.sfi_lookup_integration_methods im
			on o.integration_method = im.integration_method_id
where	o.name = @object_name

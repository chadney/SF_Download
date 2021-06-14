
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

CREATE procedure [sfi].[sfi_select_downloadable_objects]
as

select	o.object_id
,		o.name
,		o.is_queryable
,		o.cant_query
,		o.is_deleted
,		o.is_empty
,		o.download
,		dm.download_method
,		im.integration_method
,		o.bulk_query_batch_size
,		old.last_download_on
from	sfm.sfm_objects o
		join sfi.sfi_lookup_download_methods dm
			on o.download_method = dm.download_method_id
		join sfi.sfi_lookup_integration_methods im
			on o.integration_method = im.integration_method_id
		left join sfi.sfi_object_last_download_v old
			on o.name = old.object_name
			--TODO
			--on o.object_id = old.object_id
where	o.is_queryable = 1
and		o.cant_query = 0
and		o.is_empty = 0
and		o.download = 1

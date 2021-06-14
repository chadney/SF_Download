
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

create procedure [sfi].[sfi_update_field_download_status]

	@log_task_id int
,	@object_name varchar(255)
,	@field_name varchar(255)
,	@download bit

as

declare @change_type_id int = (select change_type_id from sfi.sfi_lookup_change_types where name = 'Update Field Download Status')

insert into sfm.sfm_fields_history
select	@log_task_id
,		@change_type_id
,		o.name	
,		f.name	
,		f.label	
,		f.soap_type	
,		f.type	
,		f.length	
,		f.byte_length	
,		f.digits	
,		f.precision	
,		f.scale	
,		f.is_autonumber	
,		f.is_calculated	
,		f.is_case_sensitive	
,		f.is_custom	
,		f.is_dependent_pick_list	
,		f.is_external_id	
,		f.is_ID_lookup	
,		f.is_name_field	
,		f.is_unique	
,		f.is_restricted_picklist	
,		f.is_deleted	
,		@download
from	sfm.sfm_fields f
		join sfm.sfm_objects o
			on f.object_id = o.object_id
where	o.name = @object_name
and		f.name = @field_name


update	f
set		f.download = @download
from	sfm.sfm_fields f
		join sfm.sfm_objects o
			on f.object_id = o.object_id
where	o.name = @object_name
and		f.name = @field_name







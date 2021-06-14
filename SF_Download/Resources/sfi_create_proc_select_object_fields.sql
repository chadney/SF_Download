

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

create procedure [sfi].[sfi_select_object_fields]

	@object_name varchar(255)
,	@download bit = null

as

select	f.field_id	
,		f.object_id	
,		o.name object_name
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
,		f.download
from	sfm.sfm_objects o
		join sfm.sfm_fields f
			on o.object_id = f.object_id
where	(o.name = @object_name and f.download = @download)
or		(o.name = @object_name and @download is null)




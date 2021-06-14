
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


create procedure [sfi].[sfi_select_changed_datatype_fields]

	@log_execution_id int

as

select	* 
from	
		(
		select	le.log_execution_id
		,		ct.name change_type
		,		fh.object_name	
		,		fh.name	
		,		fh.label	
		,		fh.soap_type	
		,		fh.byte_length	
		,		fh.digits	
		,		fh.is_deleted	
		,		fh.download
		,		fh.type	
		,		fh.length	
		,		fh.precision	
		,		fh.scale
		,		lag(fh.type) over (partition by fh.object_name, fh.name order by le.log_execution_id asc) type_previous
		,		lag(fh.length) over (partition by fh.object_name, fh.name order by le.log_execution_id asc) length_previous
		,		lag(fh.precision) over (partition by fh.object_name, fh.name order by le.log_execution_id asc) precision_previous
		,		lag(fh.scale) over (partition by fh.object_name, fh.name order by le.log_execution_id asc) scale_previous
		from	sfm.sfm_fields_history fh
				join sfi.sfi_lookup_change_types ct
					on fh.change_type_id = ct.change_type_id
				join sfi.sfi_log_task lt
					on fh.log_task_id = lt.log_task_id
				join sfi.sfi_log_execution le
					on lt.log_execution_id = le.log_execution_id
		) sub 
where	sub.change_type = 'Change Field Datatype'
and		sub.log_execution_id = @log_execution_id




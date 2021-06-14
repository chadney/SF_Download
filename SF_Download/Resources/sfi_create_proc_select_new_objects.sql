
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

create procedure [sfi].[sfi_select_new_objects]

	@log_execution_id as int

as

select	*
from	sfm.sfm_objects_history h
		join sfi.sfi_lookup_change_types ct
			on h.change_type_id = ct.change_type_id
		join sfi.sfi_log_task lt
			on h.log_task_id = lt.log_task_id
where	ct.name = 'New Object'
and		lt.log_execution_id = @log_execution_id
and		h.is_queryable = 1


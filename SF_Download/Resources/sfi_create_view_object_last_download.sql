
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

create view sfi.sfi_object_last_download_v
as
select	t.object_id
,		t.object_name
,		max(t.started_on) last_download_on
from	sfi.sfi_log_execution e
		join sfi.sfi_log_task t
			on e.log_execution_id = t.log_execution_id
		join sfi.sfi_lookup_tasks lt
			on t.task_id = lt.task_id
where	e.ran_to_completion = 1
and		e.action_code = 'Download'
and		lt.task = 'Download Object Data'
group by t.object_id
,		t.object_name

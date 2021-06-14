

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

CREATE view [sfm].[sfm_fields_history_v]
as
select	le.log_execution_id
,		le.action_code
,		le.started_on execution_started_on
,		lt.started_on task_started_on
,		lt.task_id
,		lkt.task task_name
,		ct.name change_type
,		fh.object_name
,		fh.name field_name
,		fh.label
,		fh.soap_type
,		fh.type
,		fh.length
,		fh.byte_length
,		fh.digits
,		fh.precision
,		fh.scale
,		fh.is_autonumber
,		fh.is_calculated
,		fh.is_case_sensitive
,		fh.is_custom
,		fh.is_dependent_pick_list
,		fh.is_external_id
,		fh.is_ID_lookup
,		fh.is_name_field
,		fh.is_unique
,		fh.is_restricted_picklist
,		fh.is_deleted
,		fh.download
from	sfm.sfm_fields_history fh
		join sfi.sfi_lookup_change_types ct
			on fh.change_type_id = ct.change_type_id
		join sfi.sfi_log_task lt
			on fh.log_task_id = lt.log_task_id
		join sfi.sfi_lookup_tasks lkt
			on lt.task_id = lkt.task_id
		join sfi.sfi_log_execution le
			on lt.log_execution_id = le.log_execution_id
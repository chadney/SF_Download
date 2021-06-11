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
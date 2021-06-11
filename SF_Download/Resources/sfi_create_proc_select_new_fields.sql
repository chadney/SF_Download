create procedure sfi.sfi_select_new_fields

	@log_execution_id int

as

select	fh.object_name	
,		fh.name	
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
		join sfi.sfi_log_execution le
			on lt.log_execution_id = le.log_execution_id
where	ct.name = 'New Field'
and		le.log_execution_id = @log_execution_id



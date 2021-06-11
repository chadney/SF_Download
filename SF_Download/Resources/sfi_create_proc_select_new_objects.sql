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


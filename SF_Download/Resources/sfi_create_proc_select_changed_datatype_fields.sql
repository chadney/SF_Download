



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




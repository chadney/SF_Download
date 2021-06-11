
create view [sfm].[sfm_objects_history_v]
as
select	le.log_execution_id
,		le.action_code
,		le.started_on execution_started_on
,		lt.started_on task_started_on
,		lt.task_id
,		lkt.task task_name
,		ct.name change_type
,		oh.key_prefix
,		oh.name object_name
,		oh.label
,		oh.is_custom
,		oh.is_custom_setting
,		oh.is_queryable
,		oh.cant_query
,		oh.cant_query_error
,		oh.is_deleted
,		oh.download
,		oh.bulk_query_batch_size
,		dm.download_method
,		im.integration_method
from	sfm.sfm_objects_history oh
		join sfi.sfi_lookup_change_types ct
			on oh.change_type_id = ct.change_type_id
		join sfi.sfi_log_task lt
			on oh.log_task_id = lt.log_task_id
		join sfi.sfi_lookup_tasks lkt
			on lt.task_id = lkt.task_id
		join sfi.sfi_log_execution le
			on lt.log_execution_id = le.log_execution_id
		left join sfi.sfi_lookup_download_methods dm
			on oh.download_method = dm.download_method_id
		left join sfi.sfi_lookup_integration_methods im
			on oh.integration_method = im.integration_method_id

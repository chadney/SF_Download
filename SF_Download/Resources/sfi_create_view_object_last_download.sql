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

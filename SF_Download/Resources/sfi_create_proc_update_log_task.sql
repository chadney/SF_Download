
CREATE PROCEDURE sfi.sfi_update_log_task
 
	@download_method varchar(50)
,	@integration_method varchar(50)
,	@sf_job_id varchar(50)
,	@sf_batch_id varchar(50)
,	@completed_on [datetime]
,	@error_text varchar(8000)
,	@record_count int
,	@log_task_id int

AS
	declare @download_method_id int = (select download_method_id from sfi.sfi_lookup_download_methods where download_method = @download_method)
	declare @integration_method_id int = (select integration_method_id from sfi.sfi_lookup_integration_methods where integration_method = @integration_method)

	UPDATE	sfi.sfi_log_task
	SET		download_method_id = @download_method_id
	,		integration_method_id = @integration_method_id
	,		sf_job_id = @sf_job_id
	,		sf_batch_id = @sf_batch_id
	,		completed_on = @completed_on
	,		error_text = @error_text 
	,		record_count = @record_count 
	
	WHERE	log_task_id = @log_task_id




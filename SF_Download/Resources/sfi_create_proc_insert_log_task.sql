



CREATE PROCEDURE sfi.sfi_insert_log_task
 
	@log_execution_id int
,	@task varchar(50)
,	@task_notes varchar(50)
,	@object_id int
,	@object_name varchar(255)
,	@download_method varchar(50)
,	@integration_method varchar(50)
,	@sf_job_id varchar(50)
,	@sf_batch_id varchar(50)
,	@started_on [datetime] 
,	@completed_on [datetime] = null
,	@error_text varchar(8000) = null
,	@record_count int = null

,	@log_task_id [int] OUT

AS

	declare @task_id int = (select task_id from sfi.sfi_lookup_tasks where task = @task)
	declare @download_method_id int = (select download_method_id from sfi.sfi_lookup_download_methods where download_method = @download_method)
	declare @integration_method_id int = (select integration_method_id from sfi.sfi_lookup_integration_methods where integration_method = @integration_method)


	INSERT 
	INTO	sfi.sfi_log_task (
			log_execution_id 
	,		task_id 
	,		task_notes 
	,		object_id 
	,		object_name 
	,		download_method_id
	,		integration_method_id 
	,		sf_job_id 
	,		sf_batch_id 
	,		started_on 
	,		completed_on 	
	,		error_text 
	,		record_count
	) 
	VALUES	(
			@log_execution_id
	,		@task_id
	,		@task_notes 
	,		@object_id
	,		@object_name 
	,		@download_method_id
	,		@integration_method_id 
	,		@sf_job_id 
	,		@sf_batch_id 
	,		@started_on 
	,		@completed_on 
	,		@error_text 
	,		@record_count 
	) 

	SET @log_task_id = SCOPE_IDENTITY()



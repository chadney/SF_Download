

CREATE PROCEDURE [sfi].[sfi_log_task_complete]
 
	@completed_on [datetime]
,	@error_text varchar(8000)
,	@record_count int
,	@log_task_id int

AS

	UPDATE	sfi.sfi_log_task
	SET		completed_on = @completed_on
	,		error_text = @error_text 
	,		record_count = @record_count 
	WHERE	log_task_id = @log_task_id


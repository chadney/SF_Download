
CREATE PROCEDURE sfi.sfi_update_log_execution
  
	@started_on datetime
,	@completed_on datetime
,	@username varchar(50)
,	@log_execution_id int
,	@ran_to_completion bit 
,	@error_text varchar(8000) = null


AS

	UPDATE	sfi.sfi_log_execution
	SET		started_on = @started_on
	,		completed_on = @completed_on
	,		username = @username
	,		ran_to_completion = @ran_to_completion
	,		error_text = @error_text
	WHERE	log_execution_id = @log_execution_id



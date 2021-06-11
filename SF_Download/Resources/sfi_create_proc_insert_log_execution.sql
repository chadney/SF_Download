

CREATE PROCEDURE sfi.sfi_insert_log_execution
 
	@action_code varchar(20)
,	@started_on datetime
,	@completed_on datetime
,	@username varchar(50)
,	@log_execution_id int OUT
,	@ran_to_completion bit = 0
,	@error_text varchar(8000) = null


AS

	INSERT 
	INTO	sfi.sfi_log_execution(action_code, started_on, completed_on, username, ran_to_completion, error_text) 
	VALUES	(@action_code, @started_on, @completed_on, @username, @ran_to_completion, @error_text) 

	SET @log_execution_id = SCOPE_IDENTITY()



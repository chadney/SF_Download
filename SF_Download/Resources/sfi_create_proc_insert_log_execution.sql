/*

SF Download – Integrating Salesforce Downloads to SQL Server

Copyright (C) 2021 Kevin Chadney


This program is free software: you can redistribute it and/or modify

it under the terms of the GNU General Public License as published by

the Free Software Foundation, either version 3 of the License, or

(at your option) any later version.


This program is distributed in the hope that it will be useful,

but WITHOUT ANY WARRANTY; without even the implied warranty of

MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the

GNU General Public License for more details.


You should have received a copy of the GNU General Public License

along with this program.  If not, see <https://www.gnu.org/licenses/>.

*/



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



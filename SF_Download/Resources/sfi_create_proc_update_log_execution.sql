

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



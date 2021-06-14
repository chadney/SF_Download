

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



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




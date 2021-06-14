
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


create procedure sfi.sfi_insert_settings 

	@is_sandbox bit
,	@sf_username varchar(50)
,	@sf_password varchar(50)
,	@sf_token varchar(50)
,	@download_new_objects bit
,	@download_empty_objects bit
,	@download_method varchar(50)
,	@integration_method varchar(50)

as

declare @download_method_id int = (select download_method_id from sfi.sfi_lookup_download_methods where download_method = @download_method)
declare @integration_method_id int = (select integration_method_id from sfi.sfi_lookup_integration_methods where integration_method = @integration_method)

insert into sfi.sfi_settings
values	
(	@is_sandbox 
,	@sf_username 
,	@sf_password 
,	@sf_token 
,	@download_new_objects 
,	@download_empty_objects
,	@download_method_id 
,	@integration_method_id	)


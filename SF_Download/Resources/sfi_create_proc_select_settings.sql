
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

create procedure sfi.sfi_select_settings
as

select	s.is_sandbox
,		s.sf_username
,		s.sf_password
,		s.sf_token
,		s.download_new_objects
,		s.download_empty_objects
,		dm.download_method
,		im.integration_method
from	sfi.sfi_settings s
		join sfi.sfi_lookup_download_methods dm
			on s.download_method_id = dm.download_method_id
		join sfi.sfi_lookup_integration_methods im
			on s.integration_method_id = im.integration_method_id
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
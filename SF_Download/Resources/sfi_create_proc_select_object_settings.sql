
create procedure [sfi].[sfi_select_object_settings]

	@object_name varchar(255)

as

select	o.object_id	
,		o.name object_name
,		o.cant_query
,		o.cant_query_error
,		o.is_deleted
,		o.is_empty
,		o.download
,		dm.download_method
,		im.integration_method
,		o.is_deleted
from	sfm.sfm_objects o
		join sfi.sfi_lookup_download_methods dm
			on o.download_method = dm.download_method_id
		join sfi.sfi_lookup_integration_methods im
			on o.integration_method = im.integration_method_id
where	o.name = @object_name

create procedure [sfi].[sfi_select_empty_objects]
as


select	o.object_id
,		o.name
,		o.is_queryable
,		o.cant_query
,		o.is_deleted
,		o.is_empty
,		o.download
,		dm.download_method
,		im.integration_method
,		o.bulk_query_batch_size 
from	sfm.sfm_objects o
		join sfi.sfi_lookup_download_methods dm
			on o.download_method = dm.download_method_id
		join sfi.sfi_lookup_integration_methods im
			on o.integration_method = im.integration_method_id
where	o.is_queryable = 1
and		o.cant_query = 0
and		o.is_empty = 1
and		o.download = 1
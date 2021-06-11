CREATE procedure [sfi].[sfi_select_downloadable_objects]
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
,		old.last_download_on
from	sfm.sfm_objects o
		join sfi.sfi_lookup_download_methods dm
			on o.download_method = dm.download_method_id
		join sfi.sfi_lookup_integration_methods im
			on o.integration_method = im.integration_method_id
		left join sfi.sfi_object_last_download_v old
			on o.name = old.object_name
			--TODO
			--on o.object_id = old.object_id
where	o.is_queryable = 1
and		o.cant_query = 0
and		o.is_empty = 0
and		o.download = 1

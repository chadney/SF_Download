create procedure [sfi].[sfi_select_object_fields]

	@object_name varchar(255)
,	@download bit = null

as

select	f.field_id	
,		f.object_id	
,		o.name object_name
,		f.name	
,		f.label	
,		f.soap_type	
,		f.type	
,		f.length	
,		f.byte_length	
,		f.digits	
,		f.precision	
,		f.scale	
,		f.is_autonumber	
,		f.is_calculated	
,		f.is_case_sensitive	
,		f.is_custom	
,		f.is_dependent_pick_list	
,		f.is_external_id	
,		f.is_ID_lookup	
,		f.is_name_field	
,		f.is_unique	
,		f.is_restricted_picklist	
,		f.is_deleted	
,		f.download
from	sfm.sfm_objects o
		join sfm.sfm_fields f
			on o.object_id = f.object_id
where	(o.name = @object_name and f.download = @download)
or		(o.name = @object_name and @download is null)




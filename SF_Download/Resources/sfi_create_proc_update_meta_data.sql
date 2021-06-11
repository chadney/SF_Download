

create procedure [sfi].[sfi_update_meta_data]

	@log_task_id int

as

	
	----------------OBJECTS-----------------
	declare @new_change_id int = (select change_type_id from sfi.sfi_lookup_change_types where name = 'New Object')
	declare @delete_change_id int = (select change_type_id from sfi.sfi_lookup_change_types where name = 'Delete Object')


	/* 
		Add deleted objects to the history

		If an object name exists in objects that is not flagged as deleted and does not
		exist in object_temp then it has been delete in SF and is not yet recorded 
		as deleted. Add a history record to record this change.

	*/

	insert 
	into	[sfm].[sfm_objects_history]

			(	[log_task_id]
			,	[change_type_id]
			,	[key_prefix]
			,	[name] 
			,	[label] 
			,	[is_custom] 
			,	[is_custom_setting] 
			,	[is_queryable]
			,	[cant_query]
			,	[cant_query_error]
			,	[is_deleted]
			,	is_empty	
			,	download
			,	download_method	
			,	integration_method
			,	bulk_query_batch_size	)

	select	@log_task_id
	,		@delete_change_id
	,		[key_prefix]
	,		[name] 
	,		[label] 
	,		[is_custom] 
	,		[is_custom_setting] 
	,		[is_queryable]
	,		[cant_query]
	,		[cant_query_error]
	,		1
	,		is_empty	
	,		download
	,		download_method
	,		integration_method
	,		bulk_query_batch_size

	from	sfm.sfm_objects o
	where	o.is_deleted = 0
	and		o.name not in (select t.name from sfm.sfm_objects_temp t)



	
	/* 
		Add new objects to the history

		If an object name exists in objects_temp that does not
		exist in objects then it has been created in SF and is not yet recorded 
		as existing. Add a history record to record this change.

	*/
	
	insert 
	into	[sfm].[sfm_objects_history]

			(	[log_task_id]
			,	[change_type_id]
			,	[key_prefix]
			,	[name] 
			,	[label] 
			,	[is_custom] 
			,	[is_custom_setting] 
			,	[is_queryable]
			,	[cant_query]
			,	[cant_query_error]
			,	[is_deleted]	
			,	is_empty	
			,	download
			,	download_method	
			,	integration_method
			,	bulk_query_batch_size	)

	select	@log_task_id
	,		@new_change_id
	,		[key_prefix]
	,		[name] 
	,		[label] 
	,		[is_custom] 
	,		[is_custom_setting] 
	,		[is_queryable]
	,		[cant_query]
	,		[cant_query_error]
	,		0 is_deleted
	,		is_empty	
	,		download
	,		download_method	
	,		integration_method
	,		bulk_query_batch_size	

	from	sfm.sfm_objects_temp t
	where	t.name not in (select o.name from sfm.sfm_objects o)



	/*
		Update deleted object to deleted
		
		Objects identified as deleted are set to is_deleted in objects

	*/
	update	o
	set		is_deleted = 1
	from	sfm.sfm_objects o
			join sfm.sfm_objects_history h 
				on o.name = h.name
				and h.log_task_id = @log_task_id
				and	h.change_type_id = @delete_change_id

	/*
		Insert new object records

		Objects identified as new are inserted into objects
	*/
	insert	
	into	[sfm].[sfm_objects]
			(	[key_prefix]
			,	[name] 
			,	[label] 
			,	[is_custom] 
			,	[is_custom_setting] 
			,	[is_queryable]
			,	[cant_query]
			,	[cant_query_error]
			,	[is_deleted]	
			,	is_empty	
			,	download
			,	download_method	
			,	integration_method
			,	bulk_query_batch_size	)

	select	[key_prefix]
	,		[name] 
	,		[label] 
	,		[is_custom] 
	,		[is_custom_setting] 
	,		[is_queryable]
	,		[cant_query]
	,		[cant_query_error]
	,		0 is_deleted
	,		is_empty	
	,		download
	,		download_method	
	,		integration_method
	,		bulk_query_batch_size	

	from	sfm.sfm_objects_history h
	where	h.change_type_id = @new_change_id
	and		h.log_task_id = @log_task_id


	----------------FIELDS-----------------
	set @new_change_id = (select change_type_id from sfi.sfi_lookup_change_types where name = 'New Field')
	set @delete_change_id = (select change_type_id from sfi.sfi_lookup_change_types where name = 'Delete Field')
	declare @datatype_change_id int = (select change_type_id from sfi.sfi_lookup_change_types where name = 'Change Field Datatype')


	/* 
		Add deleted fields to the history

		If a field name exists in fields that is not flagged as deleted and does not
		exist in object_temp then it has been deleted in SF and is not yet recorded 
		as deleted. Add a history record to record this change.

	*/

	insert 
	into	[sfm].[sfm_fields_history]
			(	[log_task_id]
			,	[change_type_id]
			,	[object_name]
			,	[name]
			,	[label]
			,	[soap_type]
			,	[type]
			,	[length]
			,	[byte_length]
			,	[digits]
			,	[precision]
			,	[scale]
			,	[is_autonumber]
			,	[is_calculated]
			,	[is_case_sensitive]
			,	[is_custom]
			,	[is_dependent_pick_list]
			,	[is_external_id]
			,	[is_ID_lookup]
			,	[is_name_field]
			,	[is_unique]
			,	[is_restricted_picklist]	
			,	[is_deleted]
			,	[download]	)

	select		@log_task_id
			,	@delete_change_id
			,	o.[name] object_name
			,	f.[name]
			,	f.[label]
			,	f.[soap_type]
			,	f.[type]
			,	f.[length]
			,	f.[byte_length]
			,	f.[digits]
			,	f.[precision]
			,	f.[scale]
			,	f.[is_autonumber]
			,	f.[is_calculated]
			,	f.[is_case_sensitive]
			,	f.[is_custom]
			,	f.[is_dependent_pick_list]
			,	f.[is_external_id]
			,	f.[is_ID_lookup]
			,	f.[is_name_field]
			,	f.[is_unique]
			,	f.[is_restricted_picklist]	
			,	1 is_deleted
			,	f.download

	from	[sfm].[sfm_fields] f
			join sfm.sfm_objects o
				on f.object_id = o.object_id
	where	f.is_deleted = 0
	and		not exists (select	1 
						from	sfm.sfm_fields_temp t
						where	o.name = t.object_name
						and		f.name = t.name)



	/* 
		Add new fields to the history

		If a field name exists in fields_temp that does not
		exist in fields then it has been created in SF and is not yet recorded 
		as existing. Add a history record to record this change.

	*/
	insert 
	into	[sfm].[sfm_fields_history]
			(	[log_task_id]
			,	[change_type_id]
			,	[object_name]
			,	[name]
			,	[label]
			,	[soap_type]
			,	[type]
			,	[length]
			,	[byte_length]
			,	[digits]
			,	[precision]
			,	[scale]
			,	[is_autonumber]
			,	[is_calculated]
			,	[is_case_sensitive]
			,	[is_custom]
			,	[is_dependent_pick_list]
			,	[is_external_id]
			,	[is_ID_lookup]
			,	[is_name_field]
			,	[is_unique]
			,	[is_restricted_picklist]	
			,	[is_deleted]
			,	download	)

	select		@log_task_id
			,	@new_change_id
			,	[object_name] object_name
			,	[name]
			,	[label]
			,	[soap_type]
			,	[type]
			,	[length]
			,	[byte_length]
			,	[digits]
			,	[precision]
			,	[scale]
			,	[is_autonumber]
			,	[is_calculated]
			,	[is_case_sensitive]
			,	[is_custom]
			,	[is_dependent_pick_list]
			,	[is_external_id]
			,	[is_ID_lookup]
			,	[is_name_field]
			,	[is_unique]
			,	[is_restricted_picklist]	
			,	0 is_deleted
			,	download
	from	sfm.sfm_fields_temp t
	where	not exists (select	1 
						from	sfm.sfm_fields f
								join sfm.sfm_objects o
									on f.object_id = o.object_id
						where	o.name = t.object_name
						and		f.name = t.name)


	/* 
		Add fields changed datatype to the history

		If a fields datatype is different in fields_temp to fields 
		then it has been created in SF and is not yet recorded.
		Add a history record to record this change.

	*/

	insert 
	into	[sfm].[sfm_fields_history]
			(	[log_task_id]
			,	[change_type_id]
			,	[object_name]
			,	[name]
			,	[label]
			,	[soap_type]
			,	[type]
			,	[length]
			,	[byte_length]
			,	[digits]
			,	[precision]
			,	[scale]
			,	[is_autonumber]
			,	[is_calculated]
			,	[is_case_sensitive]
			,	[is_custom]
			,	[is_dependent_pick_list]
			,	[is_external_id]
			,	[is_ID_lookup]
			,	[is_name_field]
			,	[is_unique]
			,	[is_restricted_picklist]	
			,	[is_deleted]
			,	download	)

	select		@log_task_id
			,	@datatype_change_id
			,	t.[object_name] object_name
			,	t.[name]
			,	t.[label]
			,	t.[soap_type]
			,	t.[type]
			,	t.[length]
			,	t.[byte_length]
			,	t.[digits]
			,	t.[precision]
			,	t.[scale]
			,	t.[is_autonumber]
			,	t.[is_calculated]
			,	t.[is_case_sensitive]
			,	t.[is_custom]
			,	t.[is_dependent_pick_list]
			,	t.[is_external_id]
			,	t.[is_ID_lookup]
			,	t.[is_name_field]
			,	t.[is_unique]
			,	t.[is_restricted_picklist]	
			,	0 is_deleted
			,	t.download
	from	sfm.sfm_fields_temp t
			join sfm.sfm_objects o 
				on t.object_name = o.name
			join sfm.sfm_fields f
				on t.name = f.name
				and o.object_id = f.object_id
	where	t.type != f.type
	or		t.length != f.length
	or		t.precision != f.precision
	or		t.scale != f.scale





	/*
		Update deleted fields to deleted
		
		Fields identified as deleted are set to is_deleted in fields

	*/

	update	f
	set		is_deleted = 1
	from	sfm.sfm_fields f
			join sfm.sfm_objects o
				on f.object_id = o.object_id
			join sfm.sfm_fields_history h 
				on o.name = h.object_name
				and f.name = h.name
				and h.log_task_id = @log_task_id
				and	h.change_type_id = @delete_change_id


	/*
		Update change fields datatypes
		
		Fields identified as changed datatype are set to the new datatype columns

	*/

	update	f
	set		f.type = h.type
	,		f.length = h.length
	,		f.precision = h.precision
	,		f.scale = h.scale
	from	sfm.sfm_fields f
			join sfm.sfm_objects o
				on f.object_id = o.object_id
			join sfm.sfm_fields_history h 
				on o.name = h.object_name
				and f.name = h.name
				and h.log_task_id = @log_task_id
				and	h.change_type_id = @datatype_change_id

	/*
		Insert new field records
		
		Fields identified as created are inserted into fields

	*/
	insert	
	into	[sfm].[sfm_fields]
			(	[object_id]
			,	[name]
			,	[label]
			,	[soap_type]
			,	[type]
			,	[length]
			,	[byte_length]
			,	[digits]
			,	[precision]
			,	[scale]
			,	[is_autonumber]
			,	[is_calculated]
			,	[is_case_sensitive]
			,	[is_custom]
			,	[is_dependent_pick_list]
			,	[is_external_id]
			,	[is_ID_lookup]
			,	[is_name_field]
			,	[is_unique]
			,	[is_restricted_picklist]	
			,	[is_deleted]
			,	download )

	select		o.[object_id]
			,	h.[name]
			,	h.[label]
			,	h.[soap_type]
			,	h.[type]
			,	h.[length]
			,	h.[byte_length]
			,	h.[digits]
			,	h.[precision]
			,	h.[scale]
			,	h.[is_autonumber]
			,	h.[is_calculated]
			,	h.[is_case_sensitive]
			,	h.[is_custom]
			,	h.[is_dependent_pick_list]
			,	h.[is_external_id]
			,	h.[is_ID_lookup]
			,	h.[is_name_field]
			,	h.[is_unique]
			,	h.[is_restricted_picklist]	
			,	h.[is_deleted]	
			,	h.download
	from	sfm.sfm_fields_history h
			join sfm.sfm_objects o
				on h.object_name = o.name
	where	h.change_type_id = @new_change_id
	and		h.log_task_id = @log_task_id


	----------------PICKLIST VALUES-----------------
	set @new_change_id = (select change_type_id from sfi.sfi_lookup_change_types where name = 'New Picklist Value')
	set @delete_change_id = (select change_type_id from sfi.sfi_lookup_change_types where name = 'Delete Picklist Value')



	/* 
		Add new picklist values to the history

		If a picklist value exists in picklist_values_temp that does not
		exist in picklist values then it has been created in SF and is not yet recorded 
		as existing. Add a history record to record this change.

	*/
	
	insert 
	into	[sfm].[sfm_picklist_values_history]

			(	[log_task_id]
			,	[change_type_id]
			,	object_name
			,	field_name
			,	value
			,	label
			,	active
			,	default_value	)

	select	@log_task_id
	,		@new_change_id
	,		t.object_name
	,		t.field_name
	,		t.value
	,		t.label
	,		t.active
	,		t.default_value
	from	sfm.sfm_picklist_values_temp t
	where	not exists (	select	1 
							from	sfm.sfm_picklist_values p
									join sfm.sfm_fields f
										on p.field_id = f.field_id
									join sfm.sfm_objects o
										on f.object_id = o.object_id
							where	o.name = t.object_name
							and		f.name = t.field_name
							and		p.value = t.value)


	/*
		Insert new picklist value records
		
		Picklist values identified as created are inserted into picklist values

	*/
	insert	
	into	[sfm].[sfm_picklist_values]
			(	field_id
			,	value
			,	label
			,	active
			,	default_value	)

	select	f.field_id
	,		h.value
	,		h.label
	,		h.active
	,		h.default_value
	from	sfm.sfm_picklist_values_history h
			join sfm.sfm_objects o
				on h.object_name = o.name
			join sfm.sfm_fields f
				on o.object_id = f.object_id
				and h.field_name = f.name
	where	h.change_type_id = @new_change_id
	and		h.log_task_id = @log_task_id


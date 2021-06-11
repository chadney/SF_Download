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


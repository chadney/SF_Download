
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

CREATE TABLE [sfi].[sfi_settings](
	[is_sandbox] [bit] NULL,
	[sf_username] [varchar](50) NULL,
	[sf_password] [varchar](50) NULL,
	[sf_token] [varchar](50) NULL,
	download_new_objects bit null,
	download_empty_objects [bit] NULL,
	download_method_id int null,
	integration_method_id int null

) 


CREATE TABLE sfi.sfi_log_execution
( 
	log_execution_id int identity(1,1) primary key clustered
,	action_code varchar(20)
,	started_on datetime
,	completed_on datetime
,	username varchar(50)
,	ran_to_completion bit
,	error_text varchar(8000)
)


CREATE TABLE [sfi].[sfi_log_task](
	[log_task_id] [int] IDENTITY(1,1) NOT NULL
,	[log_execution_id] int NOT NULL
,	[task_notes] varchar(50) NULL
,	task_id int
,	[object_id] int NULL
,	object_name varchar(255)
,	download_method_id int
,	integration_method_id int
,	[sf_job_id] varchar(50)
,	[sf_batch_id] varchar(50)
,	[started_on] [datetime] NULL
,	[completed_on] [datetime] NULL
,	error_text varchar(8000)
,	record_count int

PRIMARY KEY CLUSTERED 
(
	[log_task_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]



CREATE TABLE sfi.sfi_log_task_details
( 
	log_task_detail_id int identity(1,1) primary key clustered
,	batch_id varchar(50)
,	result_id varchar(50)
,	started_on datetime
,	completed_on datetime
,	completion_status varchar(50)
,	error_details varchar(4000)

)



create table sfi.sfi_lookup_change_types
(
	[change_type_id] [int] identity(1,1) primary key clustered
,	[name] varchar(50)
)


insert into sfi.sfi_lookup_change_types values ('New Object')
insert into sfi.sfi_lookup_change_types values ('Delete Object')
insert into sfi.sfi_lookup_change_types values ('New Field')
insert into sfi.sfi_lookup_change_types values ('Delete Field')
insert into sfi.sfi_lookup_change_types values ('Change Field Datatype')
insert into sfi.sfi_lookup_change_types values ('New Picklist Value')
insert into sfi.sfi_lookup_change_types values ('Delete Picklist Value')
insert into sfi.sfi_lookup_change_types values ('Update Object Download Status')
insert into sfi.sfi_lookup_change_types values ('Update Field Download Status')
insert into sfi.sfi_lookup_change_types values ('New Record')
insert into sfi.sfi_lookup_change_types values ('Delete Record')
insert into sfi.sfi_lookup_change_types values ('Update Record')





create table sfi.sfi_lookup_download_methods
(
download_method_id int identity(1,1) primary key clustered
, download_method varchar(50)
)

insert into sfi.sfi_lookup_download_methods values ('SOAP')
insert into sfi.sfi_lookup_download_methods values ('Bulk Query')
insert into sfi.sfi_lookup_download_methods values ('Bulk Query Batched')
insert into sfi.sfi_lookup_download_methods values ('Automatic')


create table sfi.sfi_lookup_integration_methods
(
integration_method_id int identity(1,1) primary key clustered
, integration_method varchar(50)
)

insert into sfi.sfi_lookup_integration_methods values ('Incremental')
insert into sfi.sfi_lookup_integration_methods values ('Delete and Replace')



create table sfi.sfi_lookup_tasks
(	
	task_id int identity(1,1) primary key clustered
,	task varchar(50)
,	task_description varchar(1000)
)

insert into sfi.sfi_lookup_tasks values ('Get Object Settings', null)
insert into sfi.sfi_lookup_tasks values ('Move Object Data To Base', null)
insert into sfi.sfi_lookup_tasks values ('Download Object Data', null)
insert into sfi.sfi_lookup_tasks values ('Upload Object Data', null)
insert into sfi.sfi_lookup_tasks values ('Alter Object Table Field Type', null)
insert into sfi.sfi_lookup_tasks values ('Alter Object Table Fields', null)
insert into sfi.sfi_lookup_tasks values ('Create Object Table', null)
insert into sfi.sfi_lookup_tasks values ('Update Field Download Status', null)
insert into sfi.sfi_lookup_tasks values ('Get Object Fields', null)
insert into sfi.sfi_lookup_tasks values ('Get Downloadable Objects', null)
insert into sfi.sfi_lookup_tasks values ('Get Changed Fields', null)
insert into sfi.sfi_lookup_tasks values ('Get New Object Fields', null)
insert into sfi.sfi_lookup_tasks values ('Get Global Settings', null)
insert into sfi.sfi_lookup_tasks values ('Get Empty Objects', null)
insert into sfi.sfi_lookup_tasks values ('Get New Objects', null)
insert into sfi.sfi_lookup_tasks values ('Validate Download Method', null)
insert into sfi.sfi_lookup_tasks values ('Validate Integration Method', null)
insert into sfi.sfi_lookup_tasks values ('Connect to SF', null)
insert into sfi.sfi_lookup_tasks values ('Test Salesforce Connection', null)
insert into sfi.sfi_lookup_tasks values ('Setup SQL Database', null)
insert into sfi.sfi_lookup_tasks values ('Save Inital Settings', null)
insert into sfi.sfi_lookup_tasks values ('Download Object Metadata', null)
insert into sfi.sfi_lookup_tasks values ('Upload Object Metadata', null)
insert into sfi.sfi_lookup_tasks values ('Download Field Metadata', null)
insert into sfi.sfi_lookup_tasks values ('Upload Field Metadata', null)
insert into sfi.sfi_lookup_tasks values ('Detect Metadata Changes', null)
insert into sfi.sfi_lookup_tasks values ('Count Object', null)
insert into sfi.sfi_lookup_tasks values ('Upload Object Status', null)

/*
insert into sfi.sfi_lookup_tasks values ('Test Salesforce Connection','Test Connection to the SF Partner API')
insert into sfi.sfi_lookup_tasks values ('Connect to SF','Connect to the SF Partner API')
insert into sfi.sfi_lookup_tasks values ('Download Object Metadata','Download all SF Object metadata')
insert into sfi.sfi_lookup_tasks values ('Download Field Metadata ','Download all SF Field metadata')
insert into sfi.sfi_lookup_tasks values ('Upload Object Metadata ','Load all SF Object metadata to SQL temporary object metadata tables')
insert into sfi.sfi_lookup_tasks values ('Upload Field Metadata ',' Load all SF Field metadata to SQL temporary field and picklist value metadata tables')
insert into sfi.sfi_lookup_tasks values ('Detect Metadata Changes','Loads object and field data to history and base metadata tables and calculate changes since last load')
insert into sfi.sfi_lookup_tasks values ('Create Initial Object Tables',' Create the temp, history and base tables for all non empty, queryable objects')
insert into sfi.sfi_lookup_tasks values ('Test Object Status','Test whether objects are queryable and get object row counts')
insert into sfi.sfi_lookup_tasks values ('Test New Object Status','Test whether new objects are queryable and get object row counts')
insert into sfi.sfi_lookup_tasks values ('Test Empty Object Status','Test whether empty objects now have records and gets row counts')
insert into sfi.sfi_lookup_tasks values ('Create New Object Tables','Create the temp, history and base tables for any new, non empty queryable objects')
insert into sfi.sfi_lookup_tasks values ('Add New Fields','add new fields to the temp, history and base tables')
insert into sfi.sfi_lookup_tasks values ('Download Object','Download data from the object to the SQL temp tables')
insert into sfi.sfi_lookup_tasks values ('Upload Object','Uploads the data from the SQL temp tables to history and base tables')
insert into sfi.sfi_lookup_tasks values ('Commit Load','Commits the data to all history and base tables')
*/


CREATE TABLE [sfm].[sfm_objects](
	[object_id] [int] IDENTITY(1,1) NOT NULL,
	[key_prefix] [varchar](3) NULL,
	[name] [varchar](255) NOT NULL,
	[label] [varchar](255) NULL,
	[is_custom] [bit] NULL,
	[is_custom_setting] [bit] NULL,
	[is_queryable] [bit] NULL,
	[cant_query] [bit] NULL,
	[cant_query_error] varchar (8000) NULL,
	[is_deleted] bit NULL,
	[is_empty] bit NULL,
	download bit null,
 	download_method int null,
 	integration_method int null,
 	bulk_query_batch_size int null

PRIMARY KEY CLUSTERED 
(
	[object_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]



CREATE TABLE [sfm].[sfm_objects_history](

	[object_history_id] [int] identity(1,1) primary key clustered
,	[log_task_id] [int]
,	[change_type_id] [int]
,	[key_prefix] [varchar](3) NULL
,	[name] [varchar](255) NOT NULL
,	[label] [varchar](255) NULL
,	[is_custom] [bit] NULL
,	[is_custom_setting] [bit] NULL
,	[is_queryable] [bit] NULL
,	[cant_query] [bit] NULL
,	[cant_query_error] varchar (8000) NULL
,	[is_deleted] bit NULL
,	[is_empty] bit NULL
,	download bit null
, 	download_method int null
, 	integration_method int null
, 	bulk_query_batch_size int null)



CREATE TABLE [sfm].[sfm_objects_temp](
	[key_prefix] [varchar](3) NULL,
	[name] [varchar](255) NOT NULL,
	[label] [varchar](255) NULL,
	[is_custom] [bit] NULL,
	[is_custom_setting] [bit] NULL,
	[is_queryable] [bit] NULL,
	[cant_query] [bit] NULL,
	[cant_query_error] varchar (8000) NULL,
	[is_empty] bit NULL,
	download bit null,
 	download_method int null,
 	integration_method int null,
 	bulk_query_batch_size int null)

CREATE UNIQUE INDEX [IX_sfm_objects_temp_name] on [sfm].[sfm_objects_temp]([name]);


CREATE TABLE [sfm].[sfm_fields](
	[field_id] [int] IDENTITY(1,1) NOT NULL,
	[object_id] [int] NULL,
	[name] [varchar](255) NOT NULL,
	[label] [varchar](255) NULL,
	[soap_type] [varchar](255) NULL,
	[type] [varchar](255) NULL,
	[length] [int] NULL,
	[byte_length] [int] NULL,
	[digits] [int] NULL,
	[precision] [int] NULL,
	[scale] [int] NULL,
	[is_autonumber] [bit] NULL,
	[is_calculated] [bit] NULL,
	[is_case_sensitive] [bit] NULL,
	[is_custom] [bit] NULL,
	[is_dependent_pick_list] [bit] NULL,
	[is_external_id] [bit] NULL,
	[is_ID_lookup] [bit] NULL,
	[is_name_field] [bit] NULL,
	[is_unique] [bit] NULL,
	[is_restricted_picklist] [bit] NULL,
	[is_deleted] [bit] NULL,
	[download] [bit] NULL

PRIMARY KEY CLUSTERED 
(
	[field_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]


CREATE TABLE [sfm].[sfm_fields_history](
	[field_history_id] [int] identity(1,1) primary key clustered,
	[log_task_id] [int],
	[change_type_id] [int],
	[object_name] [varchar](255) NOT NULL,
	[name] [varchar](255) NOT NULL,
	[label] [varchar](255) NULL,
	[soap_type] [varchar](255) NULL,
	[type] [varchar](255) NULL,
	[length] [int] NULL,
	[byte_length] [int] NULL,
	[digits] [int] NULL,
	[precision] [int] NULL,
	[scale] [int] NULL,
	[is_autonumber] [bit] NULL,
	[is_calculated] [bit] NULL,
	[is_case_sensitive] [bit] NULL,
	[is_custom] [bit] NULL,
	[is_dependent_pick_list] [bit] NULL,
	[is_external_id] [bit] NULL,
	[is_ID_lookup] [bit] NULL,
	[is_name_field] [bit] NULL,
	[is_unique] [bit] NULL,
	[is_restricted_picklist] [bit] NULL,
	[is_deleted] [bit] NULL,
	[download] [bit] NULL
	)

CREATE TABLE [sfm].[sfm_fields_temp](
	[object_name] [varchar](255) NOT NULL,
	[name] [varchar](255) NOT NULL,
	[label] [varchar](255) NULL,
	[soap_type] [varchar](255) NULL,
	[type] [varchar](255) NULL,
	[length] [int] NULL,
	[byte_length] [int] NULL,
	[digits] [int] NULL,
	[precision] [int] NULL,
	[scale] [int] NULL,
	[is_autonumber] [bit] NULL,
	[is_calculated] [bit] NULL,
	[is_case_sensitive] [bit] NULL,
	[is_custom] [bit] NULL,
	[is_dependent_pick_list] [bit] NULL,
	[is_external_id] [bit] NULL,
	[is_ID_lookup] [bit] NULL,
	[is_name_field] [bit] NULL,
	[is_unique] [bit] NULL,
	[is_restricted_picklist] [bit] NULL,
	[download] [bit] NULL
	)

CREATE UNIQUE INDEX [IX_sfm_fields_temp_names] on [sfm].[sfm_fields_temp]([object_name],[name]);




CREATE TABLE [sfm].[sfm_picklist_values](
	[picklist_value_id] [int] IDENTITY(1,1) NOT NULL,
	[field_id] int NOT NULL,
	[value] [varchar](255) NULL,
	[label] [varchar](255) NULL,
	[active] [bit] NULL,
	[default_value] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[picklist_value_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]



CREATE TABLE [sfm].[sfm_picklist_values_history](
	[field_history_id] [int] identity(1,1) primary key clustered,
	[log_task_id] [int],
	[change_type_id] [int],
	[object_name] [varchar](255) NOT NULL,
	[field_name] [varchar](255) NOT NULL,
	[value] [varchar](255) NULL,
	[label] [varchar](255) NULL,
	[active] [bit] NULL,
	[default_value] [bit] NULL
)


CREATE TABLE [sfm].[sfm_picklist_values_temp](
	[object_name] [varchar](255) NOT NULL,
	[field_name] [varchar](255) NOT NULL,
	[value] [varchar](255) NULL,
	[label] [varchar](255) NULL,
	[active] [bit] NULL,
	[default_value] [bit] NULL
)

--CREATE UNIQUE INDEX [IX_sfm_picklist_value_temp_names] on [sfm].[sfm_picklist_values_temp]([object_name],[field_name], [value]);




using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SF_Download
{
    //Target DB for SF download
    public class SFDTarget

    {
        //properties and fields
        #region
        private string Server { get; set; }
        private string Database { get; set; }
        private SqlConnection _sqlConnection;
        private SqlTransaction _sqlTransaction;
        private string _ConnectionString;

        private Boolean _isConnectionOK;
        public bool isConnectionOK { get { return _isConnectionOK; } }

        private Exception _ConnectionError;
        public Exception ConnectionError { get { return _ConnectionError; } }

        private int _LogExecutionId;
        public int LogExecutionId { get { return _LogExecutionId; } }


        public Logger Logger { get; set; }
        public bool IsSandbox { get; set; }
        public string SfUsername { get; set; }
        public string SfPassword { get; set; }
        public string SfToken { get; set; }
        public bool DownloadNewObjects { get; set; }
        public bool DownloadEmptyObjects { get; set; }
        public string DownloadMethod { get; set; }
        public string IntegrationMethod { get; set; }
        
        #endregion


        //constructor
        public SFDTarget(string Server, string Database)
        {
            this.Server = Server;
            this.Database = Database;

            //use the connection string builder to prevent connection string pollution attacks
            _ConnectionString =
                "Data Source=" + Server + ";" +
                "Initial Catalog=" + Database + ";" +
                "Integrated Security=SSPI;" +
                "MultipleActiveResultSets=true;";

            TestConnection();

        }


        //connections and transactions
        public void TestConnection()
        {
            using (SqlConnection cn = new SqlConnection(_ConnectionString))
            {
                try
                {
                    cn.Open();
                    _isConnectionOK = true;
                }
                catch (Exception e)
                {
                    _isConnectionOK = false;
                    _ConnectionError = e;
                    return;
                }
            }
        }

        public void OpenPersistConnection()
        {
            _sqlConnection = new SqlConnection(_ConnectionString);
            _sqlConnection.Open();

        }

        public void OpenTransaction()
        {
            _sqlTransaction = _sqlConnection.BeginTransaction();

        }

        public void CommitTransaction()
        {


            _sqlTransaction.Commit();


        }


        //initialise the database
        public bool Initialise()
        {

            int logTaskId = Logger.LogTaskStart(LogTaskType.SetupSQLDatabase);

            try
            {

                if (SchemasExist()) { throw new Exception("Cannot initialise the database as some required schemas already exist."); };

                //set up schemas and tables
                CreateSchema("sfd");
                CreateSchema("sfm");
                CreateSchema("sfi");
                CreateSchema("sft");
                RunScript(Properties.Resources.sfm_create_tables);
                RunScript(Properties.Resources.sfi_create_view_object_last_download);
                RunScript(Properties.Resources.sfi_create_view_objects_history);
                RunScript(Properties.Resources.sfi_create_view_fields_history);
                RunScript(Properties.Resources.sfi_create_proc_insert_log_execution);
                RunScript(Properties.Resources.sfi_create_proc_update_log_execution);
                RunScript(Properties.Resources.sfi_create_proc_insert_log_task);
                RunScript(Properties.Resources.sfi_create_proc_log_task_complete);
                RunScript(Properties.Resources.sfi_create_proc_update_log_task);
                RunScript(Properties.Resources.sfi_create_proc_update_meta_data);
                RunScript(Properties.Resources.sfi_create_proc_update_object_download_status);
                RunScript(Properties.Resources.sfi_create_proc_select_downloadable_objects);
                RunScript(Properties.Resources.sfi_create_proc_update_field_download_status);
                RunScript(Properties.Resources.sfi_create_proc_select_empty_objects);
                RunScript(Properties.Resources.sfi_create_proc_select_new_objects);
                RunScript(Properties.Resources.sfi_create_proc_select_object_fields);
                RunScript(Properties.Resources.sfi_create_proc_insert_settings);
                RunScript(Properties.Resources.sfi_create_proc_table_exists);
                RunScript(Properties.Resources.sfi_create_proc_select_settings);
                RunScript(Properties.Resources.sfi_create_proc_select_object_settings);
                RunScript(Properties.Resources.sfi_create_proc_select_new_fields);
                RunScript(Properties.Resources.sfi_create_proc_select_changed_datatype_fields);
            }

            catch (Exception e)
            {
                Logger.LogTaskError(LogTaskType.SetupSQLDatabase, logTaskId, e);
                Environment.Exit(0);

            }

            Logger.LogTaskComplete(LogTaskType.SetupSQLDatabase, logTaskId);
            return true;


        }


        // get saved object data
        public void GetSettings()
        {
            int logTaskId = Logger.LogTaskStart(LogTaskType.GetGlobalSettings);

            DataTable dt = new DataTable();

            try
            {
                SqlDataAdapter da = new SqlDataAdapter();
                SqlCommand cmd = new SqlCommand("sfi.sfi_select_settings", _sqlConnection);
                cmd.Transaction = _sqlTransaction;
                cmd.CommandType = CommandType.StoredProcedure;
                da.SelectCommand = cmd;
                da.Fill(dt);


                IsSandbox = (bool)dt.Rows[0]["is_sandbox"];
                SfUsername = (string)dt.Rows[0]["sf_username"];
                SfPassword = (string)dt.Rows[0]["sf_password"];
                SfToken = (string)dt.Rows[0]["sf_token"];
                DownloadNewObjects = (bool)dt.Rows[0]["download_new_objects"];
                DownloadEmptyObjects = (bool)dt.Rows[0]["download_empty_objects"];
                DownloadMethod = (string)dt.Rows[0]["download_method"];
                IntegrationMethod = (string)dt.Rows[0]["integration_method"];

            }

            catch (Exception e)
            {
                Logger.LogTaskError(LogTaskType.GetGlobalSettings, logTaskId, e);

            }

            Logger.LogTaskComplete(LogTaskType.GetGlobalSettings, logTaskId);


        }

        public DataTable GetObjectSettings(string objectName)
        {
            int logTaskId = Logger.LogTaskStart(LogTaskType.GetObjectSettings);

            DataTable dt = new DataTable();

            try
            {
                SqlDataAdapter da = new SqlDataAdapter();
                SqlCommand cmd = new SqlCommand("sfi.sfi_select_object_settings", _sqlConnection);
                cmd.Transaction = _sqlTransaction;
                cmd.Parameters.Add(new SqlParameter("@object_name", SqlDbType.VarChar, 255));
                cmd.Parameters[0].Value = objectName;
                cmd.CommandType = CommandType.StoredProcedure;
                da.SelectCommand = cmd;
                da.Fill(dt);
            }

            catch (Exception e)
            {
                Logger.LogTaskError(LogTaskType.GetObjectSettings, logTaskId, e);
                Environment.Exit(0);
            }

            Logger.LogTaskComplete(LogTaskType.GetObjectSettings, logTaskId);
            return dt;

        }

        public DataTable GetNewObjects()
        {

            int logTaskId = Logger.LogTaskStart(LogTaskType.GetNewObjects);

            DataTable dt = new DataTable();

            try
            {

                SqlDataAdapter da = new SqlDataAdapter();
                SqlCommand cmd = new SqlCommand("sfi.sfi_select_new_objects", _sqlConnection);
                cmd.Transaction = _sqlTransaction;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@log_execution_id", SqlDbType.Int));
                cmd.Parameters[0].Value = _LogExecutionId;
                da.SelectCommand = cmd;
                da.Fill(dt);
                
            }

            catch (Exception e)
            {
                Logger.LogTaskError(LogTaskType.GetNewObjects, logTaskId, e);
                Environment.Exit(0);

            }

            Logger.LogTaskComplete(LogTaskType.GetNewObjects, logTaskId);
            return dt;

        }

        public DataTable GetNewFields(int logExecutionId)
        {

            int logTaskId = Logger.LogTaskStart(LogTaskType.GetNewObjectFields);

            DataTable dt = new DataTable();

            try
            {
                SqlDataAdapter da = new SqlDataAdapter();
                SqlCommand cmd = new SqlCommand("sfi.sfi_select_new_fields", _sqlConnection);
                cmd.Transaction = _sqlTransaction;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@log_execution_id", SqlDbType.Int));
                cmd.Parameters[0].Value = logExecutionId;
                da.SelectCommand = cmd;
                da.Fill(dt);
                
            }
            
            catch (Exception e)
            {
                Logger.LogTaskError(LogTaskType.GetNewObjectFields, logTaskId, e);
                Environment.Exit(0);
            }

            Logger.LogTaskComplete(LogTaskType.GetNewObjectFields, logTaskId);
            return dt;

        }

        public DataTable GetChangedDatatypeFields(int logExecutionId)
        {

            int logTaskId = Logger.LogTaskStart(LogTaskType.GetChangedFields);

            DataTable dt = new DataTable();

            try
            {
                SqlDataAdapter da = new SqlDataAdapter();
                SqlCommand cmd = new SqlCommand("sfi.sfi_select_changed_datatype_fields", _sqlConnection);
                cmd.Transaction = _sqlTransaction;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@log_execution_id", SqlDbType.Int));
                cmd.Parameters[0].Value = logExecutionId;
                da.SelectCommand = cmd;
                da.Fill(dt);

            }

            catch (Exception e)
            {
                Logger.LogTaskError(LogTaskType.GetChangedFields, logTaskId, e);
                Environment.Exit(0);

            }


            Logger.LogTaskComplete(LogTaskType.GetChangedFields, logTaskId);
            return dt;

        }

        public DataTable GetEmptyObjects()
        {

            int logTaskId = Logger.LogTaskStart(LogTaskType.GetEmptyObjects);

            DataTable dt = new DataTable();

            try
            {
                    SqlDataAdapter da = new SqlDataAdapter();
                    SqlCommand cmd = new SqlCommand("sfi.sfi_select_empty_objects", _sqlConnection);
                    cmd.Transaction = _sqlTransaction;
                    cmd.CommandType = CommandType.StoredProcedure;
                    da.SelectCommand = cmd;
                    da.Fill(dt);

            }
            catch (Exception e)
            {
                Logger.LogTaskError(LogTaskType.GetEmptyObjects, logTaskId, e);
                Environment.Exit(0);

            }

            Logger.LogTaskComplete(LogTaskType.GetEmptyObjects, logTaskId);
            return dt;

        }

        public DataTable GetDownloadableObjects()
        {

            int logTaskId = Logger.LogTaskStart(LogTaskType.GetDownloadableObjects);

            DataTable dt = new DataTable();

            try
            {
                
                SqlDataAdapter da = new SqlDataAdapter();
                SqlCommand cmd = new SqlCommand("sfi.sfi_select_downloadable_objects", _sqlConnection);
                cmd.Transaction = _sqlTransaction;
                cmd.CommandType = CommandType.StoredProcedure;
                da.SelectCommand = cmd;
                da.Fill(dt);

            }

            catch (Exception e)
            {
                Logger.LogTaskError(LogTaskType.GetDownloadableObjects, logTaskId, e);
                Environment.Exit(0);

            }

            Logger.LogTaskComplete(LogTaskType.GetDownloadableObjects, logTaskId);
            return dt;

        }

        public DataTable GetObjectFields(string objectName, bool? download = null)
        {

            int logTaskId = Logger.LogTaskStart(LogTaskType.GetObjectFields);


            DataTable dt = new DataTable();

            try
            {
                SqlDataAdapter da = new SqlDataAdapter();
                SqlCommand cmd = new SqlCommand("sfi.sfi_select_object_fields", _sqlConnection);
                cmd.Transaction = _sqlTransaction;
                cmd.Parameters.Add(new SqlParameter("@object_name", SqlDbType.VarChar, 255));
                cmd.Parameters[0].Value = objectName;

                if (download != null)
                {
                    cmd.Parameters.Add(new SqlParameter("@download", SqlDbType.Bit));
                    cmd.Parameters[1].Value = download;
                }

                cmd.CommandType = CommandType.StoredProcedure;
                da.SelectCommand = cmd;
                da.Fill(dt);

            }
            catch (Exception e)
            {
                Logger.LogTaskError(LogTaskType.GetObjectFields, logTaskId, e);
                Environment.Exit(0);
            }

            Logger.LogTaskComplete(LogTaskType.GetObjectFields, logTaskId);
            return dt;

        }


        //save object and update meta data
        public SqlDataAdapter GetObjectsDA()
        {
            MetaDataTable SFObjectFields = new MetaDataTable("sfm_objects");
            SqlDataAdapter da = new SqlDataAdapter(SFObjectFields.GetSelect(), _sqlConnection);
            da.InsertCommand = SFObjectFields.GetInsertCommand(_sqlConnection);
            da.SelectCommand.Transaction = _sqlTransaction;
            da.InsertCommand.Transaction = _sqlTransaction;

            return da;
        }

        public SqlDataAdapter GetPicklistValuesDA()
        {
            MetaDataTable SFObjectFields = new MetaDataTable("sfm_picklist_values");
            SqlDataAdapter da = new SqlDataAdapter(SFObjectFields.GetSelect(), _sqlConnection);
            da.InsertCommand = SFObjectFields.GetInsertCommand(_sqlConnection);
            da.SelectCommand.Transaction = _sqlTransaction;
            da.InsertCommand.Transaction = _sqlTransaction;

            return da;
        }

        public SqlDataAdapter GetObjectFieldsDA()
        {
            
            SqlConnection cn = new SqlConnection(_ConnectionString);
            MetaDataTable SFObjectFields = new MetaDataTable("sfm_fields");
            SqlDataAdapter da = new SqlDataAdapter(SFObjectFields.GetSelect(), _sqlConnection);
            da.InsertCommand = SFObjectFields.GetInsertCommand(_sqlConnection);
            da.SelectCommand.Transaction = _sqlTransaction;
            da.InsertCommand.Transaction = _sqlTransaction;

            return da;
        }

        public void SaveObjects(DataTable dt)
        {

            int logTaskId = Logger.LogTaskStart(LogTaskType.UploadObjectMetadata);

            try
            {
                RunScript("TRUNCATE TABLE sfm.sfm_objects_temp");
                using (SqlDataAdapter da = GetObjectsDA())
                {
                    da.Update(dt);
                }
            }

            catch (Exception e)
            {
                Logger.LogTaskError(LogTaskType.UploadObjectMetadata, logTaskId, e);
                Environment.Exit(0);

            }

            Logger.LogTaskComplete(LogTaskType.UploadObjectMetadata, logTaskId);


        }

        public void SaveObjectFields(DataTable Fields, DataTable PicklistValues)
        {

            int logTaskId = Logger.LogTaskStart(LogTaskType.UploadFieldMetadata);


            try
            {
                RunScript("TRUNCATE TABLE sfm.sfm_fields_temp");
                RunScript("TRUNCATE TABLE sfm.sfm_picklist_values_temp");

                using (SqlDataAdapter da = GetObjectFieldsDA())
                {
                    da.Update(Fields);
                }

                using (SqlDataAdapter da = GetPicklistValuesDA())
                {
                    da.Update(PicklistValues);
                }
            }

            catch (Exception e)
            {
                Logger.LogTaskError(LogTaskType.UploadFieldMetadata, logTaskId, e);
                Environment.Exit(0);

            }

            Logger.LogTaskComplete(LogTaskType.UploadFieldMetadata, logTaskId);

        }

        public void UpdateMetaData()
        {
            int logTaskID = Logger.LogTaskStart(LogTaskType.DetectMetadataChanges);

            try
            {

                SqlCommand cmd = new SqlCommand("sfi.sfi_update_meta_data");
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@log_task_id", SqlDbType.Int);
                cmd.Parameters[0].Value = logTaskID;
                cmd.Connection = _sqlConnection;
                cmd.Transaction = _sqlTransaction;
                cmd.ExecuteNonQuery();

            }

            catch (Exception e)
            {
                Logger.LogTaskError(LogTaskType.DetectMetadataChanges, logTaskID, e);
                Environment.Exit(0);

            }

            Logger.LogTaskComplete(LogTaskType.DetectMetadataChanges, logTaskID);



        }

        public void UpdateObjectDownloadStatus(MetaDataTable mdt, bool? download, string downloadMethod, string integrationMethod, int bulkQueryBatchSize)
        {

            int logTaskId = Logger.LogTaskStart(LogTaskType.UploadObjectStatus, mdt.ObjectName);
            /*
                Objects without an is_deleted flag cannot be loaded using the incremental method
                if the incremental method is specified for this type of object it is overridden here
            */
            if (!mdt.HasIsDeleted) { integrationMethod = "Delete and Replace"; }


            try
            {
                SqlCommand cmd = new SqlCommand("sfi.sfi_update_object_download_status");
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@log_task_id", SqlDbType.Int);
                cmd.Parameters.Add("@object_name", SqlDbType.VarChar, 255);
                cmd.Parameters.Add("@cant_query", SqlDbType.Bit);
                cmd.Parameters.Add("@cant_query_error", SqlDbType.VarChar, 8000);
                cmd.Parameters.Add("@is_empty", SqlDbType.Bit);
                cmd.Parameters.Add("@download", SqlDbType.Bit);
                cmd.Parameters.Add("@download_method", SqlDbType.VarChar, 50);
                cmd.Parameters.Add("@integration_method", SqlDbType.VarChar, 50);
                cmd.Parameters.Add("@bulk_query_batch_size", SqlDbType.Int);
                cmd.Parameters[0].Value = logTaskId;
                cmd.Parameters[1].Value = mdt.ObjectName;
                cmd.Parameters[2].Value = mdt.CantQuery;
                cmd.Parameters[3].Value = mdt.CantQueryError;
                cmd.Parameters[4].Value = mdt.IsEmpty;
                cmd.Parameters[5].Value = download;
                cmd.Parameters[6].Value = downloadMethod;
                cmd.Parameters[7].Value = integrationMethod;
                cmd.Parameters[8].Value = bulkQueryBatchSize;
                cmd.Connection = _sqlConnection;
                cmd.Transaction = _sqlTransaction;
                cmd.ExecuteNonQuery();
                
            }

            catch (Exception e)
            {
                Logger.LogTaskError(LogTaskType.UploadObjectStatus, logTaskId, e);
                Environment.Exit(0);

            }

            Logger.LogTaskComplete(LogTaskType.UploadObjectStatus, logTaskId);

        }


        public bool UpdateFieldDownloadStatus(string objectName, string fieldName, bool download)
        {

            int logTaskId = Logger.LogTaskStart(LogTaskType.UpdateFieldDownloadStatus, objectName);

            int rowsAffected = 0;

            try
            {

                SqlCommand cmd = new SqlCommand("sfi.sfi_update_field_download_status");
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@log_task_id", SqlDbType.Int);
                cmd.Parameters.Add("@object_name", SqlDbType.VarChar, 255);
                cmd.Parameters.Add("@field_name", SqlDbType.VarChar, 255);
                cmd.Parameters.Add("@download", SqlDbType.Bit);
                cmd.Parameters[0].Value = logTaskId;
                cmd.Parameters[1].Value = objectName;
                cmd.Parameters[2].Value = fieldName;
                cmd.Parameters[3].Value = download;
                cmd.Connection = _sqlConnection;
                cmd.Transaction = _sqlTransaction;
                rowsAffected = cmd.ExecuteNonQuery();
            }

            catch (Exception e)
            {
                Logger.LogTaskError(LogTaskType.UpdateFieldDownloadStatus, logTaskId, e);
                Environment.Exit(0);

            }

            Logger.LogTaskComplete(LogTaskType.UpdateFieldDownloadStatus, logTaskId);
            return rowsAffected != 0;



        }


        //save object data
        public void SaveAnyObject(MetaDataTable mdt, DataTable dt)
        {

            int logTaskId = Logger.LogTaskStart(LogTaskType.UploadObjectData, mdt.ObjectName);

            try
            {
                    SqlDataAdapter da = new SqlDataAdapter(mdt.GetSelect(), _sqlConnection);
                    da.InsertCommand = mdt.GetInsertCommand(_sqlConnection);
                    da.InsertCommand.Transaction = _sqlTransaction;
                    da.SelectCommand.Transaction = _sqlTransaction;
                    da.Update(dt);
            }

            catch (Exception e)
            {
                Logger.LogTaskError(LogTaskType.UploadObjectData, logTaskId, e);
                Environment.Exit(0);

            }

            Logger.LogTaskComplete(LogTaskType.UploadObjectData, logTaskId);


        }


        //ddl functions
        public bool SchemasExist()
        {
            using (SqlConnection cn = new SqlConnection(_ConnectionString))
            {
                cn.Open();
                SqlCommand cmd = new SqlCommand("select count(*) from sys.schemas where name in ('sfd', 'sfm', 'sfi', 'sft')", cn);
                Int32 count = (Int32)cmd.ExecuteScalar();
                return count != 0;
            }
        }


        private void CreateSchema(string schemaName)
        {
            SqlCommand cmd = new SqlCommand("create schema " + schemaName, _sqlConnection);
            cmd.Transaction = _sqlTransaction;
            cmd.ExecuteNonQuery();
        }

        public bool TableExist(string schemaName, string tableName)
        {

            bool tableExists;

            SqlCommand cmd = new SqlCommand("sfi.sfi_table_exists", _sqlConnection);
            cmd.Transaction = _sqlTransaction;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(new SqlParameter("@schema_name", SqlDbType.VarChar, 50));
            cmd.Parameters.Add(new SqlParameter("@table_name", SqlDbType.VarChar, 50));
            cmd.Parameters[0].Value = schemaName;
            cmd.Parameters[1].Value = tableName;

            SqlParameter returnValue = new SqlParameter();
            returnValue.Direction = ParameterDirection.ReturnValue;
            cmd.Parameters.Add(returnValue);
            cmd.ExecuteNonQuery();

            tableExists = Convert.ToBoolean(returnValue.Value);

            return tableExists;

        }

        public void CreateTable(MetaDataTable mtd, string tableType)
        {

            

            if (!TableExist(mtd.SchemaName, mtd.TableName))
            {

                int logTaskId = Logger.LogTaskStart(LogTaskType.CreateObjectTable, mtd.ObjectName);

                try
                {
                    SqlCommand cmd = new SqlCommand(mtd.GetCreateTable(tableType), _sqlConnection);
                    cmd.Transaction = _sqlTransaction;
                    cmd.ExecuteNonQuery();
                }

                catch (Exception e)
                {
                    Logger.LogTaskError(LogTaskType.CreateObjectTable, logTaskId, e);
                    Environment.Exit(0);
                }

                Logger.LogTaskComplete(LogTaskType.CreateObjectTable, logTaskId);

            }


        }

        public void AlterTable(MetaDataTable mdt, string tableType)
        {

            if (TableExist(mdt.SchemaName, mdt.TableName))
            {
                int logTaskId = Logger.LogTaskStart(LogTaskType.AlterObjectTableFields);

                try
                {
                    SqlCommand cmd = new SqlCommand(mdt.GetAddNewField(tableType), _sqlConnection);
                    cmd.Transaction = _sqlTransaction;
                    cmd.ExecuteNonQuery();
                }

                catch (Exception e)
                {
                    Logger.LogTaskError(LogTaskType.AlterObjectTableFields, logTaskId, e);
                    Environment.Exit(0);

                }

                Logger.LogTaskComplete(LogTaskType.AlterObjectTableFields, logTaskId);

            }

        }

        public void AlterTableDatatypes(MetaDataTable mdt, string tableType)
        {

            string tableName = "";

            switch (tableType)
            {

                case "base":
                    tableName = mdt.TableName;
                    break;

                case "history":
                    tableName = mdt.TableNameHistory;
                    break;

                case "temp":
                    tableName = mdt.TableNameTemp;
                    break;

            }


            if (TableExist(mdt.SchemaName, mdt.TableName))
            {

                int logTaskId = Logger.LogTaskStart(LogTaskType.AlterObjectTableFieldType);

                try
                {

                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = _sqlConnection;
                    cmd.Transaction = _sqlTransaction;

                    foreach (SFDDataColumn dc in mdt.Fields)
                    {
                        if (dc.CanChangeDatatype())
                        {
                            string alterDatatype = "alter table " + mdt.SchemaName + '.' + tableName + " alter column " + dc.ColumnDef();
                            cmd.CommandText = alterDatatype;
                            cmd.ExecuteNonQuery();
                        }
                        else if (tableType == "base")
                        {
                            UpdateFieldDownloadStatus(mdt.ObjectName, dc.ColumnName, false);
                        }
                    }

                }
                catch (Exception e)
                {
                    Logger.LogTaskError(LogTaskType.AlterObjectTableFieldType, logTaskId, e);
                }

                Logger.LogTaskComplete(LogTaskType.AlterObjectTableFieldType, logTaskId);

            }

        }


        
        //utils
        public void RunScript(string script)
        {
                SqlCommand cmd = new SqlCommand(script, _sqlConnection);
                cmd.Transaction = _sqlTransaction;
                cmd.ExecuteNonQuery();

        }


        public void SaveSettings(bool isSandbox, string sfUsername, string sfPassword, string sfToken,
                                bool downloadNewObjects, bool downloadEmptyObjects,
                                string downloadMethod, string integrationMethod)
        {

            int logTaskId = Logger.LogTaskStart(LogTaskType.SaveInitalSettings);

            try
            {

                SqlCommand cmd = new SqlCommand("sfi.sfi_insert_settings", _sqlConnection);
                cmd.Transaction = _sqlTransaction;                
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@is_sandbox", SqlDbType.Bit);
                cmd.Parameters.Add("@sf_username", SqlDbType.VarChar, 50);
                cmd.Parameters.Add("@sf_password", SqlDbType.VarChar, 50);
                cmd.Parameters.Add("@sf_token", SqlDbType.VarChar, 50);
                cmd.Parameters.Add("@download_new_objects", SqlDbType.Bit);
                cmd.Parameters.Add("@download_empty_objects", SqlDbType.Bit);
                cmd.Parameters.Add("@download_method", SqlDbType.VarChar, 50);
                cmd.Parameters.Add("@integration_method", SqlDbType.VarChar, 50);

                cmd.Parameters[0].Value = isSandbox;
                cmd.Parameters[1].Value = sfUsername;
                cmd.Parameters[2].Value = sfPassword;
                cmd.Parameters[3].Value = sfToken;
                cmd.Parameters[4].Value = downloadNewObjects;
                cmd.Parameters[5].Value = downloadEmptyObjects;
                cmd.Parameters[6].Value = downloadMethod ?? "SOAP";
                cmd.Parameters[7].Value = integrationMethod ?? "Incremental";

                cmd.ExecuteNonQuery();

                
            }

            catch (Exception e)
            {
                Logger.LogTaskError(LogTaskType.SaveInitalSettings, logTaskId, e);
                Environment.Exit(0);
            }

            Logger.LogTaskComplete(LogTaskType.SaveInitalSettings, logTaskId);

        }


        //logging
        public int LogExecutionStart(string action_code)
        {


            using (SqlConnection cn = new SqlConnection(_ConnectionString))
            {
                cn.Open();
                SqlCommand cmd = new SqlCommand("sfi.sfi_insert_log_execution", cn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@action_code", SqlDbType.VarChar, 20, "action_code");
                cmd.Parameters.Add("@started_on", SqlDbType.DateTime, 0, "started_on");
                cmd.Parameters.Add("@completed_on", SqlDbType.DateTime, 0, "completed_on");
                cmd.Parameters.Add("@username", SqlDbType.VarChar, 50, "username");
                var p = cmd.Parameters.Add("@log_execution_id", SqlDbType.Int, 0, "log_execution_id");
                p.Direction = ParameterDirection.Output;

                cmd.Parameters[0].Value = action_code;
                cmd.Parameters[1].Value = DateTime.Now;
                cmd.Parameters[2].Value = DBNull.Value;
                cmd.Parameters[3].Value = Environment.UserName;

                cmd.ExecuteNonQuery();
                _LogExecutionId = (int)cmd.Parameters["@log_execution_id"].Value;

            }

            return (_LogExecutionId);

        }

        public void LogExecutionComplete(bool? ranToCompletion = null, string errorText = null)
        {

            using (SqlConnection cn = new SqlConnection(_ConnectionString))
            {
                cn.Open();
                DataTable dt = new DataTable("sfi_log_execution");
                SqlDataAdapter da = new SqlDataAdapter("select * from sfi.sfi_log_execution", cn);

                da.UpdateCommand = new SqlCommand("sfi.sfi_update_log_execution", cn);
                da.UpdateCommand.CommandType = CommandType.StoredProcedure;
                da.UpdateCommand.Parameters.Add("@started_on", SqlDbType.DateTime, 0, "started_on");
                da.UpdateCommand.Parameters.Add("@completed_on", SqlDbType.DateTime, 0, "completed_on");
                da.UpdateCommand.Parameters.Add("@username", SqlDbType.VarChar, 50, "username");
                da.UpdateCommand.Parameters.Add("@log_execution_id", SqlDbType.VarChar, 50, "log_execution_id");
                da.UpdateCommand.Parameters.Add("@ran_to_completion", SqlDbType.Bit, 0, "ran_to_completion");
                da.UpdateCommand.Parameters.Add("@error_text", SqlDbType.VarChar, 8000, "error_text");


                da.Fill(dt);
                DataRow[] dr = dt.Select("log_execution_id = " + _LogExecutionId.ToString());
                dr[0]["completed_on"] = DateTime.Now;
                dr[0]["ran_to_completion"] = ranToCompletion;

                da.Update(dt);

                cn.Close();
                cn.Dispose();

            }


        }

        public int LogTaskStart(string Task, string taskNotes = null, int? objectId = null, string objectName = null
                                , string sfDownLoadMethod = null, string integrationMethod = null
                                , string sfJobId = null, string sfBatchId = null)
        {

            int logTaskId;

            using (SqlConnection cn = new SqlConnection(_ConnectionString))
            {
                cn.Open();
                SqlCommand cmd = new SqlCommand("sfi.sfi_insert_log_task", cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@log_execution_id", SqlDbType.Int);
                cmd.Parameters.Add("@task_notes", SqlDbType.VarChar, 50);
                cmd.Parameters.Add("@task", SqlDbType.VarChar, 50);
                cmd.Parameters.Add("@object_id", SqlDbType.Int);
                cmd.Parameters.Add("@object_name", SqlDbType.VarChar, 255);
                cmd.Parameters.Add("@download_method", SqlDbType.VarChar, 50);
                cmd.Parameters.Add("@integration_method", SqlDbType.VarChar, 50);
                cmd.Parameters.Add("@sf_job_id", SqlDbType.VarChar, 50);
                cmd.Parameters.Add("@sf_batch_id", SqlDbType.VarChar, 50);
                cmd.Parameters.Add("@started_on", SqlDbType.DateTime);
                cmd.Parameters.Add("@completed_on", SqlDbType.DateTime);
                cmd.Parameters.Add("@error_text", SqlDbType.VarChar, 8000);
                cmd.Parameters.Add("@record_count", SqlDbType.Int);
                var p = cmd.Parameters.Add("@log_task_id", SqlDbType.Int, 0, "log_task_id");
                p.Direction = ParameterDirection.Output;

                cmd.Parameters[0].Value = _LogExecutionId;
                cmd.Parameters[1].Value = taskNotes ?? (object)DBNull.Value;
                cmd.Parameters[2].Value = Task ?? (object)DBNull.Value;
                cmd.Parameters[3].Value = objectId ?? (object)DBNull.Value;
                cmd.Parameters[4].Value = objectName ?? (object)DBNull.Value;
                cmd.Parameters[5].Value = sfDownLoadMethod ?? (object)DBNull.Value;
                cmd.Parameters[6].Value = integrationMethod ?? (object)DBNull.Value;
                cmd.Parameters[7].Value = sfJobId ?? (object)DBNull.Value;
                cmd.Parameters[8].Value = sfBatchId ?? (object)DBNull.Value;
                cmd.Parameters[9].Value = DateTime.Now;

                cmd.ExecuteNonQuery();
                logTaskId = (int)cmd.Parameters["@log_task_id"].Value;

            }



            return logTaskId;

        }

        public void LogTaskComplete(int LogTaskId, string errorText = null, int? recordCount = null)
        {

            using (SqlConnection cn = new SqlConnection(_ConnectionString))
            {

                cn.Open();
                SqlCommand cmd = new SqlCommand("sfi.sfi_log_task_complete", cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@completed_on", SqlDbType.DateTime);
                cmd.Parameters.Add("@error_text", SqlDbType.VarChar, 8000);
                cmd.Parameters.Add("@record_count", SqlDbType.Int);
                cmd.Parameters.Add("@log_task_id", SqlDbType.Int);

                cmd.Parameters[0].Value = DateTime.Now;
                cmd.Parameters[1].Value = errorText ?? (object)DBNull.Value;
                cmd.Parameters[2].Value = recordCount ?? (object)DBNull.Value;
                cmd.Parameters[3].Value = LogTaskId;

                cmd.ExecuteNonQuery();

            }
        }



    }

}

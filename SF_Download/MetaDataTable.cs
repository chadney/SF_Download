
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

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace SF_Download
{
    public class MetaDataTable
    {

        public List<SFDDataColumn> Fields { get; set; }
        public string SchemaName { get; set; }
        public string TableName { get; set; }
        public string TableNameHistory { get; set; }
        public string TableNameTemp { get; set; }

        public string ObjectName { get; set; }
        public DataTable SourceData { get; set; }
        public BulkQueryJob BulkQueryJob { get; set; }

        public bool CantQuery { get; set; }
        public string CantQueryError { get; set; }
        public int RecordCount { get; set; }
        public bool HasIsDeleted { get; set; }
        public bool IsEmpty { get; set; }

        public bool Download { get; set; }
        public string DownloadMethod { get; set; }
        public string IntegrationMethod { get; set; }
        public int BulkQueryBatchSize { get; set; }
        public DateTime LastDownloadOn { get; set; }

        //constructors


        public MetaDataTable()
        {

        }

        public MetaDataTable(string TableName)
        {
            SchemaName = "sfm";
            this.TableName = TableName;
            this.TableNameHistory = this.TableName + "_history";
            this.TableNameTemp = this.TableName + "_temp";

            Fields = new List<SFDDataColumn>();
            SourceData = null;

            if (TableName == "sfm_picklist_values")
            {
                Fields.Add(new SFDDataColumn("object_name", typeof(string), SqlDbType.VarChar, 255));
                Fields.Add(new SFDDataColumn("field_name", typeof(string), SqlDbType.VarChar, 255));
                Fields.Add(new SFDDataColumn("value", typeof(string), SqlDbType.VarChar, 255));
                Fields.Add(new SFDDataColumn("label", typeof(string), SqlDbType.VarChar, 255));
                Fields.Add(new SFDDataColumn("active", typeof(bool), SqlDbType.Bit, 1));
                Fields.Add(new SFDDataColumn("default_value", typeof(bool), SqlDbType.Bit, 1));
            }

            if (TableName == "sfm_objects")
            {
                Fields.Add(new SFDDataColumn("key_prefix", typeof(string), SqlDbType.VarChar, 255));
                Fields.Add(new SFDDataColumn("name", typeof(string), SqlDbType.VarChar, 255));
                Fields.Add(new SFDDataColumn("label", typeof(string), SqlDbType.VarChar, 255));
                Fields.Add(new SFDDataColumn("is_custom", typeof(bool), SqlDbType.Bit, 1));
                Fields.Add(new SFDDataColumn("is_custom_setting", typeof(bool), SqlDbType.Bit, 1));
                Fields.Add(new SFDDataColumn("is_queryable", typeof(bool), SqlDbType.Bit, 1));
                Fields.Add(new SFDDataColumn("cant_query", typeof(bool), SqlDbType.Bit, 1));
                Fields.Add(new SFDDataColumn("cant_query_error", typeof(string), SqlDbType.VarChar, 8000));
                Fields.Add(new SFDDataColumn("is_empty", typeof(bool), SqlDbType.Bit, 1));
                Fields.Add(new SFDDataColumn("download", typeof(bool), SqlDbType.Bit, 1));
                Fields.Add(new SFDDataColumn("download_method", typeof(int), SqlDbType.Int, 1));
                Fields.Add(new SFDDataColumn("integration_method", typeof(int), SqlDbType.Int, 1));
                Fields.Add(new SFDDataColumn("bulk_query_batch_size", typeof(int), SqlDbType.Int, 1));

            }

            if (TableName == "sfm_fields")
            {
                Fields.Add(new SFDDataColumn("object_name", typeof(string), SqlDbType.VarChar, 255));
                Fields.Add(new SFDDataColumn("name", typeof(string), SqlDbType.VarChar, 255));
                Fields.Add(new SFDDataColumn("label", typeof(string), SqlDbType.VarChar, 255));
                Fields.Add(new SFDDataColumn("soap_type", typeof(string), SqlDbType.VarChar, 255));
                Fields.Add(new SFDDataColumn("type", typeof(string), SqlDbType.VarChar, 255));
                Fields.Add(new SFDDataColumn("length", typeof(int), SqlDbType.Int, 4));
                Fields.Add(new SFDDataColumn("byte_length", typeof(int), SqlDbType.Int, 4));
                Fields.Add(new SFDDataColumn("digits", typeof(int), SqlDbType.Int, 4));
                Fields.Add(new SFDDataColumn("precision", typeof(int), SqlDbType.Int, 4));
                Fields.Add(new SFDDataColumn("scale", typeof(int), SqlDbType.Int, 4));
                Fields.Add(new SFDDataColumn("is_autonumber", typeof(bool), SqlDbType.Bit, 1));
                Fields.Add(new SFDDataColumn("is_calculated", typeof(bool), SqlDbType.Bit, 1));
                Fields.Add(new SFDDataColumn("is_case_sensitive", typeof(bool), SqlDbType.Bit, 1));
                Fields.Add(new SFDDataColumn("is_custom", typeof(bool), SqlDbType.Bit, 1));
                Fields.Add(new SFDDataColumn("is_dependent_pick_list", typeof(bool), SqlDbType.Bit, 1));
                Fields.Add(new SFDDataColumn("is_external_id", typeof(bool), SqlDbType.Bit, 1));
                Fields.Add(new SFDDataColumn("is_ID_lookup", typeof(bool), SqlDbType.Bit, 1));
                Fields.Add(new SFDDataColumn("is_name_field", typeof(bool), SqlDbType.Bit, 1));
                Fields.Add(new SFDDataColumn("is_unique", typeof(bool), SqlDbType.Bit, 1));
                Fields.Add(new SFDDataColumn("is_restricted_picklist", typeof(bool), SqlDbType.Bit, 1));
                Fields.Add(new SFDDataColumn("download", typeof(bool), SqlDbType.Bit, 1));
            }

        }

        public MetaDataTable(string objectName, DataTable dtFields)
        {

            SchemaName = "sfd";
            this.ObjectName = objectName;
            this.TableName = SchemaName + "_" + ObjectName;
            this.TableNameHistory = this.TableName + "_history";
            this.TableNameTemp = this.TableName + "_temp";
            Fields = new List<SFDDataColumn>();
            SourceData = null;
            bool hasPreviousDatatypes = dtFields.Columns.Contains("type_previous");
            HasIsDeleted = false;

            foreach (DataRow dr in dtFields.Rows)
            {

                string type = (string) dr["type"];
                string name = (string) dr["name"];
                int precision = (int) dr["precision"];
                int scale = (int) dr["scale"];
                int length = (int) dr["length"];

                SFDDataColumn dc = MapSFDDataColumn(type, name, precision, scale, length);
                SFDDataColumn dcp;
                
                if (hasPreviousDatatypes && dc != null)
                {
                    string typePrevious = (string)dr["type_previous"];
                    int precisionPrevious = (int)dr["precision_previous"];
                    int scalePrevious = (int)dr["scale_previous"];
                    int lengthPrevious = (int)dr["length_previous"];
                    
                    dcp = MapSFDDataColumn(typePrevious, name, precisionPrevious, scalePrevious, lengthPrevious);
                    dc.PreviousLength = dcp.Length;
                    dc.PreviousPrecision= dcp.Precision;
                    dc.PreviousScale = dcp.Scale;
                    dc.PreviousSqlDbType = dcp.SqlDbType;

                }


                if (dc != null)
                {
                    Fields.Add(dc);
                    if ((string)dr["name"] == "IsDeleted") { HasIsDeleted = true; }
                }
            }


        }

        public bool GetSettings(SFDTarget target)
        {
            DataTable objectSettings = target.GetObjectSettings(ObjectName);

            if (objectSettings.Rows.Count > 0 ) {


                CantQuery = (bool) objectSettings.Rows[0]["cant_query"];
                CantQueryError = objectSettings.Rows[0]["cant_query_error"] == DBNull.Value ? null : (string) objectSettings.Rows[0]["cant_query_error"];
                IsEmpty = (bool) objectSettings.Rows[0]["is_empty"];
                Download = (bool) objectSettings.Rows[0]["download"];
                DownloadMethod = (string) objectSettings.Rows[0]["download_method"];
                IntegrationMethod = (string) objectSettings.Rows[0]["integration_method"];



                return true;

            }
            else
            {
                return false;

            }

        }
        

        private SFDDataColumn MapSFDDataColumn(string type, string name, int precision, int scale, int length)
        {
            SFDDataColumn dc = null;

            switch (type) {

                case "boolean":
                    dc = new SFDDataColumn(name, typeof(bool), SqlDbType.Bit, 1);
                    break;

                case "date":
                    dc = new SFDDataColumn(name, typeof(DateTime), SqlDbType.Date, 4);
                    break;

                case "datetime":
                    dc = new SFDDataColumn(name, typeof(DateTime), SqlDbType.DateTime, 8);
                    break;

                case "percent":
                    dc = new SFDDataColumn(name, typeof(double), SqlDbType.Decimal, 17);
                    dc.Precision = precision;
                    dc.Scale = scale;
                    break;

                case "currency":
                    dc = new SFDDataColumn(name, typeof(double), SqlDbType.Decimal, 17);
                    dc.Precision = precision;
                    dc.Scale = scale;
                    break;

                case "double":
                    dc = new SFDDataColumn(name, typeof(double), SqlDbType.Decimal, 17);
                    dc.Precision = precision;
                    dc.Scale = scale;
                    break;

                case "int":
                    dc = new SFDDataColumn(name, typeof(int), SqlDbType.Int, 4);
                    break;

                case "time":
                    dc = new SFDDataColumn(name, typeof(TimeSpan), SqlDbType.Time, 4);
                    break;

                case "textarea":
                    dc = new SFDDataColumn(name, typeof(string), SqlDbType.VarChar, length);
                    break;

                case "phone":
                    dc = new SFDDataColumn(name, typeof(string), SqlDbType.VarChar, length);
                    break;

                case "id":
                    dc = new SFDDataColumn(name, typeof(string), SqlDbType.VarChar, 18);
                    break;

                case "multipicklist":
                    dc = new SFDDataColumn(name, typeof(string), SqlDbType.VarChar, length);
                    break;

                case "combobox":
                    dc = new SFDDataColumn(name, typeof(string), SqlDbType.VarChar, length);
                    break;

                case "picklist":
                    dc = new SFDDataColumn(name, typeof(string), SqlDbType.VarChar, length);
                    break;

                case "url":
                    dc = new SFDDataColumn(name, typeof(string), SqlDbType.VarChar, length);
                    break;

                case "reference":
                    dc = new SFDDataColumn(name, typeof(string), SqlDbType.VarChar, 18);
                    break;

                case "encryptedstring":
                    dc = new SFDDataColumn(name, typeof(string), SqlDbType.VarChar, length);
                    break;

                case "email":
                    dc = new SFDDataColumn(name, typeof(string), SqlDbType.VarChar, length);
                    break;

                case "string":
                    dc = new SFDDataColumn(name, typeof(string), SqlDbType.VarChar, length);
                    break;

                case "address":
                    //compound field, deal how?
                    break;

                default:
                    break;


            }

            return dc;

        }

        public DataTable GetDataTable()
        {

            DataTable dt = new DataTable(TableName);
            foreach (SFDDataColumn dc in Fields)
            {
                dt.Columns.Add(dc);
            }

            return dt;

        }

        public string GetSoqlSelect()
        {

            string columns = "";

            foreach (SFDDataColumn dc in Fields)
            {
                columns += dc.ColumnName + Environment.NewLine + ",";
            }

            columns = columns.TrimEnd(',');
            string select = "SELECT " + columns + "FROM " + ObjectName;

            return select;

        }

        public string GetDeleteFromBase(int log_task_id)
        {
            string var1 = "declare @new_record_change_type_id int = (select change_type_id from sfi.sfi_lookup_change_types where name = 'New Record') ";
            string var2 = "declare @delete_record_change_type_id int = (select change_type_id from sfi.sfi_lookup_change_types where name = 'Delete Record') ";
            string var3 = "declare @update_record_change_type_id int = (select change_type_id from sfi.sfi_lookup_change_types where name = 'Update Record') ";
            string var4 = "declare @log_task_id int = " + log_task_id;
            string vars = var1 + Environment.NewLine + var2 + Environment.NewLine + var3 + Environment.NewLine + var4 + Environment.NewLine;

            string delete = "delete from " + SchemaName + "." + TableName + " ";
            string where = "where id in (select  id " +
                           "from    " + SchemaName + "." + TableNameHistory + " h " +
                           "where h.log_task_id = " + log_task_id + " " +
                           "and h.change_type_id in (@delete_record_change_type_id, @update_record_change_type_id) )";

            string script = vars + delete + where;
            return script;

        }

        public string GetHistoryToBase(int log_task_id)
        {
            string var1 = "declare @new_record_change_type_id int = (select change_type_id from sfi.sfi_lookup_change_types where name = 'New Record') ";
            string var2 = "declare @delete_record_change_type_id int = (select change_type_id from sfi.sfi_lookup_change_types where name = 'Delete Record') ";
            string var3 = "declare @update_record_change_type_id int = (select change_type_id from sfi.sfi_lookup_change_types where name = 'Update Record') ";
            string var4 = "declare @log_task_id int = " + log_task_id;
            string vars = var1 + Environment.NewLine + var2 + Environment.NewLine + var3 + Environment.NewLine + var4 + Environment.NewLine;

            string insert = "insert into " + SchemaName + "." + TableName + " ";
            string columnsInsert = "";
            string columnsSelect = "";
            foreach (SFDDataColumn dc in Fields)
            {
                columnsSelect += "h." + dc.ColumnName + Environment.NewLine + ",";
                columnsInsert += dc.ColumnName + Environment.NewLine + ",";
            }

            columnsInsert = columnsInsert.TrimEnd(',');
            columnsSelect = columnsSelect.TrimEnd(',');

            string fromClause = "from " + SchemaName + "." + TableNameHistory + " h " +
                                "where h.log_task_id = @log_task_id " +
                                "and h.change_type_id in (@new_record_change_type_id, @update_record_change_type_id)";

            string script = vars + insert + "(" + columnsInsert + ")" + "select " + columnsSelect + " " + fromClause;
            return script;

        }

        public string GetTempToHistory(int log_task_id)
        {

            string var1 = "declare @new_record_change_type_id int = (select change_type_id from sfi.sfi_lookup_change_types where name = 'New Record') ";
            string var2 = "declare @delete_record_change_type_id int = (select change_type_id from sfi.sfi_lookup_change_types where name = 'Delete Record') ";
            string var3 = "declare @update_record_change_type_id int = (select change_type_id from sfi.sfi_lookup_change_types where name = 'Update Record') ";
            string var4 = "declare @log_task_id int = " + log_task_id;
            string vars = var1 + Environment.NewLine + var2 + Environment.NewLine + var3 + Environment.NewLine + var4 + Environment.NewLine;

            string insert = "insert into " + SchemaName + "." + TableNameHistory + " ";
            string logFields = "log_task_id, change_type_id, ";
            string selectLogFields = "  @log_task_id " +
                                     "  ,   case " +
                                     "          when t.IsDeleted = 1 then @delete_record_change_type_id " +
                                     "          when b.id is null then @new_record_change_type_id " +
                                     "          else @update_record_change_type_id " +
                                     "      end change_type_id, ";
            string fromClause = " from    " + SchemaName + "." + TableNameTemp + " t " +
                          "  left join " + SchemaName + "." + TableName + " b " +
                          " on t.Id = b.Id";

            string columnsInsert = "";
            string columnsSelect = "";

            foreach (SFDDataColumn dc in Fields)
            {
                columnsSelect += "t." + dc.ColumnName + Environment.NewLine + ",";
                columnsInsert += dc.ColumnName + Environment.NewLine + ",";
            }

            columnsInsert = columnsInsert.TrimEnd(',');
            columnsSelect = columnsSelect.TrimEnd(',');

            string script = vars + insert + "(" + logFields + columnsInsert + ")" + "select " + selectLogFields + columnsSelect + " " + fromClause;
            return script;
        }

        public string GetTempToBase()
        {
            string script = "truncate table " + SchemaName + "." + TableName + ";";
            script = script += Environment.NewLine;
            script = script += "insert into " + SchemaName + "." + TableName + " ";
            script = script += "select * from " + SchemaName + "." + TableNameTemp + ";";

            return script;
            
        }


        public string GetCreateTable(string tableType)
        {
            string historyColumns = "";

            string tableName = "";

            switch (tableType) {

                case "base":
                    tableName = TableName;
                    break;

                case "history":
                    tableName = TableNameHistory;
                    historyColumns = "history_id int identity(1,1) primary key clustered " +
                                        ", log_task_id int " +
                                        ", change_type_id int, ";
                    break;

                case "temp":
                    tableName = TableNameTemp;
                    break;

            }

            string columns = "";

            foreach (SFDDataColumn dc in Fields)
            {
                columns += dc.ColumnDef() + Environment.NewLine + ",";
            }

            columns = columns.TrimEnd(',');

            string create = "CREATE TABLE " + SchemaName + "." + tableName + " (" + historyColumns + columns + ")";

            return create;
        }

        public string GetAddNewField(string tableType)
        {

            string tableName = "";

            switch (tableType)
            {

                case "base":
                    tableName = TableName;
                    break;

                case "history":
                    tableName = TableNameHistory;
                    break;

                case "temp":
                    tableName = TableNameTemp;
                    break;

            }

            string addNewField = "alter table " + SchemaName + '.' + tableName + " ";
            string columns = "add ";


            foreach (SFDDataColumn dc in Fields)
            {
                    columns += dc.ColumnDef() + Environment.NewLine + ",";
            }

            columns = columns.TrimEnd(',');

            addNewField += columns;

            return addNewField;

        }




        public string GetSelect()
        {
            string select = "select * from " + SchemaName + "." + TableNameTemp;
            return select;
        }

        public string GetInsert()

        {
            string columns = "";
            string values = "";

            foreach (SFDDataColumn dc in Fields)
            {
                columns += dc.ColumnName + ",";
                values += '@' + dc.ColumnName + ",";
            }

            columns = columns.TrimEnd(',');
            values = values.TrimEnd(',');

            string insert = "INSERT INTO " + SchemaName + "." + TableNameTemp + " (" + columns + ") VALUES (" + values + ")";

            return insert;
        }

        public SqlCommand GetInsertCommand(SqlConnection cn)
        {
            SqlCommand insertCmd = new SqlCommand(GetInsert(), cn);

            foreach (SFDDataColumn dc in Fields)
            {
                insertCmd.Parameters.Add("@" + dc.ColumnName, dc.SqlDbType, dc.Length, dc.ColumnName);
            }


            return insertCmd;
        }


        public string CalculateDownloadMethod()
        {

            /* 
             * Since delete records cannot be download by bulk it's risky to allow 
             * Automatic calculation of download method so this is removed.
             * Leaving the method in for future use but currently it does nothing
             * but return the property value of DownloadMethod
             */

            
            /*
            if (DownloadMethod == "Automatic")
            {
                switch (RecordCount)
                {
                    case var _ when(RecordCount < 10000000):
                        return "SOAP";

                    case var _ when (RecordCount < 0):
                        return "Bulk Query";

                    case var _ when (RecordCount < 0):
                        return "Bulk Query Batched";

                    default:
                        return "SOAP";

                }

            }
            else
            {
                return DownloadMethod;
            }

            */

            return DownloadMethod;


        }

    }

}

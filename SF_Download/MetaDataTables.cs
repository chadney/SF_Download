
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
using System.Linq;
using System.Threading.Tasks;

namespace SF_Download
{
    class MetaDataTables
    {

        public List<MetaDataTable> Tables { get; set; }
        private SFDSource _Source;
        private SFDTarget _Target;

        public MetaDataTables(SFDSource source, SFDTarget target)
        {
            Tables = new List<MetaDataTable>();
            _Source = source;
            _Target = target;

        }

        public void AddNewFieldObjects()
        {

            DataTable changedFields = _Target.GetNewFields(_Target.LogExecutionId);
            IEnumerable<string> changedFieldsEnumerable = changedFields.AsEnumerable()
                .Select<DataRow, string>(row => row.Field<string>("object_name"))
                .Distinct();

            foreach (string objectName in changedFieldsEnumerable)
            {

                DataTable fields = changedFields.Select("object_name = '" + objectName + "'").CopyToDataTable();
                //use this to generate a meta datatable with the structure of the object
                MetaDataTable mdt = new MetaDataTable (objectName, fields);
                Tables.Add(mdt);
            }


        }


        public void AddChangedFieldDatatypeObjects()
        {

            DataTable changedFields = _Target.GetChangedDatatypeFields(_Target.LogExecutionId);
            IEnumerable<string> changedFieldsEnumerable = changedFields.AsEnumerable()
                .Select<DataRow, string>(row => row.Field<string>("object_name"))
                .Distinct();
             
            foreach (string objectName in changedFieldsEnumerable)
            {

                DataTable fields = changedFields.Select("object_name = '" + objectName + "'").CopyToDataTable();
                //use this to generate a meta datatable with the structure of the object
                MetaDataTable mdt = new MetaDataTable(objectName, fields);
                Tables.Add(mdt);
            }


        }
        public MetaDataTables(SFDSource source, SFDTarget target, DataTable objects, bool hasLastDownloadOn )
        {
            Tables = new List<MetaDataTable>();
            _Source = source;
            _Target = target;

            foreach (DataRow obj in objects.Rows)
            {
                //get a datatable of all object fields for object name
                string objectName = (string)obj["name"];
                //use this to generate a meta datatable with the structure of the object
                MetaDataTable mdt = new MetaDataTable(objectName, target.GetObjectFields(objectName, true));
                //TODO - this can be replaced by mdt.GetSettings?
                mdt.DownloadMethod = (string) (obj["download_method"] == DBNull.Value ? null : obj["download_method"]);
                mdt.IntegrationMethod = (string) (obj["integration_method"] == DBNull.Value ? null : obj["integration_method"]);
                mdt.BulkQueryBatchSize = (int) (obj["bulk_query_batch_size"] == DBNull.Value ? 0 : obj["bulk_query_batch_size"]);

                if (hasLastDownloadOn) { mdt.LastDownloadOn = (DateTime)(obj["last_download_on"] == DBNull.Value ? DateTime.MinValue : obj["last_download_on"]); }

                Tables.Add(mdt);
            }

        }


        public Task<MetaDataTable>[] GetCountTasks()
        {
            List<Task<MetaDataTable>> listTasks = new List<Task<MetaDataTable>>();

            foreach (MetaDataTable mdt in Tables)
            {
                Task<MetaDataTable> getDataTask = _Source.GetCountAsync(mdt);
                listTasks.Add(getDataTask);
            }

            Task<MetaDataTable>[] arrayTasks = listTasks.ToArray();
            return arrayTasks;

        }


        public Task<MetaDataTable>[] GetDownloadTasks()
        {
            List<Task<MetaDataTable>> listTasks = new List<Task<MetaDataTable>>();

            foreach (MetaDataTable mdt in Tables)
            {
                switch (mdt.CalculateDownloadMethod())
                {
                    case "SOAP":
                        listTasks.Add(_Source.GetDataAsync(mdt, mdt.LastDownloadOn));
                        break;

                    case "Bulk Query":
                        listTasks.Add(_Source.BulkGetDataByBatch(mdt, mdt.LastDownloadOn, false));
                        break;

                    case "Bulk Query Batched":
                        listTasks.Add(_Source.BulkGetDataByBatch(mdt, mdt.LastDownloadOn, true));
                        break;

                }
            }

            Task<MetaDataTable>[] arrayTasks = listTasks.ToArray();
            return arrayTasks;

        }


        public void AddNewFields()
        {
            foreach (MetaDataTable mdt in Tables)
            {
                _Target.AlterTable(mdt, "base");
                _Target.AlterTable(mdt, "history");
                _Target.AlterTable(mdt, "temp");
            }

        }

        public void AlterFieldDatatypes()
        {

            foreach (MetaDataTable mdt in Tables)
            {
                _Target.AlterTableDatatypes(mdt, "base");
                _Target.AlterTableDatatypes(mdt, "history");
                _Target.AlterTableDatatypes(mdt, "temp");
            }

        }




    }
}

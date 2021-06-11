using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SF_Download
{
    using SFP = sf_partner;

    public class SFDSource
    {
        //fields and properties
        #region
        //fields
        private SFP.SessionHeader SFSessionHeader;
        private EndpointAddress SFEndpointAddress;
        private SFP.SoapClient SFSoapClient;
        private string SFBulkBaseUrl;


        //properties
        public XNamespace bulkXmlNs { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string Token { get; set; }
        public bool Sandbox { get; set; } 
        public Logger Logger { get; set; }

        private Boolean _isConnectionOK;
        public bool isConnectionOK { get { return _isConnectionOK; } }

        private Exception _ConnectionError;
        public Exception ConnectionError { get { return _ConnectionError; } }

        private SFDTarget _Target;

        private DataTable _Objects;
        public DataTable Objects {
            get
            {
                if (_Objects == null)
                {
                    _Objects = GetObjects();
                }
                return _Objects;
            }

        }

        private DataTable _ObjectsFields;
        public DataTable ObjectFields
        {
            get
            {
                if (_ObjectsFields == null)
                {
                    Dictionary<string, DataTable> objectFields = GetObjectFields();
                    _ObjectsFields = objectFields["SFObjectFields"];
                    _PicklistValues = objectFields["SFObjectPicklistValues"];
                }
                return _ObjectsFields;
            }

        }

        private DataTable _PicklistValues;
        public DataTable PicklistValues
        {
            get
            {
                if (_PicklistValues == null)
                {
                    _PicklistValues = GetObjectFields()["SFObjectPicklistValues"];
                }
                return _PicklistValues;
            }

        }


        #endregion


        //constructor
        public SFDSource(string User, string Password, string Token, bool Sandbox, SFDTarget Target)
        {
            bulkXmlNs = "http://www.force.com/2009/06/asyncapi/dataload";
            this.User = User;
            this.Password = Password;
            this.Token = Token;
            this.Sandbox = Sandbox;
            _Target = Target;

        }


        //connection methods
        public void TestConnection()
        {

            int logTaskId = Logger.LogTaskStart(LogTaskType.TestSFConnection);

            try
            {
                SFP.SoapClient sc = new SFP.SoapClient();
                SFP.LoginResult lr = sc.login(null, null, User, Password + Token);
                _isConnectionOK = true; //deal with password expired case
                sc.Close();
            }
            catch (Exception e)
            {
                Logger.LogTaskError(LogTaskType.TestSFConnection, logTaskId, e);
                Environment.Exit(0);

            }

            Logger.LogTaskComplete(LogTaskType.TestSFConnection, logTaskId);

        }

        public void Connect()
        {

            int logTaskId = Logger.LogTaskStart(LogTaskType.ConnecttoSF);

            try
            {
                SFP.SoapClient sc = new SFP.SoapClient();
                SFP.LoginResult lr = sc.login(null, null, User, Password + Token);
                _isConnectionOK = true;
                SFEndpointAddress = new EndpointAddress(lr.serverUrl);
                SFSessionHeader = new SFP.SessionHeader();
                SFSessionHeader.sessionId = lr.sessionId;
                sc.Close();
                SFSoapClient = new SFP.SoapClient("Soap", SFEndpointAddress);
                SFBulkBaseUrl = lr.serverUrl.Substring(0, lr.serverUrl.IndexOf("Soap/")) + "async/40.0/";

            }
            catch (Exception e)
            {
                Logger.LogTaskError(LogTaskType.ConnecttoSF, logTaskId, e);
                Environment.Exit(0);

            }

            _Target.LogTaskComplete(logTaskId);

        }


        //meta data methods
        private DataTable GetObjects()
        {


            int logTaskId = Logger.LogTaskStart(LogTaskType.DownloadObjectMetadata);
            DataTable SFObjects = new MetaDataTable("sfm_objects").GetDataTable();

            try
            {

                DataRow dr = SFObjects.NewRow();
                SFP.DescribeGlobalResult dgr = null;
                SFP.LimitInfo[] li;
                li = SFSoapClient.describeGlobal(SFSessionHeader, null, null, out dgr);


                for (int i = 0; i < dgr.sobjects.Count(); i++)
                {

                    dr["key_prefix"] = dgr.sobjects[i].keyPrefix;
                    dr["name"] = dgr.sobjects[i].name;
                    dr["label"] = dgr.sobjects[i].label;
                    dr["is_custom"] = dgr.sobjects[i].custom;
                    dr["is_custom_setting"] = dgr.sobjects[i].customSetting;
                    dr["is_queryable"] = dgr.sobjects[i].queryable;

                    SFObjects.Rows.Add(dr);
                    dr = SFObjects.NewRow();

                }

            }

            catch(Exception e)
            {
                Logger.LogTaskError(LogTaskType.DownloadObjectMetadata, logTaskId, e);
                Environment.Exit(0);
                
            }

            Logger.LogTaskComplete(LogTaskType.DownloadObjectMetadata, logTaskId);
            return SFObjects;

        }

        private Dictionary<string, DataTable> GetObjectFields()
        {


            int logTaskId = Logger.LogTaskStart(LogTaskType.DownloadFieldMetadata);

            DataTable SFObjectFields = new MetaDataTable("sfm_fields").GetDataTable();
            DataRow fdr = SFObjectFields.NewRow();
            DataTable SFObjectPicklistValues = new MetaDataTable("sfm_picklist_values").GetDataTable();
            DataRow pdr = SFObjectPicklistValues.NewRow();

            try
            {

                //get all objects
                DataTable obs = Objects;
                List<string[]> objLists = new List<string[]>();
                string[] objList = new string[100];

                int btcCount = 1;
                int objCount = 0;
                foreach (DataRow dr in obs.Rows)
                {
                    if (objCount == 100)
                    {
                        objLists.Add(objList);
                        objCount = 0;
                        btcCount = btcCount + 1;
                        objList = new string[100];
                    }

                    objList[objCount] = (string)dr["name"];
                    objCount++;

                }

                objLists.Add(objList);

                for (int i = 0; i < btcCount; i++)
                {
                    SFP.DescribeSObjectResult[] dor = null;
                    SFP.LimitInfo[] li;
                    li = SFSoapClient.describeSObjects(SFSessionHeader, null, null, null, objLists[i], out dor);

                    for (int j = 0; j < dor.Count(); j++)
                    {
                        for (int k = 0; k < dor[j].fields.Count(); k++)
                        {

                            if (dor[j].fields[k].picklistValues != null)
                            {
                                for (int l = 0; l < dor[j].fields[k].picklistValues.Count(); l++)
                                {
                                    pdr["object_name"] = dor[j].name;
                                    pdr["field_name"] = dor[j].fields[k].name;
                                    pdr["value"] = dor[j].fields[k].picklistValues[l].value;
                                    pdr["label"] = dor[j].fields[k].picklistValues[l].label;
                                    pdr["active"] = dor[j].fields[k].picklistValues[l].active;
                                    pdr["default_value"] = dor[j].fields[k].picklistValues[l].defaultValue;
                                    SFObjectPicklistValues.Rows.Add(pdr);
                                    pdr = SFObjectPicklistValues.NewRow();
                                }
                            }


                            fdr["object_name"] = dor[j].name;
                            fdr["name"] = dor[j].fields[k].name;
                            fdr["label"] = dor[j].fields[k].label;
                            fdr["soap_type"] = dor[j].fields[k].soapType;
                            fdr["type"] = dor[j].fields[k].type;
                            fdr["length"] = dor[j].fields[k].length;
                            fdr["byte_length"] = dor[j].fields[k].byteLength;
                            fdr["digits"] = dor[j].fields[k].digits;
                            fdr["precision"] = dor[j].fields[k].precision;
                            fdr["scale"] = dor[j].fields[k].scale;
                            fdr["is_autonumber"] = dor[j].fields[k].autoNumber;
                            fdr["is_calculated"] = dor[j].fields[k].calculated;
                            fdr["is_case_sensitive"] = dor[j].fields[k].caseSensitive;
                            fdr["is_custom"] = dor[j].fields[k].custom;
                            fdr["is_dependent_pick_list"] = dor[j].fields[k].dependentPicklist;
                            fdr["is_external_id"] = dor[j].fields[k].externalId;
                            fdr["is_ID_lookup"] = dor[j].fields[k].idLookup;
                            fdr["is_unique"] = dor[j].fields[k].unique;
                            fdr["is_restricted_picklist"] = dor[j].fields[k].restrictedPicklist;
                            fdr["is_name_field"] = dor[j].fields[k].nameField;
                            fdr["download"] = true;
                            SFObjectFields.Rows.Add(fdr);
                            fdr = SFObjectFields.NewRow();
                        }
                    }
                }
            }

            catch(Exception e)
            {
                Logger.LogTaskError(LogTaskType.DownloadFieldMetadata, logTaskId, e);
                Environment.Exit(0);


            }

            Dictionary<string, DataTable> dc = new Dictionary<string, DataTable>();
            dc.Add("SFObjectFields", SFObjectFields);
            dc.Add("SFObjectPicklistValues", SFObjectPicklistValues);

            Logger.LogTaskComplete(LogTaskType.DownloadFieldMetadata, logTaskId);

            return dc;

        }

        public DataTable GetObject(string objectName)
        {

            DataTable SFObjectFields = new MetaDataTable("sfm_fields").GetDataTable();
            DataRow fdr = SFObjectFields.NewRow();

            SFP.DescribeSObjectResult dor = null;
            SFP.LimitInfo[] li;
            li = SFSoapClient.describeSObject(SFSessionHeader, null, null, null, objectName, out dor);

            for (int i = 0; i < dor.fields.Count(); i++)
            {

                fdr["object_name"] = dor.name;
                fdr["name"] = dor.fields[i].name;
                fdr["label"] = dor.fields[i].label;
                fdr["soap_type"] = dor.fields[i].soapType;
                fdr["type"] = dor.fields[i].type;
                fdr["length"] = dor.fields[i].length;
                fdr["byte_length"] = dor.fields[i].byteLength;
                fdr["digits"] = dor.fields[i].digits;
                fdr["precision"] = dor.fields[i].precision;
                fdr["scale"] = dor.fields[i].scale;
                fdr["is_autonumber"] = dor.fields[i].autoNumber;
                fdr["is_calculated"] = dor.fields[i].calculated;
                fdr["is_case_sensitive"] = dor.fields[i].caseSensitive;
                fdr["is_custom"] = dor.fields[i].custom;
                fdr["is_dependent_pick_list"] = dor.fields[i].dependentPicklist;
                fdr["is_external_id"] = dor.fields[i].externalId;
                fdr["is_ID_lookup"] = dor.fields[i].idLookup;
                fdr["is_unique"] = dor.fields[i].unique;
                fdr["is_restricted_picklist"] = dor.fields[i].restrictedPicklist;
                fdr["is_name_field"] = dor.fields[i].nameField;
                SFObjectFields.Rows.Add(fdr);
                fdr = SFObjectFields.NewRow();
            }

            return SFObjectFields;

        }


        //download and upload data methods
        public DataTable GetData(MetaDataTable mdt)
        {
            DataTable SFData = mdt.GetDataTable();
            DataRow dr = SFData.NewRow();
            string select = mdt.GetSoqlSelect();
            bool done = false;


            SFP.LimitInfo[] li;
            SFP.QueryResult qr = null;

            try
            {
                li = SFSoapClient.query(SFSessionHeader, null, null, null, null, select, out qr);
            }
            catch (FaultException e)
            {
                mdt.CantQuery = true;
                mdt.CantQueryError = e.Message;
                return null;
            }


            //batch loop
            while (!done)
            {

                done = qr.done;
                int recordCount = 0;

                if (qr.records != null) { recordCount = qr.records.Count();}


                //loop records
                for (int i = 0; i < recordCount; i++)
                {   //loop fields
                    for (int j = 0; j < qr.records[i].Any.Count(); j++)
                    {
                        //if not blank
                        if (qr.records[i].Any[j].InnerText != "")
                        {

                            switch (SFData.Columns[qr.records[i].Any[j].LocalName].DataType.Name)
                            {
                                case "Double":
                                    
                                    dr[qr.records[i].Any[j].LocalName] = Convert.ToDouble(qr.records[i].Any[j].InnerText);
                                    break;

                                case "TimeSpan":
                                    dr[qr.records[i].Any[j].LocalName] = qr.records[i].Any[j].InnerText.TrimEnd('Z');
                                    break;

                                case "Int32":
                                    dr[qr.records[i].Any[j].LocalName] = Convert.ToInt32(Double.Parse(qr.records[i].Any[j].InnerText));
                                    break;

                                default:
                                    dr[qr.records[i].Any[j].LocalName] = qr.records[i].Any[j].InnerText;
                                    break;
                            }
                        }
                        //end fields
                    }

                    SFData.Rows.Add(dr);
                    dr = SFData.NewRow();

                    //end records
                }

                if (!done) { li = SFSoapClient.queryMore(SFSessionHeader, null, null, qr.queryLocator, out qr); }

                //end batches
            }


            return (SFData);

        }

        public async Task<MetaDataTable> GetCountAsync(MetaDataTable mdt)
        {


            int logTaskId = Logger.LogTaskStart(LogTaskType.CountObject, mdt.ObjectName);

            string select = "select count() from " + mdt.ObjectName;

            SFP.LimitInfo[] li;
            SFP.QueryResult qr = null;

            try
            {
                await Task.Run(() => li = SFSoapClient.query(SFSessionHeader, null, null, null, null, select, out qr));
            }
            catch (FaultException e)
            {
                mdt.CantQuery = true;
                mdt.CantQueryError = e.Message;
                return (mdt);

            }
            catch (Exception e)
            {

                Logger.LogTaskError(LogTaskType.CountObject, logTaskId, e);
                Environment.Exit(0);

            }

            mdt.RecordCount = qr.size;
            mdt.IsEmpty = mdt.RecordCount == 0;

            Logger.LogTaskComplete(LogTaskType.CountObject, logTaskId);
            return (mdt);


        }

        async public Task<MetaDataTable> GetDataAsync(MetaDataTable mdt, DateTime lastDownLoadOn)
        {

            // 1. If they don't exist create the required object tables
            _Target.CreateTable(mdt, "temp");
            _Target.CreateTable(mdt, "history");
            _Target.CreateTable(mdt, "base");

            // 2. Log task start
            int logTaskId = Logger.LogTaskStart(LogTaskType.DownloadObjectData, mdt.ObjectName);

            // 3. Get a data table matching the SF object fields
            DataTable SFData = mdt.GetDataTable();
            DataRow dr = SFData.NewRow();

            // 4. Create the SOQL Query
            string lastDownLoadOnStr =  lastDownLoadOn.ToUniversalTime()
                                        .ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'");

            string where = " WHERE SystemModstamp > " + lastDownLoadOnStr;
            string select = mdt.GetSoqlSelect();
            if (mdt.IntegrationMethod == "Incremental") { select += where; }


            // 5. Execute SOQL query
            bool done = false;
            SFP.LimitInfo[] li;
            SFP.QueryResult qr = null;
                

            try
            {
                await Task.Run(() => li = SFSoapClient.queryAll(SFSessionHeader, null, null, select, out qr));

            }

            catch (FaultException e)
            {
                mdt.CantQuery = true;
                mdt.CantQueryError = e.Message;
                mdt.SourceData = SFData;
                return mdt;

            }

            catch (Exception e)
            {
                Logger.LogTaskError(LogTaskType.DownloadObjectData, logTaskId, e);
                Environment.Exit(0);

            }


            try
            {

                // 6. Process batches
                while (!done)
                {

                    done = qr.done;
                    int recordCount = 0;

                    if (qr.records != null) { recordCount = qr.records.Count(); }


                    //loop records
                    for (int i = 0; i < recordCount; i++)
                    {   //loop fields
                        for (int j = 0; j < qr.records[i].Any.Count(); j++)
                        {
                            //if not blank
                            if (qr.records[i].Any[j].InnerText != "")
                            {

                                switch (SFData.Columns[qr.records[i].Any[j].LocalName].DataType.Name)
                                {
                                    case "Double":

                                        dr[qr.records[i].Any[j].LocalName] = Convert.ToDouble(qr.records[i].Any[j].InnerText);
                                        break;

                                    case "TimeSpan":
                                        dr[qr.records[i].Any[j].LocalName] = qr.records[i].Any[j].InnerText.TrimEnd('Z');
                                        break;

                                    case "Int32":
                                        dr[qr.records[i].Any[j].LocalName] = Convert.ToInt32(Double.Parse(qr.records[i].Any[j].InnerText));
                                        break;

                                    default:
                                        dr[qr.records[i].Any[j].LocalName] = qr.records[i].Any[j].InnerText;
                                        break;
                                }
                            }
                            //end fields
                        }

                        SFData.Rows.Add(dr);
                        dr = SFData.NewRow();

                        //end records
                    }

                    if (!done) { li = SFSoapClient.queryMore(SFSessionHeader, null, null, qr.queryLocator, out qr); }

                    //end batches
                }

            } 

            catch (Exception e)
            {
                Logger.LogTaskError(LogTaskType.DownloadObjectData, logTaskId, e);
                Environment.Exit(0);

            }

            Logger.LogTaskComplete(LogTaskType.DownloadObjectData, logTaskId);


            // 7. Save to SQL
            mdt.CantQuery = false;
            mdt.SourceData = SFData;
            _Target.RunScript("TRUNCATE TABLE " + mdt.SchemaName + "." + mdt.TableNameTemp);
            _Target.SaveAnyObject(mdt, mdt.SourceData);
            return (mdt);

        }

        public async Task<MetaDataTable> BulkGetDataByBatch(MetaDataTable mdt, DateTime lastDownLoadOn, bool pkChunking)
        {

            // 1. If they don't exist create the required object tables
            _Target.CreateTable(mdt, "temp");
            _Target.CreateTable(mdt, "history");
            _Target.CreateTable(mdt, "base");

            // 2. Log Task
            int logTaskId = Logger.LogTaskStart(LogTaskType.DownloadObjectData, mdt.ObjectName);

            // 3. Create the SOQL Query
            string lastDownLoadOnStr = lastDownLoadOn.ToUniversalTime()
                                        .ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'");

            string where = " WHERE SystemModstamp > " + lastDownLoadOnStr;

            string select = mdt.GetSoqlSelect();
            if (mdt.IntegrationMethod == "Incremental") { select += where; }



            // 4. Truncate the Temp Table
            _Target.RunScript("TRUNCATE TABLE " + mdt.SchemaName + "." + mdt.TableNameTemp);


            try
            {

                // 5. Create a new bulk query job at SF instance
                BulkQueryJob bqj = new BulkQueryJob(await BulkCreateJob(mdt.ObjectName, pkChunking, mdt.BulkQueryBatchSize));
                mdt.BulkQueryJob = bqj;

                // 6. Get a table of fields for the object
                DataTable dt = _Target.GetObjectFields(mdt.ObjectName, true);

                // 7. Execute bulk query
                bqj.BatchId = await BulkCreateQuery(bqj.JobId, select);

                //8. Wait till all batches are processed
                //the job will generate multiple batches if PK chunking is on
                string jobStatus = "Waiting";
                do
                {   //wait a bit
                    await Task.Run(() => Thread.Sleep(10000));
                    jobStatus = await BulkGetJobStatus(bqj.JobId);
                }
                while (jobStatus == "Waiting");

                //TODO: Will not work for PK chunked queries. The starting batch is not
                //run and so will need to look to each chunked batch to get the state 
                //and error message. The state message is not even a node in the XML 
                //so will throw an error.
                if (jobStatus == "Failed")
                {
                    XElement batchStatus = await BulkGetBatchStatus(bqj.JobId, bqj.BatchId);
                    bqj.BatchStatus = batchStatus.Descendants(bulkXmlNs + "state").First().Value;
                    bqj.BatchError = batchStatus.Descendants(bulkXmlNs + "stateMessage").First().Value;
                    throw new Exception(bqj.BatchError);
                }

                // 9. Get all Batch IDs
                //retrieve the status of all of the batches created
                XElement batchXML = await BulkGetBatches(bqj.JobId);
                //set up a list of tasks where each task will process one batch
                List<Task<Boolean>> listBulkUploadDataTasks = new List<Task<Boolean>>();


                // 10. For Each batch ID
                foreach (XElement batchElement in batchXML.Descendants(bulkXmlNs + "id"))
                {

                    //get the batch ID
                    string batchId = batchElement.Value;

                    //the orginal batch is not processed when PK chunking is on so ignore this batch
                    if (batchId != bqj.BatchId || !pkChunking)
                    {

                        XElement resultsXML = await BulkGetResults(bqj.JobId, batchId);
                        foreach (XElement resultElement in resultsXML.Descendants(bulkXmlNs + "result"))
                        {
                            string resultId = resultElement.Value;

                            //create a meta data table for each batch that will be processed
                            //generate the data table that the CSV file will processed into
                            MetaDataTable mdtu = new MetaDataTable(mdt.ObjectName, dt);
                            mdtu.SourceData = mdtu.GetDataTable();
                            mdtu.BulkQueryJob = bqj;

                            //create a task to run the Bulk Upload from SF to CSV to SQL and add to task list
                            Task<Boolean> bulkUploadDataTask = BulkUploadData(mdtu, batchElement.Value, resultId);
                            listBulkUploadDataTasks.Add(bulkUploadDataTask);
                        }


                    }

                }

                Task<Boolean>[] arrayBulkUploadDataTasks = listBulkUploadDataTasks.ToArray();
                Task.WaitAll(arrayBulkUploadDataTasks);
            }
            
            catch (Exception e)
            {
                Logger.LogTaskError(LogTaskType.DownloadObjectData, logTaskId, e);
                Environment.Exit(0);

            }

            Logger.LogTaskComplete(LogTaskType.DownloadObjectData, logTaskId);
            return mdt;

        }

        private async Task<Boolean> BulkUploadData (MetaDataTable mdt, string batchId, string resultSetId)
        {

            // 1. Open a stream and get the results set
            Stream st = await BulkGetResultSet(mdt.BulkQueryJob.JobId, batchId, resultSetId);

            // 4. Setup the text file reader
            TextFieldParser ftp = new TextFieldParser(st);

            ftp.TextFieldType = FieldType.Delimited;
            ftp.Delimiters = new[] { "," };
            ftp.HasFieldsEnclosedInQuotes = true;

            
            int i = 0;
            //skip the first (header) row
            ftp.ReadFields();
            string[] ftpRow;

            //loop through the file adding to datatable
            while (!ftp.EndOfData)
            {

                ftpRow = ftp.ReadFields();

                for (int j = 0; j < ftpRow.Count(); j++)
                {

                    if (ftpRow[j] == "") { ftpRow[j] = null; }

                }

                mdt.SourceData.Rows.Add(ftpRow);
                i++;


            }

            _Target.SaveAnyObject(mdt, mdt.SourceData);
            
            return (true);



        }

       
        //bulk query methods
        private async Task<string> BulkCreateJob(string objectName, bool pkChunking, int chunkSize = 250000)
        {
            HttpClient hc = new HttpClient();
            string createJobUrl = SFBulkBaseUrl + "job";
            string createJobXml = Properties.Resources.ResourceManager.GetString("createAccountJob");
            createJobXml = createJobXml.Replace("ObjectName", objectName);
            StringContent sc = new StringContent(createJobXml, Encoding.UTF8, "text/xml");

            sc.Headers.Add("X-SFDC-Session", SFSessionHeader.sessionId);

            if (pkChunking) { sc.Headers.Add("Sforce-Enable-PKChunking", "chunkSize=" + chunkSize); }


            HttpResponseMessage rm = await hc.PostAsync(createJobUrl, sc);
            Stream st = await rm.Content.ReadAsStreamAsync();
            XElement xl = XElement.Load(st);
            return xl.Descendants(bulkXmlNs + "id").First().Value;
        }
        
        private async Task<string> BulkCreateQuery(string jobId, string query)
        {
            HttpClient hc = new HttpClient();
            string createQueryUrl = SFBulkBaseUrl + "job/" + jobId + "/batch";
            StringContent sc = new StringContent(query, Encoding.UTF8, "text/csv");
            sc.Headers.Add("X-SFDC-Session", SFSessionHeader.sessionId);
            HttpResponseMessage rm = await hc.PostAsync(createQueryUrl, sc);
            Stream st = await rm.Content.ReadAsStreamAsync();
            XElement xl = XElement.Load(st);
            return xl.Descendants(bulkXmlNs + "id").First().Value;
        }

        private async Task<XElement> BulkGetBatchStatus(string jobId, string batchId)
        {
            HttpClient hc = new HttpClient();
            string getQueryStatusUrl = SFBulkBaseUrl + "job/" + jobId + "/batch/" + batchId;
            hc.DefaultRequestHeaders.Add("X-SFDC-Session", SFSessionHeader.sessionId);
            HttpResponseMessage rm = await hc.GetAsync(getQueryStatusUrl);
            Stream st = await rm.Content.ReadAsStreamAsync();
            XElement xl = XElement.Load(st);
            return xl;
        }

        private async Task<XElement> BulkGetResults(string jobId, string batchId)
        { 
            HttpClient hc = new HttpClient();
            string getResultsUrl = SFBulkBaseUrl + "job/" + jobId + "/batch/" + batchId + "/result";
            hc.DefaultRequestHeaders.Add("X-SFDC-Session", SFSessionHeader.sessionId);
            HttpResponseMessage rm = await hc.GetAsync(getResultsUrl);
            Stream st = await rm.Content.ReadAsStreamAsync();
            XElement xl = XElement.Load(st);
            return xl;
        }

        private async Task<Stream> BulkGetResultSet(string jobId, string batchId, string resultId)
        {

            HttpClient hc = new HttpClient();
            string getResultSetUrl = SFBulkBaseUrl + "job/" + jobId + "/batch/" + batchId + "/result/" + resultId;
            hc.DefaultRequestHeaders.Add("X-SFDC-Session", SFSessionHeader.sessionId);
            HttpResponseMessage rm = await hc.GetAsync(getResultSetUrl);
            Stream st = await rm.Content.ReadAsStreamAsync();
            return st;

        }

        private async Task<string> BulkGetJobStatus(string jobId)
        {
            HttpClient hc = new HttpClient();
            string getJobStatusUrl = "https://eu6.salesforce.com/services/async/40.0/job/" + jobId;
            hc.DefaultRequestHeaders.Add("X-SFDC-Session", SFSessionHeader.sessionId);

            HttpResponseMessage rm = await hc.GetAsync(getJobStatusUrl);
            Stream st = await rm.Content.ReadAsStreamAsync();
            XElement xl = XElement.Load(st);
            int batchesCompleted = int.Parse(xl.Descendants(bulkXmlNs + "numberBatchesCompleted").First().Value);
            int batchesTotal = int.Parse(xl.Descendants(bulkXmlNs + "numberBatchesTotal").First().Value);
            int batchesFailed = int.Parse(xl.Descendants(bulkXmlNs + "numberBatchesFailed").First().Value);

            if (batchesFailed != 0) {
                return "Failed";
            }
            else if (batchesCompleted == batchesTotal && batchesCompleted > 0)
            {
                return "Success";
            }            
            else { return "Waiting"; }
            



        }

        private async Task<XElement> BulkGetBatches(string jobId)
        {
            HttpClient hc = new HttpClient();
            string getBatchesUrl = "https://eu6.salesforce.com/services/async/40.0/job/" + jobId + "/batch";
            hc.DefaultRequestHeaders.Add("X-SFDC-Session", SFSessionHeader.sessionId);

            HttpResponseMessage rm = await hc.GetAsync(getBatchesUrl);
            Stream st = await rm.Content.ReadAsStreamAsync();
            XElement xl = XElement.Load(st);

            return xl;
        }


    }

}

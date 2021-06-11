using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using System.Data.SqlClient;
using System.Data;
using System.IO;
using System.Xml.Linq;
using static SF_Download.Logger;

namespace SF_Download
{

    class Program
    {
    
        static void Main(string[] args)
        {
            var result = Parser.Default.ParseArguments<InitOptions, DownloadOptions, ObjectOptions, FieldOptions>(args)
                .WithParsed<InitOptions>(options => { Init(options); })
                .WithParsed<DownloadOptions>(options => { Download(options); })
                .WithParsed<ObjectOptions>(options => { Objects(options); })
                .WithParsed<FieldOptions>(options => { Fields(options); });

        }

        // Manage Object Download Settings
        static void Objects(ObjectOptions options)
        {

            // 1. Test SQL Connection
            SFDTarget target = new SFDTarget(options.SqlServer, options.SqlDatabase);
            if (!target.isConnectionOK)
            {
                Console.WriteLine("SQL Login Failed");
                Console.WriteLine(target.ConnectionError.Message);
                return;
            }


            // 2. Check each option has a valid value
            Lookup downloadMethods = new Lookup("DownloadMethod");
            Lookup IntegrationMethods = new Lookup("IntegrationMethod");

            if (!(new List<string> { "Y", "N", null }).Contains(options.Download))
            {
                Console.WriteLine("Download option must be: Y | N");
                return;
            }

            if (!downloadMethods.ExistsLookupValue(options.DownloadMethod))
            {
                Console.WriteLine("Download method option must be: {0}", downloadMethods.ListLookupKeys());
                return;
            }

            if (!IntegrationMethods.ExistsLookupValue(options.IntegrationMethod))
            {
                Console.WriteLine("Integration method option must be: {0}", IntegrationMethods.ListLookupKeys());
                return;
            }


            target.OpenPersistConnection();
            Logger logger = new Logger(-1, 1);
            logger.Target = target;
            target.Logger = logger;
            target.LogExecutionStart("Objects");



            // 3. Get the object current settings
            MetaDataTable mdt = new MetaDataTable(options.SfObject, target.GetObjectFields(options.SfObject));
            if (!mdt.GetSettings(target))
            {
                Console.WriteLine("Cannot get object settings, check that {0} exists.", mdt.ObjectName);
                return;
            }


            // 4. If no options set output the current settings for the object
            if (options.Download == null && options.DownloadMethod == null && options.IntegrationMethod == null)
            {
                Console.WriteLine(options.SfObject + ":{3}Download={0}{3}Download Method={1}{3}Integration Method={2}", mdt.Download, mdt.DownloadMethod, mdt.IntegrationMethod, Environment.NewLine);
                target.LogExecutionComplete(true);
                return;
            }


            // 5. If the object has no delete flag it cannot be set to Download Method incremental
            if (options.IntegrationMethod == "Incremental" && !mdt.HasIsDeleted)
            {
                Console.WriteLine("You can't set the integration method to Incremental for {0} as it does not have a IsDeleted column in Saleforce. This object's settings have not been changed.", mdt.ObjectName);
                return;
            }

            // 5. If the object is changed from Incremental to Delete
            if (options.IntegrationMethod == "Delete" && mdt.IntegrationMethod == "Incremental")
            {
                Console.WriteLine("When changing from Incremental to Delete and Replace any existing history will not be removed. Switching back to Incremental will mean there are gaps in the history.");
            }
        
            // 5. Update the options that are set
            // convert the download option to a bool
            bool? download = null;
            if (options.Download == "Y") { download = true; }
            if (options.Download == "N") { download = false; }

            target.UpdateObjectDownloadStatus(mdt, download, 
                downloadMethods.GetLookupValue(options.DownloadMethod, false), 
                IntegrationMethods.GetLookupValue(options.IntegrationMethod, false), 250000);

            target.LogExecutionComplete(true);

        }

        // Manage Field Download Settings
        static void Fields(FieldOptions options)
        {

            // 1. Connect to target
            SFDTarget target = new SFDTarget(options.SqlServer, options.SqlDatabase);
            if (!target.isConnectionOK)
            {
                Console.WriteLine("SQL Login Failed");
                Console.WriteLine(target.ConnectionError.Message);
                return;
            }

            target.OpenPersistConnection();
            Logger logger = new Logger(-1, 1);
            logger.Target = target;
            target.Logger = logger;
            target.LogExecutionStart("Fields");


            // 2. Get a list of protected fields
            Lookup protectedFields = new Lookup("ProtectedFields");

            // 2. If field name has been specified update just this field.
            if (options.FieldName != null) {

                //Download option must be set
                if (!(new List<string> { "Y", "N" }).Contains(options.Download))
                {
                    Console.WriteLine();
                    Console.WriteLine("Download option must be: Y | N");
                    return;
                }

                
                //Cannot change protected fields to N
                if (protectedFields.ExistsLookupValue(options.FieldName.ToUpper()) && options.Download == "N")
                {
                    Console.WriteLine();
                    Console.WriteLine("Download cannot be set to No for protected fields");
                    return;
                }

                bool download = false;
                if (options.Download == "Y") { download = true; }

                if (target.UpdateFieldDownloadStatus(options.SfObject, options.FieldName, download))
                {
                    Console.WriteLine();
                    Console.WriteLine("Field {0} on {1} updated.", options.FieldName, options.SfObject);
                    target.LogExecutionComplete(true);

                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine("Field {0} on {1} could not be updated. Check the object and field exist", options.FieldName, options.SfObject);
                }
                
                

            }

            // If field name not specified get all fields and prompt for status
            else
            {
                Console.WriteLine();
                Console.WriteLine("Updating download status for {0}:", options.SfObject);
                Console.WriteLine();
                DataTable fields = target.GetObjectFields(options.SfObject);
                if (fields.Rows.Count < 1)
                {
                    Console.WriteLine("No fields found for this object, check that {0} exists.", options.SfObject);
                    return;

                }

                foreach (DataRow dr in fields.Rows)
                {

                    if (protectedFields.ExistsLookupValue(((string) dr["name"]).ToUpper())) {
                        continue;
                    }

                    char input = 'Z';
                    Console.Write("Field: {0,-25}  Download (Y/N): ", dr["name"]);

                    do
                    {
                        input = char.ToUpper(Console.ReadKey(true).KeyChar);
                    }
                    while (!(input == 'Y' || input == 'N'));

                    Console.WriteLine("{0,3}", input);
                    bool download = false;
                    if (input == 'Y') { download = true; }

                    target.UpdateFieldDownloadStatus(options.SfObject, (string) dr["name"], download);

                    }

                Console.WriteLine();
                Console.WriteLine("All fields updated.");
                target.LogExecutionComplete(true);

            }

        }

        // Initialise Database and Meta Data
        static void Init(InitOptions options)
        {


            Console.WriteLine();
            Console.WriteLine(DateTime.Now.ToString() + " : Test SQL Connection");
            // 1. Test SQL connection   
            SFDTarget target = new SFDTarget(options.SqlServer, options.SqlDatabase);
            if (!target.isConnectionOK)
            {
                Console.WriteLine("SQL Login Failed");
                Console.WriteLine(target.ConnectionError.Message);
                return;
            }

            Logger logger = new Logger(1, 1);



            // 2. Test SF connection
            SFDSource source = new SFDSource(options.SfUsername, options.SfPassword, options.SfToken, false, target);
            source.Logger = logger;
            source.TestConnection();
            

            // 3. Check each option has a valid value
            Lookup downloadMethods = new Lookup("DownloadMethod");
            Lookup integrationMethods = new Lookup("IntegrationMethod");

            if (!downloadMethods.ExistsLookupValue(options.DownloadMethod))
            {
                logger.LogTaskError(LogTaskType.ValidateDownloadMethod, 0, new Exception("Download method must be:" + downloadMethods.ListLookupKeys()));
                return;
            }

            if (!integrationMethods.ExistsLookupValue(options.IntegrationMethod))
            {
                logger.LogTaskError(LogTaskType.ValidateIntegrationMethod, 0, new Exception("Integration method must be:" + integrationMethods.ListLookupKeys())); return;
            }
                
            target.Logger = logger;
            target.OpenPersistConnection();
            target.OpenTransaction();
            target.Initialise();
            target.CommitTransaction();
            target.LogExecutionStart("Init");
            logger.Target = target;

            target.OpenTransaction();


            // 5. Set default options and save
            options.DownloadMethod =  downloadMethods.GetLookupValue(options.DownloadMethod);
            options.IntegrationMethod = integrationMethods.GetLookupValue(options.IntegrationMethod);
            // Default values are set for unspecified parameters by target.SaveSettings
            target.SaveSettings(false, options.SfUsername, options.SfPassword, options.SfToken,
                                options.DownloadNew, false, options.DownloadMethod, options.IntegrationMethod);

            
            // 6. Connect to SF
            source.Connect();
                

            // 7. Save the meta data back to the SQL database temp tables and move through
            // history and base tables detecting changes
            target.SaveObjects(source.Objects);
            target.SaveObjectFields(source.ObjectFields, source.PicklistValues);
            target.UpdateMetaData();


            // 8. Run a count query against all objects
            Console.WriteLine(DateTime.Now.ToString() + " : Check SF Object Status");
            MetaDataTables mdts = new MetaDataTables(source, target, source.Objects, false);
            Task<MetaDataTable>[] listGetCountTasks = mdts.GetCountTasks();
            Task.WaitAll(listGetCountTasks);

            // 9. Update each object with status and default options
            foreach (Task<MetaDataTable> getCountTask in listGetCountTasks)
            {
                MetaDataTable mdt = getCountTask.Result;
                target.UpdateObjectDownloadStatus(mdt, options.DownloadNew, options.DownloadMethod, options.IntegrationMethod, 250000); 
            }


            target.CommitTransaction();

            // 10. Done!
            target.LogExecutionComplete(true);
            Console.WriteLine(DateTime.Now.ToString() + " : Database Initialisation Complete");
            Console.WriteLine();
            return;

        }

        // Run a Download
        static void Download(DownloadOptions options)
        {

            Console.WriteLine();
            Console.WriteLine(DateTime.Now.ToString() + " : Test SQL Connection");
            // 1. Test SQL connection   
            SFDTarget target = new SFDTarget(options.SqlServer, options.SqlDatabase);
            if (!target.isConnectionOK)
            {
                Console.WriteLine("SQL Login Failed");
                Console.WriteLine(target.ConnectionError.Message);
                return;
            }

            target.OpenPersistConnection();
            Logger logger = new Logger(1, 1);
            logger.Target = target;
            target.Logger = logger;
            target.LogExecutionStart("Download");
            target.GetSettings();


            // 2. Test SF connection
            SFDSource source = new SFDSource(target.SfUsername, target.SfPassword, target.SfToken, false, target);
            source.Logger = logger;
            source.TestConnection();
            source.Connect();


            // open meta data processing transaction
            target.OpenTransaction();

            // 4. Get and save the latest meta data to temp and then update to history and base
            target.SaveObjects(source.Objects);
            target.SaveObjectFields(source.ObjectFields, source.PicklistValues);
            target.UpdateMetaData();


            // 5. Get new objects and test with a count query
            DataTable newObjects = target.GetNewObjects();
            MetaDataTables mdts = new MetaDataTables(source, target, newObjects, false);
            Task<MetaDataTable>[] listGetCountTasks = mdts.GetCountTasks();
            Task.WaitAll(listGetCountTasks);


            // 6. Update each object with status and default settings
            foreach (Task<MetaDataTable> getCountTask in listGetCountTasks)
            {
                MetaDataTable mdt = getCountTask.Result;
                target.UpdateObjectDownloadStatus(mdt, target.DownloadNewObjects, target.DownloadMethod, target.IntegrationMethod, 250000);
            }


            // 7. Check the counts of previously empty objects
            DataTable emptyObjects = target.GetEmptyObjects();
            mdts = new MetaDataTables(source, target, emptyObjects, false);
            listGetCountTasks = mdts.GetCountTasks();
            Task.WaitAll(listGetCountTasks);


            // 8. Update each object with status and default settings
            foreach (Task<MetaDataTable> getCountTask in listGetCountTasks)
            {
                MetaDataTable mdt = getCountTask.Result;
                target.UpdateObjectDownloadStatus(mdt, true, mdt.DownloadMethod, mdt.IntegrationMethod, mdt.BulkQueryBatchSize);
            }


            //  9. Get new fields for all objects
            //  Alter the target tables to add these fields
            //  TODO: Move this to just after the update meta data step
            //  since this is part of the meta data management process
            MetaDataTables mdtfu = new MetaDataTables(source, target);
            mdtfu.AddNewFieldObjects();
            mdtfu.AddNewFields();


            //  10. Get changed datatype fields for all objects
            //  If possible update the data types 
            //  If not possible disable download on these fields
            MetaDataTables mdtfdu = new MetaDataTables(source, target);
            mdtfdu.AddChangedFieldDatatypeObjects();
            mdtfdu.AlterFieldDatatypes();


            // commit meta data processing
            target.CommitTransaction();


            // open data processing transaction
            target.OpenTransaction();

            DataTable downloadObjects = target.GetDownloadableObjects();
            mdts = new MetaDataTables(source, target, downloadObjects, true);
            Console.WriteLine(DateTime.Now.ToString() + " : Downloading All Objects");
            Task<MetaDataTable>[] listGetDownloadTasks = mdts.GetDownloadTasks();
            Task.WaitAll(listGetDownloadTasks);


            // 12. Move data between temp and history and base tables
            foreach (Task<MetaDataTable> getDownloadTask in listGetDownloadTasks)
            {


                MetaDataTable mdt = getDownloadTask.Result;
                int logTaskId = logger.LogTaskStart(LogTaskType.MoveObjectDataToBase, mdt.ObjectName);

                try
                {
                    if (mdt.HasIsDeleted && mdt.IntegrationMethod == "Incremental")
                    {
                        target.RunScript(mdt.GetTempToHistory(logTaskId));
                        target.RunScript(mdt.GetDeleteFromBase(logTaskId));
                        target.RunScript(mdt.GetHistoryToBase(logTaskId));
                    }
                    else
                    {
                        target.RunScript(mdt.GetTempToBase());
                    }
                }

                catch (Exception e)
                {
                    logger.LogTaskError(LogTaskType.MoveObjectDataToBase, logTaskId, e);
                    Environment.Exit(0);

                }

                logger.LogTaskComplete(LogTaskType.MoveObjectDataToBase, logTaskId);


            }

            // commit data processing
            target.CommitTransaction();

            // 13. Done
            target.LogExecutionComplete(true);
            Console.WriteLine(DateTime.Now.ToString() + " : Download Complete");
            //Console.ReadLine();

        }


    }


}

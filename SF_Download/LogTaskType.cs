


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


namespace SF_Download
{

    public class LogTaskType : Enumeration
    {
        public static readonly LogTaskType MoveObjectDataToBase = new LogTaskType(0, "Move Object Data To Base", 0, 10);
        public static readonly LogTaskType DownloadObjectData = new LogTaskType(0, "Download Object Data", 0, 10);
        public static readonly LogTaskType UploadObjectData = new LogTaskType(0, "Upload Object Data", 0, 10);
        public static readonly LogTaskType AlterObjectTableFieldType = new LogTaskType(0, "Alter Object Table Field Type", 0, 0);
        public static readonly LogTaskType AlterObjectTableFields = new LogTaskType(0, "Alter Object Table Fields", 0, 0);
        public static readonly LogTaskType CreateObjectTable = new LogTaskType(0, "Create Object Table", 0, 10);
        public static readonly LogTaskType UpdateFieldDownloadStatus = new LogTaskType(0, "Update Field Download Status", 0, 0);
        public static readonly LogTaskType GetObjectFields = new LogTaskType(0, "Get Object Fields", 0, 10);
        public static readonly LogTaskType GetDownloadableObjects = new LogTaskType(0, "Get Downloadable Objects", 0, 0);
        public static readonly LogTaskType GetObjectSettings = new LogTaskType(0, "Get Object Settings", 0, 0);
        public static readonly LogTaskType GetChangedFields = new LogTaskType(0, "Get Changed Fields", 0, 0);   
        public static readonly LogTaskType GetNewObjectFields = new LogTaskType(0, "Get New Object Fields", 0, 0);
        public static readonly LogTaskType GetGlobalSettings = new LogTaskType(0, "Get Global Settings", 0, 0);
        public static readonly LogTaskType GetEmptyObjects = new LogTaskType(0, "Get Empty Objects", 0, 0);
        public static readonly LogTaskType GetNewObjects = new LogTaskType(0, "Get New Objects", 0, 0);
        public static readonly LogTaskType ValidateDownloadMethod = new LogTaskType(0, "Validate Download Method", 0, 0);
        public static readonly LogTaskType ValidateIntegrationMethod = new LogTaskType(0, "Validate Integration Method", 0, 0);
        public static readonly LogTaskType ConnecttoSF = new LogTaskType(0, "Connect to SF", 0, 0);
        public static readonly LogTaskType TestSFConnection = new LogTaskType(0, "Test Salesforce Connection", 0, 0);
        public static readonly LogTaskType SetupSQLDatabase = new LogTaskType(0, "Setup SQL Database", 100, 0);
        public static readonly LogTaskType SaveInitalSettings = new LogTaskType(0, "Save Inital Settings", 0, 0);
        public static readonly LogTaskType DownloadObjectMetadata = new LogTaskType(0, "Download Object Metadata", 0, 0);
        public static readonly LogTaskType UploadObjectMetadata = new LogTaskType(0, "Upload Object Metadata", 0, 0);
        public static readonly LogTaskType DownloadFieldMetadata = new LogTaskType(0, "Download Field Metadata", 0, 0);
        public static readonly LogTaskType UploadFieldMetadata = new LogTaskType(0, "Upload Field Metadata", 0, 0);
        public static readonly LogTaskType DetectMetadataChanges = new LogTaskType(0, "Detect Metadata Changes", 0, 0);
        public static readonly LogTaskType CountObject = new LogTaskType(0, "Count Object", 0, 10);
        public static readonly LogTaskType UploadObjectStatus = new LogTaskType(0, "Upload Object Status", 0, 10);

        // Constructors
        private LogTaskType() { }

        private LogTaskType(int value, string displayName, int minLogLevel, int minConsoleLogLevel) : base(value, displayName)
        {
            MinDBLogLevel = minLogLevel;
            MinConsoleLogLevel = minConsoleLogLevel;

        }

        // Logging levels
        public int MinDBLogLevel { get; set; }
        public int MinConsoleLogLevel { get; set; }


    }

}

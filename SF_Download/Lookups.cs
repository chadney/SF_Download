
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

using System.Collections.Generic;

namespace SF_Download
{
    class Lookup
    {
        private Dictionary<string, string> LookupValues;
        private string defaultValue;

        public Lookup (string lookupType)

        {
            this.LookupValues = new Dictionary<string, string>();

            if (lookupType == "DownloadMethod")
            {
                LookupValues.Add("SOAP", "SOAP");
                LookupValues.Add("Bulk", "Bulk Query");
                LookupValues.Add("Batched", "Bulk Query Batched");

                defaultValue = "SOAP";
            }

            if (lookupType == "IntegrationMethod")
            {
                LookupValues.Add("Incremental", "Incremental");
                LookupValues.Add("Delete", "Delete and Replace");

                defaultValue = "Incremental";
            }

            if (lookupType == "ProtectedFields")
            {
                LookupValues.Add("ID", "ID");
                LookupValues.Add("ISDELETED", "ISDELETED");
                LookupValues.Add("SYSTEMMODSTAMP", "SYSTEMMODSTAMP");

            }
        }


        public bool ExistsLookupValue(string lookupKey)
        {
            if (lookupKey == null)
            {
                return true;
            }
            
            return LookupValues.ContainsKey(lookupKey);
        }



        public string GetLookupValue(string lookupKey, bool returnDefaultForNulls = true)
        {

            if (lookupKey == null && returnDefaultForNulls)
            {
                return defaultValue;
            }
            else if (lookupKey == null)
            {
                return null;
            }
            else
            {
                LookupValues.TryGetValue(lookupKey, out string lookupValue);
                return lookupValue;
            }

        }


        public string ListLookupKeys()
        {
            return string.Join(" | ", LookupValues.Keys);
        }

    }
}

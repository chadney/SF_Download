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

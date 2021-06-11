using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace SF_Download
{
    static class CsvMaker
    {
        static private string QuoteValue(string value)
        {
            return String.Concat("\"",
            value.Replace("\"", "\"\""), "\"");
        }

        static public void WriteCsv(DataTable dt, TextWriter wr)
        {
            IEnumerable<String> items = null;
            IEnumerable<String> headerValues = dt.Columns.OfType<SFDDataColumn>()
                .Select(column => QuoteValue(column.ColumnName));

            wr.WriteLine(String.Join(",", headerValues));

            foreach (DataRow row in dt.Rows)
            {
                items = row.ItemArray.Select(o => QuoteValue(o?.ToString() ?? String.Empty));
                wr.WriteLine(String.Join(",", items));
            }

            wr.Flush();

        }

    }
}

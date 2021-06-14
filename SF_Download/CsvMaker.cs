
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

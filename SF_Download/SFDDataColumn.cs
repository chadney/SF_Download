
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
using System.Data;


namespace SF_Download
{
    public class SFDDataColumn : DataColumn
    {
        public SqlDbType SqlDbType { get; set; }
        public int Length { get; set; }
        public int Scale { get; set; }
        public int Precision { get; set; }
        public SqlDbType PreviousSqlDbType { get; set; }
        public int PreviousLength { get; set; }
        public int PreviousScale { get; set; }
        public int PreviousPrecision { get; set; }
        public bool PK { get; set; }

        public SFDDataColumn(string columnName, Type columnType, SqlDbType sqlType, int length) 
            :base(columnName, columnType)
        {
            SqlDbType = sqlType;
            Length = length;
        }

        public string ColumnDef()
        {   
            string lsp = "";
            string mLength = Length <= 8000 ? Length.ToString() : "MAX";
            
            if (SqlDbType.ToString() == "VarChar") { lsp = "(" + mLength + ")"; }
            if (SqlDbType.ToString() == "Decimal") { lsp = "(" + Precision.ToString() +"," + Scale.ToString() + ")"; }
            return ColumnName + " " + SqlDbType.ToString() + lsp;   
        }

        public bool CanChangeDatatype()
        {

            bool canChangeDatatype = false;

            /*  SQL Datatypes used are:
             
                Bit
                Date
                DateTime
                Decimal
                Int
                Time
                Varchar

                Currently data types cannot be changed. Only the lengths.
            */


            if (SqlDbType.Equals(PreviousSqlDbType))
            {
                switch (SqlDbType)
                {
                    case SqlDbType.VarChar:
                        if (Length >= PreviousLength)
                        {
                            canChangeDatatype = true;
                        }
                        break;

                    case SqlDbType.Decimal:
                        if (Scale >= PreviousScale && Precision >= PreviousPrecision)
                        {
                            canChangeDatatype = true;
                        }
                        break;

                    default:
                        canChangeDatatype = false;
                        break;
                        
                }
            }

            return canChangeDatatype;
        }

    }
}

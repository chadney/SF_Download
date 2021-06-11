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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace IMETPOClasses
{
    public class LineItem
    {
        public enum LineItemState
        {
            pending = 0,
            approved = 2,
            rejected = 4,
            received = 8,
            deleted = 16,
            purchased = 32,
            inventory = 64
        };

        public Guid lineitemid;
        public int qty;
        public string unit;
        public string description;
        public string url;
        public float unitprice;
        public LineItemState state;
        public int qtyreceived;
        public bool inventoryIMET;
        public bool inventoryMD;
        public string itemnumber;

        public float TotalPrice
        {
            get
            {
                return qty * unitprice;
            }
        }

        public LineItem()
        {
            lineitemid = Guid.Empty;
            qty = 0;
            unit = string.Empty;
            description = string.Empty;
            url = string.Empty;
            unitprice = 0;
            state = LineItemState.pending;
            qtyreceived = -1;
            inventoryIMET = false;
            inventoryMD = false;
        }

        public void Save(SqlConnection conn, Guid requestid)
        {
            if (lineitemid == Guid.Empty)
            {
                lineitemid = Guid.NewGuid();
            }
            SqlCommand cmd = new SqlCommand()
            {
                Connection = conn,
                CommandType = CommandType.StoredProcedure,
                CommandText = "sp_savelineitem"
            };
            cmd.Parameters.Add(new SqlParameter("@inid", lineitemid));
            cmd.Parameters.Add(new SqlParameter("@inrequestid", requestid));
            cmd.Parameters.Add(new SqlParameter("@inqty", qty));
            cmd.Parameters.Add(new SqlParameter("@inunit", unit));
            cmd.Parameters.Add(new SqlParameter("@indescription", description));
            cmd.Parameters.Add(new SqlParameter("@inurl", url));
            cmd.Parameters.Add(new SqlParameter("@inunitprice", unitprice));
            cmd.Parameters.Add(new SqlParameter("@instate", state));
            cmd.Parameters.Add(new SqlParameter("@inqtyreceived", qtyreceived));
            cmd.Parameters.Add(new SqlParameter("@ininventoryimet", inventoryIMET));
            cmd.Parameters.Add(new SqlParameter("@ininventorymd", inventoryMD));
            cmd.Parameters.Add(new SqlParameter("@initemnumber", itemnumber));
            cmd.ExecuteNonQuery();
        }
    }
}

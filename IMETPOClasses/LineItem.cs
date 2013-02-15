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
        // The line item state maps to an int, which is then stored in the database
        // In the original, it was a string, but I can't abide strings for things that are not text.
        public enum LineItemState
        {
            opened = -1,
            pending = 0,
            approved = 2,
            rejected = 4,
            received = 8,
            deleted = 16,
            purchased = 32,
            inventory = 64,
            closed = 128
        };

        public Guid lineitemid;
        // The Quantity
        public int qty;
        // The Unit of measure
        public string unit;
        public string description;
        // The URL to view the item on the web
        public string url;
        // Price per unit
        public float unitprice;
        // Quantity Received
        public int qtyreceived;
        // True if it needs to be IMET inventoried; otherwise, false
        public bool inventoryIMET;
        // True if it needs to be State of MD inventoried, otherwise false
        public bool inventoryMD;
        // The number of the item in the vendor's catalog
        public string itemnumber;
        public LineItemState state;

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
            state = LineItemState.opened;
            qtyreceived = -1;
            inventoryIMET = false;
            inventoryMD = false;
        }

        // The requestid is passed in because a.) there's no other need for this item to references its parent, and b.) I'm lazy.
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

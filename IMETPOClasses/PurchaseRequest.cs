using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace IMETPOClasses
{
    public class PurchaseRequest
    {
        public enum RequestState
        {
            opened = -1,
            pending = 0,
            approved = 2,
            rejected = 4,
            received = 8,
            deleted = 16,
            purchased = 32,
            closed = 64
        }

        public Guid GetTransactionUser(RequestTransaction.TransactionType inType)
        {
            Guid ret = Guid.Empty;
            foreach (RequestTransaction t in history)
            {
                if (t.transaction == inType)
                {
                    ret = t.userid;
                }
            }
            return ret;
        }

        public static string GetRequestStateString(RequestState inPerm)
        {
            switch (inPerm)
            {
                case RequestState.opened:
                    return "New";
                case RequestState.pending:
                    return "Pending";
                case RequestState.approved:
                    return "Approved";
                case RequestState.rejected:
                    return "Rejected";
                case RequestState.received:
                    return "Received";
                case RequestState.deleted:
                    return "Deleted";
                case RequestState.closed:
                    return "Closed";
                case RequestState.purchased:
                    return "Purchased";
            }
            return string.Empty;
        }
        public Guid requestid;
        public User userid;
        public string neededby;
        public RequestState state;
        public Vendor vendorid;
        public string description;
        public string requestornotes;
        public string executornotes;
        public string purchasernotes;
        public string ponumber;
        public string tagnumber;
        public string paymenttype;
        public float shipcharge;
        public float taxcharge;
        public float misccharge;
        public FASNumber fasnumber;
        public string fasnumberstring
        {
            get
            {
                if (fasnumber == null)
                {
                    return string.Empty;
                }
                return fasnumber.Number;
            }
        }
        public List<LineItem> lineitems;
        public List<RequestTransaction> history;
        public string requisitionnumber;

        public PurchaseRequest()
        {
            requestid = Guid.Empty;
            userid = null;
            neededby = string.Empty;
            state = RequestState.opened;
            vendorid = null;
            description = string.Empty;
            requestornotes = string.Empty;
            executornotes = string.Empty;
            purchasernotes = string.Empty;
            ponumber = string.Empty;
            tagnumber = string.Empty;
            paymenttype = string.Empty;
            shipcharge = 0;
            taxcharge = 0;
            misccharge = 0;
            fasnumber = null;
            lineitems = new List<LineItem>();            
            history = new List<RequestTransaction>();
            requisitionnumber = string.Empty;
        }

        public void Save(SqlConnection conn)
        {
            if (requestid == Guid.Empty)
            {
                requestid = Guid.NewGuid();
            }

            SqlCommand cmd = new SqlCommand()
            {
                Connection = conn,
                CommandType = CommandType.StoredProcedure,
                CommandText = "sp_saverequest"
            };           
            cmd.Parameters.Add(new SqlParameter("@inid", requestid));
            if(userid != null)
                cmd.Parameters.Add(new SqlParameter("@inuserid", userid.userid));
            cmd.Parameters.Add(new SqlParameter("@inneededby", neededby));
            cmd.Parameters.Add(new SqlParameter("@instate", (int)state));
            if(vendorid != null)
                cmd.Parameters.Add(new SqlParameter("@invendorid", vendorid.vendorid));
            cmd.Parameters.Add(new SqlParameter("@indescription", description));
            cmd.Parameters.Add(new SqlParameter("@inrequestornotes", requestornotes));
            cmd.Parameters.Add(new SqlParameter("@inexecutornotes", executornotes));
            cmd.Parameters.Add(new SqlParameter("@inpurchasernotes", purchasernotes));
            cmd.Parameters.Add(new SqlParameter("@inponumber", ponumber));
            cmd.Parameters.Add(new SqlParameter("@intagnumber", tagnumber));
            cmd.Parameters.Add(new SqlParameter("@inpaymenttype", paymenttype));
            cmd.Parameters.Add(new SqlParameter("@inshipcarge", shipcharge));
            cmd.Parameters.Add(new SqlParameter("@intaxcharge", taxcharge));
            cmd.Parameters.Add(new SqlParameter("@inmisccharge", misccharge));
            cmd.Parameters.Add(new SqlParameter("@infasnumber", fasnumber.Number));
            cmd.Parameters.Add(new SqlParameter("@inrequisitionnumber", requisitionnumber));

            cmd.ExecuteNonQuery();
            foreach (LineItem li in lineitems)
            {
                li.Save(conn, requestid);
            }
            foreach (RequestTransaction t in history)
            {
                if (!t.isLogged)
                {
                    t.Save(conn, requestid);
                }
            }
        }
        public void Load(SqlConnection conn, Guid inid)
        {
            SqlCommand cmd = new SqlCommand()
            {
                Connection = conn,
                CommandText = "sp_loadrequest",
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add(new SqlParameter("@inrequestid", inid));
            SqlDataReader reader = cmd.ExecuteReader();
            Guid newuserid = Guid.Empty;
            Guid newvendorid = Guid.Empty;
            string tmp_fasnumber = string.Empty;

            while (reader.Read())
            {
                if (!reader.IsDBNull(reader.GetOrdinal("id")))
                {
                    requestid = new Guid(reader["id"].ToString());
                }
                if (!reader.IsDBNull(reader.GetOrdinal("userid")))
                {
                    newuserid = new Guid(reader["userid"].ToString());
                }
                if (!reader.IsDBNull(reader.GetOrdinal("neededby")))
                {
                    neededby = reader["neededby"].ToString();
                }
                if (!reader.IsDBNull(reader.GetOrdinal("state")))
                {
                    state = (RequestState)int.Parse(reader["state"].ToString());
                }
                if (!reader.IsDBNull(reader.GetOrdinal("vendorid")))
                {
                    newvendorid = new Guid(reader["vendorid"].ToString());
                }
                if (!reader.IsDBNull(reader.GetOrdinal("description")))
                {
                    description = reader["description"].ToString();
                }
                if (!reader.IsDBNull(reader.GetOrdinal("requestornotes")))
                {
                    requestornotes = reader["requestornotes"].ToString();
                }
                if (!reader.IsDBNull(reader.GetOrdinal("executornotes")))
                {
                    executornotes = reader["executornotes"].ToString();
                }
                if (!reader.IsDBNull(reader.GetOrdinal("purchasernotes")))
                {
                    purchasernotes = reader["purchasernotes"].ToString();
                }
                if (!reader.IsDBNull(reader.GetOrdinal("ponumber")))
                {
                    ponumber = reader["ponumber"].ToString();
                }
                if (!reader.IsDBNull(reader.GetOrdinal("tagnumber")))
                {
                    tagnumber = reader["tagnumber"].ToString();
                }
                if (!reader.IsDBNull(reader.GetOrdinal("paymenttype")))
                {
                    paymenttype = reader["paymenttype"].ToString();
                }
                if (!reader.IsDBNull(reader.GetOrdinal("shipcharge")))
                {
                    shipcharge = float.Parse(reader["shipcharge"].ToString());
                }
                if (!reader.IsDBNull(reader.GetOrdinal("taxcharge")))
                {
                    taxcharge = float.Parse(reader["taxcharge"].ToString());
                }
                if (!reader.IsDBNull(reader.GetOrdinal("misccharge")))
                {
                    misccharge = float.Parse(reader["misccharge"].ToString());
                }
                if (!reader.IsDBNull(reader.GetOrdinal("fasnumber")))
                {
                    tmp_fasnumber = reader["fasnumber"].ToString();
                }
                if (!reader.IsDBNull(reader.GetOrdinal("requisitionnumber")))
                {
                    requisitionnumber = reader["requisitionnumber"].ToString();
                }
            }
            reader.Close();
            userid = null;
            if (newuserid != Guid.Empty)
            {
                userid = new User();
                userid.Load(conn, newuserid);
            }
            if (newvendorid != Guid.Empty)
            {
                vendorid = new Vendor();
                vendorid.Load(conn, newvendorid);
            }
            if (!string.IsNullOrEmpty(tmp_fasnumber))
            {
                fasnumber = new FASNumber();
                fasnumber.Load(conn, tmp_fasnumber);
            }
            cmd = new SqlCommand()
            {
                Connection = conn,
                CommandText = "sp_loadlineitems",
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add(new SqlParameter("@inrequestid", inid));
            reader = cmd.ExecuteReader();
            lineitems = new List<LineItem>();
            while (reader.Read())
            {
                if(!reader.IsDBNull(reader.GetOrdinal("id")))
                {
                    LineItem li = new LineItem();
                    li.qty = int.Parse(reader["qty"].ToString());
                    li.unit = reader["unit"].ToString();
                    li.description = reader["description"].ToString();
                    li.url = reader["url"].ToString();
                    li.unitprice = float.Parse(reader["unitprice"].ToString());
                    li.state = (LineItem.LineItemState)int.Parse(reader["state"].ToString());
                    li.qtyreceived = int.Parse(reader["qtyreceived"].ToString());
                    li.inventoryIMET = bool.Parse(reader["inventoryimet"].ToString());
                    li.inventoryMD = bool.Parse(reader["inventorymd"].ToString());
                    li.itemnumber = reader["itemnumber"].ToString();
                    li.lineitemid = new Guid(reader["id"].ToString());
                    lineitems.Add(li);
                }
            }
            reader.Close();

            cmd = new SqlCommand()
            {
                Connection = conn,
                CommandText = "sp_loadTransactions",
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add(new SqlParameter("@inrequestid", inid));
            reader = cmd.ExecuteReader();
            history = new List<RequestTransaction>();
            while (reader.Read())
            {
                if (!reader.IsDBNull(reader.GetOrdinal("requestid")))
                {
                    RequestTransaction t = new RequestTransaction();
                    t.transaction = (RequestTransaction.TransactionType)int.Parse(reader["transaction_type"].ToString());
                    t.comments = reader["comments"].ToString();
                    t.timestamp = DateTime.Parse(reader["transaction_time"].ToString());
                    t.userid = new Guid(reader["userid"].ToString());
                    t.username = reader["username"].ToString();
                    t.isLogged = true;
                    history.Add(t);
                }
            }
            reader.Close();
        }

        public static string GenerateTagNumber(SqlConnection conn, string username)
        {            
            string fiscal_year = Utils.GetSystemSetting(conn, "fiscalyear") + "-";
            SqlCommand cmd = new SqlCommand()
            {
                Connection = conn,
                CommandText = "sp_getlasttagnumber",
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add(new SqlParameter("@inprefix", fiscal_year));
            SqlDataReader reader = cmd.ExecuteReader();
            string last_tag_number = string.Empty;
            while (reader.Read())
            {
                if(!reader.IsDBNull(reader.GetOrdinal("tagnumber")))
                    last_tag_number = reader["tagnumber"].ToString();
            }   
            reader.Close();     
            if(string.IsNullOrEmpty(last_tag_number))
            {
                return fiscal_year + "000";
            }
            else
            {
                char[] delim = {'-'};
                string[] tokens = last_tag_number.Split(delim);
                int last_val = int.Parse(tokens[1]) + 1;
                return fiscal_year + string.Format("{0:000}", last_val);
            }
        }

        public float LineItemTotal
        {
            get
            {
                float ret = 0;
                foreach (LineItem li in lineitems)
                {
                    ret += li.TotalPrice;
                }
                return ret;
            }
        }

        public float TotalPrice
        {
            get
            {
                float ret = misccharge + shipcharge + taxcharge;
                foreach (LineItem li in lineitems)
                {
                    ret += li.TotalPrice;
                }
                return ret;
            }
        }
    }
}

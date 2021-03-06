﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace IMETPOClasses
{
    /// <summary>
    ///  A class that encapsulates the data and metadata associated with a purchase request.
    /// </summary>
    public class PurchaseRequest
    {
        // The request state maps to an int, which is what is stored in the database.  I formulated the values so that they could be OR'd together...force of habit, you would never need to do that.
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

        // A convenience function to return the first user that execute a transaction of type inType
        // In reality, this is just used to find the person who opened the request
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

        // Map the state to a string, for display purposes
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
        // The owner; surprisingly, this is not often used, as the history tracks who did what with the request.
        public User userid;
        // This is currently used for nothing; holdover from the original.
        public string neededby;
        public RequestState state;
        // The vendor for this purchase
        public Vendor vendorid;
        // This is labeled "justification" on the form
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
        // The account associated with this request
        public FASNumber fasnumber;
        // The alternate account for orders split between accounts (TODO: Just make it a list, to make it more scalable)
        public FASNumber alt_fasnumber;
        // Any files attached to this request.
        public List<AttachedFile> attachments;
        // The URL to the shopping cart for this request.
        public string shoppingcarturl;

        // Another convenience method: return the string component of the associated account.
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

        // Another convenience method: return the string component of the associated secondary account.
        public string alt_fasnumberstring
        {
            get
            {
                if (alt_fasnumber == null)
                {
                    return string.Empty;
                }
                return alt_fasnumber.Number;
            }
        }
        // The line items associated with this request.
        public List<LineItem> lineitems;
        // The history of this item.
        public List<RequestTransaction> history;
        // The requisition number for this order.
        public string requisitionnumber;

        /// <summary>
        /// This is a convenience function that mass-updates the line item state, ignoring deleted line items and items that need to be inventoried.
        /// </summary>
        /// <param name="inState">The new state for the line items.</param>
        public void SetLineItemState(LineItem.LineItemState inState)
        {
            foreach (LineItem li in lineitems)
            {
                if (li.state.HasFlag(LineItem.LineItemState.deleted))
                    continue;
                li.state |= inState;
            }
        }

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
            alt_fasnumber = null;
            lineitems = new List<LineItem>();            
            history = new List<RequestTransaction>();
            requisitionnumber = string.Empty;
            shoppingcarturl = string.Empty;
            attachments = new List<AttachedFile>();
        }

        /// <summary>
        ///  Save this request's data to the backing database
        /// </summary>
        /// <param name="conn">An open connection to the backing database.</param>
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
            if (fasnumber != null)
            {
                cmd.Parameters.Add(new SqlParameter("@infasnumber", fasnumber.Number));
            }
            else
            {
                cmd.Parameters.Add(new SqlParameter("@infasnumber", string.Empty));
            }
            if (alt_fasnumber != null)
            {
                cmd.Parameters.Add(new SqlParameter("@inalt_fasnumber", alt_fasnumber.Number));
            }
            cmd.Parameters.Add(new SqlParameter("@inrequisitionnumber", requisitionnumber));
            cmd.Parameters.Add(new SqlParameter("@inshoppingcarturl", shoppingcarturl));

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
            foreach (AttachedFile f in attachments)
            {
                f.Save(conn, requestid);
            }
        }

        /// <summary>
        /// Load the backing data for a request into this request.
        /// </summary>
        /// <param name="conn">An open connection to the backing database.</param>
        /// <param name="inid">The ID of the request to load.</param>
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
            string tmp_alt_fasnumber = string.Empty;
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
                if (!reader.IsDBNull(reader.GetOrdinal("alt_fasnumber")))
                {
                    tmp_alt_fasnumber = reader["alt_fasnumber"].ToString();
                }
                if (!reader.IsDBNull(reader.GetOrdinal("requisitionnumber")))
                {
                    requisitionnumber = reader["requisitionnumber"].ToString();
                }
                if (!reader.IsDBNull(reader.GetOrdinal("shoppingcarturl")))
                {
                    shoppingcarturl = reader["shoppingcarturl"].ToString();
                }
            }
            reader.Close();
            userid = null;
            // This section loads all of the related entities
            // The owner
            if (newuserid != Guid.Empty)
            {
                userid = new User();
                userid.Load(conn, newuserid);
            }
            // The vendor
            if (newvendorid != Guid.Empty)
            {
                vendorid = new Vendor();
                vendorid.Load(conn, newvendorid);
            }
            // The account
            if (!string.IsNullOrEmpty(tmp_fasnumber))
            {
                fasnumber = new FASNumber();
                fasnumber.Load(conn, tmp_fasnumber);
            }
            // The co-account
            if (!string.IsNullOrEmpty(tmp_alt_fasnumber))
            {
                alt_fasnumber = new FASNumber();
                alt_fasnumber.Load(conn, tmp_alt_fasnumber);
            }
            // The line items
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

            // The history
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
                    t.userfullname = reader["fullname"].ToString();
                    t.isLogged = true;
                    history.Add(t);
                }
            }
            reader.Close();

            // The attached files
            cmd = new SqlCommand()
            {
                Connection = conn,
                CommandText = "sp_loadattachedfiles",
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add(new SqlParameter("@inrequestid", inid));
            reader = cmd.ExecuteReader();
            attachments = new List<AttachedFile>();
            while (reader.Read())
            {
                AttachedFile f = new AttachedFile();
                if (!reader.IsDBNull(reader.GetOrdinal("uploadedfile_id")))
                {
                    f.ID = new Guid(reader["uploadedfile_id"].ToString());
                }
                if (!reader.IsDBNull(reader.GetOrdinal("filepath")))
                {
                    f.Path = reader["filepath"].ToString();
                }
                if (!reader.IsDBNull(reader.GetOrdinal("filename")))
                {
                    f.Filename = reader["filename"].ToString();
                }
                attachments.Add(f);
            }
            reader.Close();
        }

        // Generate a new tag number, based on the current fiscal year.
        // NOTE: There is a timing issue here, wherein two requests could conceivably generate the same number.  
        // If it actually ever happens, I will devise a locking strategy.
        public static string GenerateTagNumber(SqlConnection conn, string username)
        {            
            // Fetch the fiscal year (stored in web.config)
            SqlCommand cmd = null;

            cmd = new SqlCommand()
            {
                Connection = conn,
                CommandText = "sp_getlasttagnumber",
                CommandType = CommandType.StoredProcedure
            };
            SqlDataReader reader = cmd.ExecuteReader();
            string last_tag_number = string.Empty;
            // Read the last tag number
            while (reader.Read())
            {
                if(!reader.IsDBNull(reader.GetOrdinal("tagnumber")))
                    last_tag_number = reader["tagnumber"].ToString();
            }   
            reader.Close();     
            // If the last tag number is empty, then start with 000
            if(string.IsNullOrEmpty(last_tag_number))
            {
                return "0001";
            }
            else
            {
                // Otherwise, split the last tag number
                int new_val = int.Parse(last_tag_number) + 1;
                // Increment by 1
                return string.Format("{0:0000}", new_val);
            }
        }

        // Return the sum of the line item prices (excluding additional charges)
        public float LineItemTotal
        {
            get
            {
                float ret = 0;
                foreach (LineItem li in lineitems)
                {
                    if(li.state != LineItem.LineItemState.deleted)
                        ret += li.TotalPrice;
                }
                return ret;
            }
        }

        // Return the price of the entire request, including additional charges.
        public float TotalPrice
        {
            get
            {
                float ret = misccharge + shipcharge + taxcharge;
                foreach (LineItem li in lineitems)
                {
                    if (li.state != LineItem.LineItemState.deleted)
                        ret += li.TotalPrice;
                }
                return ret;
            }
        }
    }
}

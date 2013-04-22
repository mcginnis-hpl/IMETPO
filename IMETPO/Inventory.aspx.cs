using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;
using IMETPOClasses;

namespace IMETPO
{
    /// <summary>
    /// The Inventory page; this is a specialized form of the Search functionality, which digs out line items instead of requests.  It shows which items are flagged
    /// as requiring inventory, but have not been flagged as inventoried.
    /// </summary>
    public partial class Inventory : imetspage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsAuthenticated)
            {
                string url = "Default.aspx?RETURNURL=" + Request.Url.ToString();
                Response.Redirect(url, false);
                return;
            }
            try
            {
                PopulateHeader(titlespan);
                if (!IsPostBack)
                {
                    chkIMETInventory.Checked = true;
                    chkMDInventory.Checked = true;
                    Search();
                }
                else if (!string.IsNullOrEmpty(hiddenInventoried.Value))
                {
                    Flag(new Guid(hiddenInventoried.Value));
                    hiddenInventoried.Value = string.Empty;
                }
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        /// <summary>
        /// Flag a single item as inventoried.
        /// </summary>
        /// <param name="lineitemid">The ID of the item to flag</param>
        protected void Flag(Guid lineitemid)
        {
            SqlConnection conn = ConnectToConfigString("imetpsconnection");
            // Get the ID of the parent request.
            string query = "SELECT requestid FROM lineitems WHERE id='" + lineitemid.ToString() + "'";
            SqlCommand cmd = new SqlCommand()
            {
                Connection = conn,
                CommandType = CommandType.Text,
                CommandText = query
            };
            SqlDataReader reader = cmd.ExecuteReader();
            Guid requestid = Guid.Empty;
            while (reader.Read())
            {
                requestid = new Guid(reader["requestid"].ToString());
            }
            reader.Close();
            if (requestid != Guid.Empty)
            {
                // Load the request from the ID.
                PurchaseRequest req = new PurchaseRequest();
                req.Load(conn, requestid);
                // Find the matching line item
                foreach (LineItem li in req.lineitems)
                {
                    if (li.lineitemid == lineitemid)
                    {
                        // Flag the item as closed
                        li.state |= LineItem.LineItemState.closed;
                        // Note the flagging in the request history.
                        req.history.Add(new RequestTransaction(RequestTransaction.TransactionType.Modified, CurrentUser.userid, CurrentUser.username, "Line item " + li.itemnumber + " inventoried.", CurrentUser.FullName));
                        req.Save(conn);
                    }
                }
            }
            conn.Close();
            Search();
        }

        /// <summary>
        /// Load all line items that require inventory.
        /// </summary>
        protected void Search()
        {

            SqlConnection conn = ConnectToConfigString("imetpsconnection");
            string query = "SELECT lineitemid, requestid, qty, unit, description, itemnumber, inventoryimet, inventorymd, qtyreceived, unitprice, fasnumber FROM v_inventory_items";
            bool init = false;
            // Add a where clause for IMET inventory
            if (chkIMETInventory.Checked)
            {
                if (!init)
                {
                    query += " WHERE";
                }
                else
                {
                    query += " OR";
                }
                init = true;
                query += " inventoryimet=1";
            }
            // Add a "where" clause for MD inventory
            if (chkMDInventory.Checked)
            {
                if (!init)
                {
                    query += " WHERE";
                }
                else
                {
                    query += " OR";
                }
                init = true;
                query += " inventorymd=1";
            }
            SqlCommand cmd = new SqlCommand()
            {
                Connection = conn,
                CommandType = CommandType.Text,
                CommandText = query
            };
            // Build a sortable table of the results
            SqlDataReader reader = cmd.ExecuteReader();
            string html = "<table class='example table-autosort:0 table-stripeclass:alternate'><thead><tr>";
            html += "<th class='table-sortable:default'>Account</th>";
            html += "<th class='table-sortable:numeric'>Quantity</th>";
            html += "<th class='table-sortable:default'>UOM</th>";
            html += "<th class='table-sortable:default'>Description</th>";
            html += "<th class='table-sortable:default'>Item Number</th>";
            html += "<th class='table-sortable:default'>IMET Inv.</th>";
            html += "<th class='table-sortable:default'>MD Inv.</th>";
            html += "<th class='table-sortable:numeric'>Qty. Received</th>";
            html += "<th class='table-sortable:numeric'>Unit Price</th>";
            html += "<th></th>";
            html += "</tr></thead><tbody>";
            while (reader.Read())
            {
                html += "<tr>";
                html += "<td>" + reader["fasnumber"].ToString() + "</td>";
                html += "<td>" + reader["qty"].ToString() + "</td>";
                html += "<td>" + reader["unit"].ToString() + "</td>";
                html += "<td>" + reader["description"].ToString() + "</td>";
                html += "<td>" + reader["itemnumber"].ToString() + "</td>";
                bool inventory_imet = bool.Parse(reader["inventoryimet"].ToString());
                if (inventory_imet)
                {
                    html += "<td>&#x2713;</td>";
                }
                else
                {
                    html += "<td></td>";
                }
                bool inventory_md = bool.Parse(reader["inventorymd"].ToString());
                if (inventory_md)
                {
                    html += "<td>&#x2713;</td>";
                }
                else
                {
                    html += "<td></td>";
                }
                html += "<td>" + reader["qtyreceived"].ToString() + "</td>";
                html += "<td>" + reader["unitprice"].ToString() + "</td>";
                html += "<td><a class='squarebutton' href='SubmitRequest.aspx?REQUESTID=" + reader["requestid"].ToString() + "'><span>View</span></a>";
                // Here is the javascript that actually does the "flagging" as inventoried.
                html += "&nbsp;&nbsp;&nbsp;<a class='squarebutton' href='javascript:FlagAsInventoried(\"" + reader["lineitemid"].ToString() + "\")'><span>Mark as Inventoried</span></a>";
                html += "</td>";
                html += "</tr>";
            }
            html += "</tbody></table>";
            results.InnerHtml = html;
            reader.Close();
            conn.Close();
        }

        /// <summary>
        /// Re-search the database when the IMET checkbox changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void chkIMETInventory_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                Search();
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        /// <summary>
        /// Re-search the database when the MD checkbox changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void chkMDInventory_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                Search();
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }
    }
}
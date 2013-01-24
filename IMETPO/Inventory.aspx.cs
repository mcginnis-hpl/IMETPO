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
    public partial class Inventory : imetspage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsAuthenticated)
            {
                string url = "Default.aspx?RETURNURL=" + Request.Url.ToString();
                Response.Redirect(url);
                return;
            }
            try
            {
                PopulateHeader(appTitle, appSubtitle);
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

        protected void Flag(Guid lineitemid)
        {
            SqlConnection conn = ConnectToConfigString("imetpsconnection");
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
                PurchaseRequest req = new PurchaseRequest();
                req.Load(conn, requestid);
                foreach (LineItem li in req.lineitems)
                {
                    if (li.lineitemid == lineitemid)
                    {
                        li.inventoryMD = false;
                        li.inventoryIMET = false;
                        req.history.Add(new RequestTransaction(RequestTransaction.TransactionType.Modified, CurrentUser.userid, CurrentUser.username, "Line item " + li.itemnumber + " inventoried."));
                        req.Save(conn);
                    }
                }
            }
            conn.Close();
            Search();
        }

        protected void Search()
        {
            SqlConnection conn = ConnectToConfigString("imetpsconnection");
            string query = "SELECT lineitemid, requestid, qty, unit, description, itemnumber, inventoryimet, inventorymd, qtyreceived, unitprice, fasnumber FROM v_inventory_items";
            bool init = false;
            if (chkIMETInventory.Checked)
            {
                if (!init)
                {
                    query += " WHERE";
                }
                else
                {
                    query += " AND";
                }
                init = true;
                query += " inventoryimet=1";
            }
            if (chkMDInventory.Checked)
            {
                if (!init)
                {
                    query += " WHERE";
                }
                else
                {
                    query += " AND";
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
                html += "<td><a href='SubmitRequest.aspx?REQUESTID=" + reader["requestid"].ToString() + "'>View</a>";
                html += "&nbsp;&nbsp;&nbsp;<a href='javascript:FlagAsInventoried(\"" + reader["lineitemid"].ToString() + "\")'>Mark as Inventoried</a>";
                html += "</td>";
                html += "</tr>";
            }
            html += "</tbody></table>";
            results.InnerHtml = html;
            reader.Close();
            conn.Close();
        }

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
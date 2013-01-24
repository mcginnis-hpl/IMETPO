using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using IMETPOClasses;
using System.Data;
using System.Data.SqlClient;

namespace IMETPO
{
    public partial class ViewRequest : imetspage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (!IsAuthenticated)
                {
                    string url = "Default.aspx?RETURNURL=" + Request.Url.ToString();
                    Response.Redirect(url);
                    return;
                }
            }
            try {
                logoImage.Src = GetApplicationSetting("printLogoURL");
                logoImage.Alt = GetApplicationSetting("printLogoAlt");
            }
            catch (Exception)
            {
            }
            Guid requestid = Guid.Empty;
            for (int i = 0; i < Request.Params.Count; i++)
            {
                if (Request.Params.GetKey(i).ToUpper() == "REQUESTID")
                {
                    requestid = new Guid(Request.Params[i]);
                }
            }
            if (requestid != Guid.Empty)
            {
                SqlConnection conn = ConnectToConfigString("imetpsconnection");
                try
                {
                    PurchaseRequest p = new PurchaseRequest();
                    p.Load(conn, requestid);
                    PopulateData(conn, p);
                }
                catch (Exception ex)
                {
                    HandleError(ex);
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        protected void BuildHistory(PurchaseRequest working)
        {
            string html = "<h4>Purchase Request History</h4><table class='history'>";
            html += "<tr><th>Transaction</th><th>User</th><th>Timestamp</th><th>Notes</th></tr>";
            foreach (RequestTransaction t in working.history)
            {
                html += "<tr>";
                html += "<td>" + RequestTransaction.TransactionTypeString(t.transaction) + "</td>";
                html += "<td>" + t.username + "</td>";
                html += "<td>" + t.timestamp.ToString() + "</td>";
                html += "<td>" + t.comments + "</td>";
                html += "</tr>";
            }
            html += "</table>";
            history.InnerHtml = html;
        }

        protected void PopulateData(SqlConnection conn, PurchaseRequest req)
        {
            span_executornotes.InnerHtml = req.executornotes;
            span_fasnumber.InnerHtml = "<h1>" + req.fasnumber.Number + "</h1>";
            span_fasnumber2.InnerHtml = req.fasnumberstring;
            span_justification.InnerHtml = req.description;
            span_misccharges.InnerHtml = string.Format("{0:C}", req.misccharge);
            span_purchasernotes.InnerHtml = req.purchasernotes;
            span_reqnumber.InnerHtml = req.tagnumber;
            span_requestornotes.InnerHtml = req.requestornotes;
            span_shippingcharges.InnerHtml = string.Format("{0:C}", req.shipcharge);
            span_tagnumber.InnerHtml = req.tagnumber;
            foreach (RequestTransaction t in req.history)
            {
                if (t.transaction == RequestTransaction.TransactionType.Opened)
                {
                    span_requestor.InnerHtml = t.username;
                }
            }
            span_taxcharges.InnerHtml = string.Format("{0:C}", req.taxcharge);
            span_totalprice.InnerHtml = string.Format("{0:C}", req.TotalPrice);
            if(req.vendorid != null)
            {
                string vendorinfo = "<table border='0'>";
                vendorinfo += "<tr><td>" + req.vendorid.vendorname + "</td></tr>";
                if(!string.IsNullOrEmpty(req.vendorid.address1))
                    vendorinfo += "<tr><td>" + req.vendorid.address1 + "</td></tr>";
                if (!string.IsNullOrEmpty(req.vendorid.address2))
                    vendorinfo += "<tr><td>" + req.vendorid.address2 + "</td></tr>";
                bool needs_close = false;
                if (!string.IsNullOrEmpty(req.vendorid.city))
                {
                    if(!needs_close)
                        vendorinfo += "<tr><td>";
                    vendorinfo += req.vendorid.city;
                    needs_close = true;
                }
                if (!string.IsNullOrEmpty(req.vendorid.st))
                {
                    if (needs_close)
                    {
                        vendorinfo += ", ";
                    }
                    else                            
                    {
                        vendorinfo += "<tr><td>";
                    }
                    vendorinfo += req.vendorid.st;
                    needs_close = true;
                }
                if (!string.IsNullOrEmpty(req.vendorid.zip))
                {
                    if (needs_close)
                    {
                        vendorinfo += " ";
                    }
                    else
                    {
                        vendorinfo += "<tr><td>";
                    }
                    vendorinfo += req.vendorid.zip;
                    needs_close = true;
                }
                if (needs_close)
                    vendorinfo += "</td></tr>";
                needs_close = false;
                if (!string.IsNullOrEmpty(req.vendorid.phone))
                {
                    vendorinfo += "<tr><td>P: " + req.vendorid.phone;
                    needs_close = true;
                }
                if (!string.IsNullOrEmpty(req.vendorid.fax))
                {
                    if (needs_close)
                        vendorinfo += "  ";
                    else
                        vendorinfo += "<tr><td>";
                    vendorinfo += "F: " + req.vendorid.fax;
                    needs_close = true;
                }
                if (needs_close)
                    vendorinfo += "</td></tr>";
                if(!string.IsNullOrEmpty(req.vendorid.url))
                    vendorinfo += "<tr><td>WWW: " + req.vendorid.url + "</td></tr>";
                vendorinfo += "</table>";
                span_vendorinfo.InnerHtml = vendorinfo;
            }
            BuildHistory(req);
            PopulateLineItems(req);
        }

        protected void PopulateLineItems(PurchaseRequest working)
        {
            foreach (LineItem l in working.lineitems)
            {
                TableRow tr = new TableRow();
                TableCell td = new TableCell();
                td.Text = l.qty.ToString();
                tr.Cells.Add(td);

                td = new TableCell();
                td.Text = l.unit;
                tr.Cells.Add(td);

                td = new TableCell();
                td.Text = l.itemnumber;
                tr.Cells.Add(td);
                
                td = new TableCell();
                td.Text = l.description;
                tr.Cells.Add(td);

                td = new TableCell();
                td.Text = string.Format("{0:C}", l.unitprice);
                tr.Cells.Add(td);

                td = new TableCell();
                td.Text = string.Format("{0:C}", l.TotalPrice);
                tr.Cells.Add(td);

                td = new TableCell();
                td.Text = l.qtyreceived.ToString();
                tr.Cells.Add(td);

                tbldetails.Rows.AddAt(1, tr);
            }
        }
    }
}
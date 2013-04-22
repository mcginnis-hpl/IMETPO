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
    /// <summary>
    /// ViewRequest is the print version of the Purchase Request page.  
    /// </summary>
    public partial class ViewRequest : imetspage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (!IsAuthenticated)
                {
                    string url = "Default.aspx?RETURNURL=" + Request.Url.ToString();
                    Response.Redirect(url, false);
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
            bool is_ack = false;
            Guid requestid = Guid.Empty;
            // Pull the ID of the request from the parameters.
            for (int i = 0; i < Request.Params.Count; i++)
            {
                if (Request.Params.GetKey(i).ToUpper() == "REQUESTID")
                {
                    requestid = new Guid(Request.Params[i]);
                }
                if (Request.Params.GetKey(i).ToUpper() == "ACK")
                {
                    if (Request.Params[i] == "1")
                    {
                        is_ack = true;
                    }
                }
            }
            // Populate the page with data from the request.
            if (requestid != Guid.Empty)
            {
                SqlConnection conn = ConnectToConfigString("imetpsconnection");
                try
                {
                    PurchaseRequest p = new PurchaseRequest();
                    p.Load(conn, requestid);
                    PopulateData(conn, p, is_ack);
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

        /// <summary>
        /// Build the history portion of the page from the transaction history of the request.
        /// </summary>
        /// <param name="working"></param>
        protected void BuildHistory(PurchaseRequest working)
        {
            string html = "<h4>Purchase Request History</h4><table class='history'>";
            html += "<tr><th>Transaction</th><th>User</th><th>Timestamp</th><th>Notes</th></tr>";
            // This is pretty easy; just put the RequestTransaction info in a table.
            foreach (RequestTransaction t in working.history)
            {
                if (t.transaction == RequestTransaction.TransactionType.Opened)
                    continue;
                html += "<tr>";
                html += "<td>" + RequestTransaction.TransactionTypeString(t.transaction) + "</td>";
                if (!string.IsNullOrEmpty(t.userfullname.Trim()))
                {
                    html += "<td>" + t.userfullname + "</td>";
                }
                else
                {
                    html += "<td>" + t.username + "</td>";
                }
                html += "<td>" + t.timestamp.ToString() + "</td>";
                html += "<td>" + t.comments + "</td>";
                html += "</tr>";
            }
            html += "</table>";
            history.InnerHtml = html;
        }

        /// <summary>
        /// This function populates all of the portions of the page with data from req.
        /// </summary>
        /// <param name="conn">An open connection to the IMETPS database.</param>
        /// <param name="req">The request whose data we are populating.</param>
        /// <param name="is_ack">A flag inficating whether this is an acknowledgement or not.</param>
        protected void PopulateData(SqlConnection conn, PurchaseRequest req, bool is_ack)
        {
            // All of the text stuff is pretty straightforward.
            span_executornotes.InnerHtml = req.executornotes;
            span_fasnumber.InnerHtml = "<h1>" + req.fasnumber.Number + "</h1>";
            span_fasnumber2.InnerHtml = req.fasnumberstring;
            if (!string.IsNullOrEmpty(req.alt_fasnumberstring))
            {
                span_fasnumber2.InnerHtml += "/" + req.alt_fasnumberstring;
            }
            span_justification.InnerHtml = req.description;
            span_misccharges.InnerHtml = string.Format("{0:C}", req.misccharge);
            span_purchasernotes.InnerHtml = req.purchasernotes;
            span_reqnumber.InnerHtml = req.requisitionnumber;
            span_requestornotes.InnerHtml = req.requestornotes;
            span_shippingcharges.InnerHtml = string.Format("{0:C}", req.shipcharge);
            span_tagnumber.InnerHtml = req.tagnumber;
            // This is a lazy way to pull th ename of the requestor from the request.
            foreach (RequestTransaction t in req.history)
            {
                if (t.transaction == RequestTransaction.TransactionType.Created)
                {
                    span_requestor.InnerHtml = t.userfullname;
                }
            }
            // Format the dollar fields correctly.
            span_taxcharges.InnerHtml = string.Format("{0:C}", req.taxcharge);
            span_totalprice.InnerHtml = string.Format("{0:C}", req.TotalPrice);
            if (req.attachments.Count == 0)
            {
                rowAttachment.Visible = false;
                rowAttachmentHelp.Visible = false;
            }
            else
            {
                // Populate the attachments area
                string linkurl = "<table border='0'>";
                foreach (AttachedFile f in req.attachments)
                {
                    linkurl += "<tr><td><a href='DownloadAttachment.aspx?ATTACHMENTID=" + f.ID.ToString() + "' target='_blank'>Download " + f.Filename + "</a></td></tr>";
                }
                linkurl += "</table>";
                rowAttachment.Visible = true;
                rowAttachmentHelp.Visible = true;
                attachmentlink.InnerHtml = linkurl;
            }
            if (string.IsNullOrEmpty(req.shoppingcarturl))
            {
                rowLink.Visible = false;
                rowLinkHelp.Visible = false;
            }
            else
            {
                rowLink.Visible = true;
                rowLinkHelp.Visible = true;
                shoppingcartlink.InnerHtml = "<a href='" + req.shoppingcarturl + "' target='_blank'>" + req.shoppingcarturl + "</a>";
            }
            top_requisition.InnerHtml = GetApplicationSetting("applicationPrintTitle");
            // Populate the vendor information; it's all just text stuff.
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
                    vendorinfo += "<tr><td>URL: " + req.vendorid.url + "</td></tr>";
                if (!string.IsNullOrEmpty(req.vendorid.contact_name))
                    vendorinfo += "<tr><td>ATTN: " + req.vendorid.contact_name + "</td></tr>";
                if (!string.IsNullOrEmpty(req.vendorid.contact_phone))
                    vendorinfo += "<tr><td>P: " + req.vendorid.contact_phone + "</td></tr>";
                if (!string.IsNullOrEmpty(req.vendorid.contact_email))
                    vendorinfo += "<tr><td>" + req.vendorid.contact_email + "</td></tr>";
                if (!string.IsNullOrEmpty(req.vendorid.customer_account_number))
                    vendorinfo += "<tr><td>Account #: " + req.vendorid.customer_account_number + "</td></tr>";
                vendorinfo += "</table>";
                span_vendorinfo.InnerHtml = vendorinfo;                
            }
            // If this is an acknowledgement, put some buttons at the bottom to go back to the main menu.
            if (is_ack)
            {
                ack_tagnumber.InnerHtml = req.tagnumber;
                string html = "<table><tr><td><a class='squarebutton' href='SubmitRequest.aspx?REQUESTID=" + req.requestid.ToString() + "'><span>Edit this request</span></a></td>";
                html += "<td><a class='squarebutton' href='Default.aspx'><span>Return to main menu</span></a></td></tr></table>";
                ack_controls.InnerHtml = html;
            }
            else
            {
                acknowledgement.Visible = false;
                ack_controls.Visible = false;
            }
            // Print the history
            BuildHistory(req);
            // Print the line items
            PopulateLineItems(req);
        }

        /// <summary>
        /// Iterate through the line items and print them on the form.
        /// </summary>
        /// <param name="working">The purchase request to put on the form.</param>
        protected void PopulateLineItems(PurchaseRequest working)
        {
            foreach (LineItem l in working.lineitems)
            {
                if (l.state == LineItem.LineItemState.deleted)
                    continue;

                // This is just a matter of adding rows to the table -- used ASP because it's cleaner than dumping out a bunch of text.
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
                if(l.qtyreceived < 0)
                    td.Text = "0";
                else
                    td.Text = l.qtyreceived.ToString();
                tr.Cells.Add(td);

                tbldetails.Rows.AddAt(1, tr);
            }
        }
    }
}
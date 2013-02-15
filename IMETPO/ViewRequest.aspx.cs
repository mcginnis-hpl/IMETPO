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

        protected void BuildHistory(PurchaseRequest working)
        {
            string html = "<h4>Purchase Request History</h4><table class='history'>";
            html += "<tr><th>Transaction</th><th>User</th><th>Timestamp</th><th>Notes</th></tr>";
            foreach (RequestTransaction t in working.history)
            {
                if (t.transaction == RequestTransaction.TransactionType.Opened)
                    continue;
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

        protected void PopulateData(SqlConnection conn, PurchaseRequest req, bool is_ack)
        {
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
            foreach (RequestTransaction t in req.history)
            {
                if (t.transaction == RequestTransaction.TransactionType.Created)
                {
                    span_requestor.InnerHtml = t.username;
                }
            }
            span_taxcharges.InnerHtml = string.Format("{0:C}", req.taxcharge);
            span_totalprice.InnerHtml = string.Format("{0:C}", req.TotalPrice);
            if (req.attachments.Count == 0)
            {
                rowAttachment.Visible = false;
                rowAttachmentHelp.Visible = false;
            }
            else
            {
                string linkurl = "<a href='DownloadAttachment.aspx?ATTACHMENTID=" + req.attachments[0].ID.ToString() + "' target='_blank'>Download " + req.attachments[0].Filename + "</a>";
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
                if (is_ack)
                {
                    ack_tagnumber.InnerHtml = req.tagnumber;
                    string html = "<table><tr><td><a class='squarebutton' href='SubmitRequest.aspx?REQUESTID=" +req.requestid.ToString() + "'><span>Edit this request</span></a></td>";
                    html += "<td><a class='squarebutton' href='Default.aspx'><span>Return to main menu</span></a></td></tr></table>";
                    ack_controls.InnerHtml = html;
                }
                else
                {
                    acknowledgement.Visible = false;
                    ack_controls.Visible = false;
                }
            }
            BuildHistory(req);
            PopulateLineItems(req);
        }

        protected void PopulateLineItems(PurchaseRequest working)
        {
            foreach (LineItem l in working.lineitems)
            {
                if (l.state == LineItem.LineItemState.deleted)
                    continue;

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
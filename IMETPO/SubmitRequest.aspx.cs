using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using IMETPOClasses;

namespace IMETPO
{
    /// <summary>
    /// The main page of the application - this is the page that does all the work editing a request.
    /// </summary>
    public partial class SubmitRequest : imetspage
    {
        // The dynamic controls are added in the OnInit event of the page -- this makes it possible to maintain their state and use them later in the postback.
        protected override void OnInit(EventArgs e)
        {
            try
            {
                if (!DesignMode)
                {
                    SqlConnection conn = ConnectToConfigString("imetpsconnection");
                    PurchaseRequest working = (PurchaseRequest)GetSessionValue("WorkingPurchaseRequest");
                    if (working != null)
                    {
                        PopulateData(conn, working, false);
                    }
                    conn.Close();
                }
                base.OnInit(e);
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (!IsPostBack)
                {
                    RemoveSessionValue("WorkingPurchaseRequest");
                    // Bounce the user back to the main page if they are not logged in.
                    if (!IsAuthenticated)
                    {
                        string url = "Default.aspx?RETURNURL=" + Request.Url.ToString();
                        Response.Redirect(url, false);
                        return;
                    }
                    PopulateHeader(titlespan);
                    Guid requestid = Guid.Empty;
                    // Extract the current request ID from the parameters.
                    for (int i = 0; i < Request.Params.Count; i++)
                    {
                        if (Request.Params.GetKey(i).ToUpper() == "REQUESTID")
                        {
                            requestid = new Guid(Request.Params[i]);
                        }
                    }
                    // Load the current request from the ID, if available.
                    SqlConnection conn = ConnectToConfigString("imetpsconnection");
                    PurchaseRequest working = new PurchaseRequest();
                    if (requestid != Guid.Empty)
                    {
                        working.Load(conn, requestid);
                    }
                    else
                    {
                        working.userid = CurrentUser;
                    }
                    PopulateData(conn, working, true);
                    conn.Close();
                    SetSessionValue("WorkingPurchaseRequest", working);
                }
                else
                {
                    if (!string.IsNullOrEmpty(hiddenLineItems.Value))
                    {
                        string[] tokens = hiddenLineItems.Value.Split(":".ToCharArray());
                        if (tokens[0] == "REMOVEATTACHMENT")
                        {
                            RemoveAttachment(new Guid(tokens[1]));
                        }
                        hiddenLineItems.Value = string.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }
        
        // The execution mode determines what controls are visible; it is determiend from the parameters, and from the state of the request.
        protected string GetExecMode(PurchaseRequest working)
        {
            string ret = "submit";
            if (working.state == PurchaseRequest.RequestState.pending || working.state == PurchaseRequest.RequestState.rejected || working.state == PurchaseRequest.RequestState.opened)
            {
                for (int i = 0; i < Request.Params.Count; i++)
                {
                    if (Request.Params.GetKey(i).ToUpper() == "MODE")
                    {
                        ret = Request.Params[i].ToLower();
                        break;
                    }
                }
            }
            else
            {
                switch (working.state)
                {
                    case PurchaseRequest.RequestState.approved:
                        ret = "purchase";
                        break;
                    case PurchaseRequest.RequestState.purchased:
                        ret = "receive";
                        break;
                    case PurchaseRequest.RequestState.received:
                        ret = "close";
                        break;
                    case PurchaseRequest.RequestState.closed:
                        ret = "close";
                        break;
                }
            }
            return ret;
        }

        // Populate the history section of the page.
        protected void BuildHistory(PurchaseRequest working)
        {
            string html = "<h4>Purchase Request History</h4><table class='history'>";
            html += "<tr><th>Transaction</th><th>User</th><th>Timestamp</th><th>Notes</th></tr>";
            // Just iterate throught he history and populate a table with the data from each transaction.
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

       /// <summary>
        /// Populate the page with data from the current request.
       /// </summary>
       /// <param name="conn">An open connection to the IMETPS database</param>
       /// <param name="working">The current purchase request.</param>
       /// <param name="initValues">If true, then put the values from the request in the controls; otherwise, leave the controls as they are.</param>
        protected void PopulateData(SqlConnection conn, PurchaseRequest working, bool initValues)
        {
            string mode = GetExecMode(working);

            if (working.requestid == Guid.Empty || working.state == PurchaseRequest.RequestState.opened || working.state == PurchaseRequest.RequestState.rejected)
            {
                btnDelete.Visible = false;
            }
            else
            {
                btnDelete.Visible = true;
            }
            // Change the title of the form based on the current execution mode.
            if (mode == "submit")
            {
                title.InnerHtml = "Submit a purchase request";
            }
            else if (mode == "edit")
            {
                title.InnerHtml = "Update a purchase request";
            }
            else if (mode == "approve")
            {
                title.InnerHtml = "Update or Approve Pending Purchase Requests";
            }
            else if (mode == "close")
            {
                title.InnerHtml = "Close a purchase request";
            }
            else if (mode == "purchase")
            {
                title.InnerHtml = "Purchase Request Submissions";
            }
            else if (mode == "receive")
            {
                title.InnerHtml = "Receive a purchase request";
            }
            // If initValues, then populate the controls
            if (initValues)
            {
                // Clear the list of FAS numbers
                comboFASNumber.Items.Clear();
                comboalt_FASnumber.Items.Clear();

                // If this is a new request, put all of the FAS numbers available to this account in the combo box.
                List<FASNumber> fasnumbers = null;
                if (mode == "submit" || mode == "edit" || working.fasnumber == null)
                {
                    comboFASNumber.Items.Add(new ListItem(string.Empty, string.Empty));
                    comboalt_FASnumber.Items.Add(new ListItem(string.Empty, string.Empty));
                    fasnumbers = CurrentUser.LoadFASNumbers(conn, IMETPOClasses.User.Permission.requestor);
                    foreach (FASNumber f in fasnumbers)
                    {
                        if (f.Disabled)
                            continue;
                        ListItem li = new ListItem();
                        li.Text = f.Number + " (" + f.Description + ")";
                        li.Value = f.Number;
                        comboFASNumber.Items.Add(li);
                        if (li.Value == working.fasnumberstring)
                        {
                            comboFASNumber.SelectedIndex = comboFASNumber.Items.Count - 1;
                        }

                        li = new ListItem();
                        li.Text = f.Number + " (" + f.Description + ")";
                        li.Value = f.Number;
                        comboalt_FASnumber.Items.Add(li);
                        if (li.Value == working.alt_fasnumberstring)
                        {
                            comboalt_FASnumber.SelectedIndex = comboalt_FASnumber.Items.Count - 1;
                        }
                    }
                    comboFASNumber.Enabled = true;
                    comboalt_FASnumber.Enabled = true;
                }
                // Otherwise, just add the current FAS number to the list, and disable it so it can't be changed,.
                else
                {
                    fasnumbers = new List<FASNumber>();
                    fasnumbers.Add(working.fasnumber);
                    ListItem li = new ListItem();
                    li.Text = working.fasnumber.Number + " (" + working.fasnumber.Description + ")";
                    li.Value = working.fasnumber.Number;
                    comboFASNumber.Items.Add(li);
                    if (working.alt_fasnumber != null)
                    {
                        fasnumbers.Add(working.alt_fasnumber);
                        li = new ListItem();
                        li.Text = working.alt_fasnumber.Number + " (" + working.alt_fasnumber.Description + ")";
                        li.Value = working.alt_fasnumber.Number;
                        comboalt_FASnumber.Items.Add(li);
                    }
                    comboFASNumber.Enabled = false;
                    comboalt_FASnumber.Enabled = false;
                }
                // Build a list of possible requestors
                Guid requestorid = working.GetTransactionUser(RequestTransaction.TransactionType.Opened);
                if (requestorid == Guid.Empty)
                    requestorid = CurrentUser.userid;

                comboRequester.Items.Clear();
                if (CurrentUser.UserPermissions.Contains(IMETPOClasses.User.Permission.globalrequestor))
                {
                    List<IMETPOClasses.User> allusers = IMETPOClasses.User.LoadAllUsers(conn);
                    foreach (IMETPOClasses.User tmpuser in allusers)
                    {
                        comboRequester.Items.Add(new ListItem(tmpuser.username, tmpuser.userid.ToString()));
                        if (tmpuser.userid == requestorid)
                        {
                            comboRequester.SelectedIndex = comboRequester.Items.Count - 1;
                        }
                    }
                }
                else
                {
                    comboRequester.Items.Add(new ListItem(CurrentUser.username, CurrentUser.userid.ToString()));
                    User tmpuser = CurrentUser.parentuser;
                    while (tmpuser != null)
                    {
                        comboRequester.Items.Add(new ListItem(tmpuser.username, tmpuser.userid.ToString()));
                        if (tmpuser.userid == requestorid)
                        {
                            comboRequester.SelectedIndex = comboRequester.Items.Count - 1;
                        }
                        tmpuser = tmpuser.parentuser;
                    }
                }
                if (mode == "submit" || mode == "edit")
                {
                    comboRequester.Enabled = true;
                }
                else
                {
                    comboRequester.Enabled = false;
                }

                // Populate the list of available vendors.
                comboVendors.Items.Clear();
                comboVendors.Items.Add(new ListItem("New Vendor", "NEW"));
                List<Vendor> vendors = Vendor.LoadAllVendors(conn);
                foreach (Vendor v in vendors)
                {
                    comboVendors.Items.Add(new ListItem(v.vendorname, v.vendorid.ToString()));
                    if (working.vendorid != null && working.vendorid.vendorid == v.vendorid)
                    {
                        comboVendors.SelectedIndex = comboVendors.Items.Count - 1;
                    }
                }
                if (mode == "submit" || mode == "edit")
                {
                    comboVendors.Enabled = true;
                }
                else
                {
                    comboVendors.Enabled = false;
                }
                // Populate the vendor information from the currently selected vendor (if any)
                if (comboVendors.SelectedValue == "NEW")
                {
                    txtVendorName.Enabled = true;
                    txtVendorURL.Enabled = true;
                }
                else
                {
                    txtVendorName.Enabled = false;
                    txtVendorURL.Enabled = false;
                }
                if (mode == "submit" || mode == "edit" || mode == "approve")
                {
                    vendorDetails.Visible = false;
                    vendorDetailsHeader.Visible = false;
                    rowVendorContactAccountNumber.Visible = false;
                    rowVendorContactEmail.Visible = false;
                    rowVendorContactName.Visible = false;
                    rowVendorContactPhone.Visible = false;
                }
                else
                {
                    vendorDetails.Visible = true;
                    vendorDetailsHeader.Visible = true;
                }
                if (working.vendorid != null)
                {
                    txtVendorName.Text = working.vendorid.vendorname;
                    txtVendorURL.Text = working.vendorid.url;
                    txtVendorAddress1.Text = working.vendorid.address1;
                    txtVendorAddress2.Text = working.vendorid.address2;
                    txtVendorCity.Text = working.vendorid.city;
                    txtVendorDescription.Text = working.vendorid.description;
                    txtVendorFax.Text = working.vendorid.fax;
                    txtVendorFEIN.Text = working.vendorid.fein;
                    txtVendorPhone.Text = working.vendorid.phone;
                    txtVendorPostalCode.Text = working.vendorid.phone;
                    txtVendorState.Text = working.vendorid.st;
                    txtVendor_customer_account_number.Text = working.vendorid.customer_account_number;
                    txtVendorcontact_name.Text = working.vendorid.contact_name;
                    txtVendorcontact_email.Text = working.vendorid.contact_email;
                    txtVendorcontact_phone.Text = working.vendorid.contact_phone;
                }
                else
                {
                    txtVendorName.Text = string.Empty;
                    txtVendorURL.Text = string.Empty;
                    txtVendorAddress1.Text = string.Empty;
                    txtVendorAddress2.Text = string.Empty;
                    txtVendorCity.Text = string.Empty;
                    txtVendorDescription.Text = string.Empty;
                    txtVendorFax.Text = string.Empty;
                    txtVendorFEIN.Text = string.Empty;
                    txtVendorPhone.Text = string.Empty;
                    txtVendorPostalCode.Text = string.Empty;
                    txtVendorState.Text = string.Empty;
                    txtVendor_customer_account_number.Text = string.Empty;
                    txtVendorcontact_name.Text = string.Empty;
                    txtVendorcontact_email.Text = string.Empty;
                    txtVendorcontact_phone.Text = string.Empty;
                }
                // Populate the list of attachments.
                PopulateAttachments(working);

                if (!string.IsNullOrEmpty(working.tagnumber))
                {
                    lblTagNumber.Text = working.tagnumber;
                }
                else
                {
                    lblTagNumber.Text = string.Empty;
                }
                lblStatus.Text = PurchaseRequest.GetRequestStateString(working.state);

                /*if (CurrentUser.UserPermissions.Contains(IMETPOClasses.User.Permission.globalapprover) && (working.state == PurchaseRequest.RequestState.pending || working.state == PurchaseRequest.RequestState.recjected))
                {
                    rowBypass.Visible = true;
                    Guid curr_requestor = new Guid(comboRequester.SelectedValue);
                    if (curr_requestor != CurrentUser.userid)
                    {
                        rowBypass.Visible = true;
                    }
                    else
                    {
                        rowBypass.Visible = false;
                    }
                }
                else
                {
                    rowBypass.Visible = false;
                }*/
                if (working.state != PurchaseRequest.RequestState.opened && working.state != PurchaseRequest.RequestState.pending && working.state != PurchaseRequest.RequestState.rejected)
                {
                    btnDelete.Visible = false;
                }
                else
                {
                    btnDelete.Visible = true;
                }
                // Chnage the submit button prompt depending on the state of the request.
                if (working.state == PurchaseRequest.RequestState.opened)
                {
                    btnSubmit.Text = "<span>Submit this Request</span>";
                }
                else
                {
                    btnSubmit.Text = "<span>Save Changes to this Request</span>";
                }

                btnOverrideAutoApprove.Visible = false;
                // This is a tough one: look through the transaction history for an auto-approval and, if the request was auto-approved, allow an admin to bounce it back for review.
                if (CurrentUser.UserPermissions.Contains(IMETPOClasses.User.Permission.admin) || CurrentUser.UserPermissions.Contains(IMETPOClasses.User.Permission.purchaser))
                {
                    if (working.state == PurchaseRequest.RequestState.approved)
                    {
                        for (int i = working.history.Count - 1; i >= 0; i--)
                        {
                            if (working.history[i].transaction == RequestTransaction.TransactionType.Approved)
                            {
                                if (working.history[i].comments.IndexOf("Auto-approved because requestor") >= 0)
                                {
                                    btnOverrideAutoApprove.Visible = true;
                                }
                                break;
                            }
                        }
                    }
                }
                // Determin if the user can approve -- either due to global approval permissions, or approver permissions on the current user's account.
                bool can_approve = false;
                if (CurrentUser.UserPermissions.Contains(IMETPOClasses.User.Permission.globalapprover))
                {
                    can_approve = true;
                }
                else
                {
                    foreach (FASNumber f in fasnumbers)
                    {
                        if (f.Number == working.fasnumberstring)
                        {
                            foreach (FASPermission p in f.Permissions)
                            {
                                if (p.permission == IMETPOClasses.User.Permission.approver && p.userid == CurrentUser.userid)
                                {
                                    can_approve = true;
                                    break;
                                }
                            }
                        }
                    }
                }
                if (can_approve && mode == "approve")
                {
                    btnApprove.Visible = true;
                    btnReject.Visible = true;
                }
                else
                {
                    btnApprove.Visible = false;
                    btnReject.Visible = false;
                }

                // Manage the visibility of the requisition controls based on the mode.
                if (mode == "submit" || mode == "edit" || mode == "approve")
                {
                    rowRequisitionNumber.Visible = false;
                }
                else if (mode == "purchase")
                {
                    rowRequisitionNumber.Visible = true;
                    txtRequisitionNumber.Text = working.requisitionnumber;
                    txtRequisitionNumber.Visible = true;
                    lblRequisitionNumber.Visible = false;
                }
                else
                {
                    rowRequisitionNumber.Visible = true;
                    lblRequisitionNumber.Text = working.requisitionnumber;
                    txtRequisitionNumber.Visible = false;
                    lblRequisitionNumber.Visible = true;
                }

                // Allow cart links or attachments if the request is in create or edit mode; otherwise, disable the link controls.
                if (mode == "submit" || mode == "edit")
                {
                    txtCartLink.Visible = true;
                    txtCartLink.Text = working.shoppingcarturl;
                }
                else
                {
                    if (!string.IsNullOrEmpty(working.shoppingcarturl))
                    {
                        cartlink.InnerHtml = "<a href='" + working.shoppingcarturl + "' target='_blank'>" + working.shoppingcarturl + "</a>";
                    }
                    txtCartLink.Visible = false;
                }

                if (mode == "submit" || mode == "edit")
                {
                    rowUpload.Visible = true;
                    rowUploadHelp.Visible = true;
                }
                else
                {
                    rowUpload.Visible = false;
                    if (working.attachments.Count == 0)
                    {
                        rowUploadHelp.Visible = false;
                    }
                    else
                    {
                        rowUploadHelp.Visible = true;
                    }
                }

                // Show the requestor fields based on the mode of the page.  For most of these fields, there is either a label or a text box.  Show the text box if it is
                // editable; otherwise, show the label.
                if (mode == "submit" || mode == "edit")
                {
                    if (!string.IsNullOrEmpty(working.description))
                    {
                        txtDescription.Text = working.description;
                    }
                    else
                    {
                        txtDescription.Text = string.Empty;
                    }
                    txtDescription.Visible = true;
                    lblDescription.Visible = false;

                    if (!string.IsNullOrEmpty(working.requestornotes))
                    {
                        txtRequestorNotes.Text = working.requestornotes;
                    }
                    else
                    {
                        txtRequestorNotes.Text = string.Empty;
                    }
                    txtRequestorNotes.Visible = true;
                    lblRequestorNotes.Visible = false;
                }
                else
                {
                    lblDescription.InnerText = working.description;
                    lblDescription.Visible = true;
                    txtDescription.Visible = false;

                    lblRequestorNotes.InnerText = working.requestornotes;
                    txtRequestorNotes.Visible = false;
                    lblRequestorNotes.Visible = true;
                }
                // If this is approval mode and the user can approve, show the executor notes; otherwise, hide it.
                if (mode == "approve" && can_approve)
                {
                    if (!string.IsNullOrEmpty(working.executornotes))
                    {
                        txtExecutorNotes.Text = working.executornotes;
                    }
                    else
                    {
                        txtExecutorNotes.Text = string.Empty;
                    }
                    txtExecutorNotes.Visible = true;
                    lblExecutorNotes.Visible = false;
                }
                else if (mode == "submit" || mode == "edit")
                {
                    rowExecutorNotes.Visible = false;
                }
                else
                {
                    lblExecutorNotes.InnerText = working.executornotes;
                    txtExecutorNotes.Visible = false;
                    lblExecutorNotes.Visible = true;
                }
                
                // Hide the purchaser fields if the request has not yet gotten to the "Approved" stage.
                if (mode == "submit" || mode == "edit" || mode == "approve")
                {                                        
                    rowPurchaserNotes.Visible = false;
                    txtPurchaserNotes.Visible = false;
                }
                else
                {
                    txtPurchaserNotes.Text = string.Empty;
                    txtPurchaserNotes.Visible = true;
                    rowPurchaserNotes.Visible = true;
                }

                // If this request is in purchase mode, and the user is a purchaser, show the purchase complete checkbox.
                if (mode == "purchase" && CurrentUser.UserPermissions.Contains(IMETPOClasses.User.Permission.purchaser))
                {
                    rowPurchaseComplete.Visible = true;
                }
                else
                {
                    rowPurchaseComplete.Visible = false;
                }

                if (mode == "submit" || mode == "edit" || mode == "approve")
                {
                    btnAddRows.Visible = true;
                }
                else
                {
                    btnAddRows.Visible = false;
                }
            }
            // Show the line items
            PopulateLineItems(conn, working, initValues);
            BuildHistory(working);
            if (working.requestid != Guid.Empty)
            {
                string html = "<a class='squarebutton' href='ViewRequest.aspx?REQUESTID=" + working.requestid.ToString() + "' target='_blank'><span>Printer-friendly version</span></a>";
                printlink.InnerHtml = html;
            }
        }

        // Populate the list of line items.
        protected void PopulateLineItems(SqlConnection conn, PurchaseRequest working, bool initValues)
        {
            int i = 0;
            // Remove all of the rows from the table that are not static rows.
            while (i < tblLineItems.Rows.Count)
            {
                if (tblLineItems.Rows[i].GetType() == typeof(TableHeaderRow) || tblLineItems.Rows[i].GetType() == typeof(TableFooterRow) || tblLineItems.Rows[i].ID == "newItemRow")
                {
                    i += 1;
                }
                else
                {
                    tblLineItems.Rows.RemoveAt(i);
                }
            }
            int num_rows = 5;
            if (working.state == PurchaseRequest.RequestState.purchased || working.state == PurchaseRequest.RequestState.deleted || working.state == PurchaseRequest.RequestState.received)
            {
                num_rows = working.lineitems.Count;
            }
            else
            {
                if (working.lineitems.Count > 5)
                {
                    num_rows = (working.lineitems.Count / 5) * 5;
                    if (working.lineitems.Count % 5 > 0)
                        num_rows += 5;
                }
            }            
            // There are some fields that only show up in certain modes.  Show or hide those based on the exec mode of the request.
            string mode = GetExecMode(working);
            if ((mode == "purchase") && CurrentUser.UserPermissions.Contains(IMETPOClasses.User.Permission.purchaser))
            {
                headerToBeInventoried.Visible = true;
            }
            else
            {
                headerToBeInventoried.Visible = false;
            }
            if (mode == "receive" || mode == "close")
            {
                headerQuantityReceived.Visible = true;
                headerReceived.Visible = true;
            }
            else
            {
                headerQuantityReceived.Visible = false;
                headerReceived.Visible = false;
            }
            // Find the place in the table where we should start inserting rows.
            int row_index = 0;
            for (i = 0; i < tblLineItems.Rows.Count; i++)
            {
                if (tblLineItems.Rows[i].GetType() == typeof(TableHeaderRow))
                {
                    row_index = i + 1;
                }
            }
            // This is how we manage the default 5-row thing on the request page.  A hidden field stores the list of GUIDs assigned to the rows, which in turn links to the line items that
            // might already be in the request.
            string rows = (string)GetSessionValue("rowids");//hiddenLineItemIDs.Value;
            List<string> ids = new List<string>();
            if (string.IsNullOrEmpty(rows))
            {
                for (i = 0; i < working.lineitems.Count; i++)
                {
                    ids.Add(working.lineitems[i].lineitemid.ToString());
                }                
            }
            else
            {
                string[] tokens = rows.Split(",".ToCharArray());
                ids.AddRange(tokens);
            }

            // If the number of rows is less than the desired number, make a whole bunch of new guids and keep adding them.
            while (ids.Count < num_rows)
            {
                Guid newid = Guid.NewGuid();
                ids.Add(newid.ToString());
            }
            bool editable = mode == "submit" || mode == "edit" || mode == "approve" || mode == "purchase";         
            // Add a row to the table for each line item (or desired blank row)
            for (i = 0; i < num_rows; i++)
            {
                LineItem li = null;
                if(i < working.lineitems.Count)
                    li = working.lineitems[i];

                // Add a control for each field in the line item (or blank, if there is not yet a line item).
                Guid rowid = new Guid(ids[i]);
                if (li != null && li.state == LineItem.LineItemState.deleted)
                    continue;                
                TableRow tr = new TableRow();
                TableCell tc = new TableCell();
                TextBox txt = new TextBox();
                txt.CssClass = "TextQuantity";
                txt.Width = Unit.Percentage(100);
                txt.ID = "txtQuantity_" + rowid.ToString();
                txt.Attributes.Add("onchange", "updateTotal();");
                if (initValues && li != null)
                    txt.Text = li.qty.ToString();
                if (editable)
                {
                    txt.Enabled = true;
                }
                else
                {
                    txt.Enabled = false;
                }
                tc.Controls.Add(txt);
                tr.Cells.Add(tc);

                tc = new TableCell();
                txt = new TextBox();
                txt.CssClass = "TextUOM";
                txt.Width = Unit.Percentage(100);
                txt.ID = "txtUOM_" + rowid.ToString();
                if (initValues && li != null)
                    txt.Text = li.unit;
                if (editable)
                {
                    txt.Enabled = true;
                }
                else
                {
                    txt.Enabled = false;
                }
                tc.Controls.Add(txt);
                tr.Cells.Add(tc);

                tc = new TableCell();
                txt = new TextBox();
                txt.CssClass = "TextLineItemNumber";
                txt.ID = "txtLineItemNumber_" + rowid.ToString();
                txt.Width = Unit.Percentage(100);
                if (initValues && li != null)
                    txt.Text = li.itemnumber;
                if (editable)
                {
                    txt.Enabled = true;
                }
                else
                {
                    txt.Enabled = false;
                }
                tc.Controls.Add(txt);
                tr.Cells.Add(tc);

                tc = new TableCell();
                txt = new TextBox();
                txt.CssClass = "TextLineItemDesc";
                txt.Width = Unit.Percentage(100);
                txt.ID = "txtLineItemDesc_" + rowid.ToString();
                if (initValues && li != null)
                    txt.Text = li.description;
                if (editable)
                {
                    txt.Enabled = true;
                }
                else
                {
                    txt.Enabled = false;
                }
                tc.ColumnSpan = 2;
                tc.Controls.Add(txt);
                tr.Cells.Add(tc);



                tc = new TableCell();
                txt = new TextBox();
                txt.CssClass = "TextUnitPrice";
                txt.ID = "txtUnitPrice_" + rowid.ToString();
                txt.Attributes.Add("onchange", "updateTotal();");
                txt.Width = Unit.Percentage(100);
                if (initValues && li != null)
                    txt.Text = li.unitprice.ToString();
                if (editable)
                {
                    txt.Enabled = true;
                }
                else
                {
                    txt.Enabled = false;
                }
                tc.Controls.Add(txt);
                tr.Cells.Add(tc);

                tc = new TableCell();
                Label lbl = new Label();
                lbl.CssClass = "LabelUnitPrice";                
                lbl.ID = "lblTotalPrice_" + rowid.ToString();
                if (initValues && li != null)
                    lbl.Text = (li.qty * li.unitprice).ToString();
                if (editable)
                {
                    txt.Enabled = true;
                }
                else
                {
                    txt.Enabled = false;
                }
                tc.Controls.Add(lbl);
                tr.Cells.Add(tc);

                // Show or hide the "receive" fields, based on mode of the page.
                if (mode == "receive" || mode == "close")
                {
                    tc = new TableCell();
                    tr.Cells.Add(tc);

                    txt = new TextBox();
                    txt.CssClass = "TextQtyReceived";
                    txt.ID = "txtQuantityReceived_" + rowid.ToString();
                    txt.Attributes.Add("onchange", "checkInt('" + txt.ID + "');");
                    txt.Width = Unit.Percentage(100);
                    if (mode == "close")
                        txt.Enabled = false;
                    if (initValues && li != null)
                    {
                        if (li.qtyreceived < 0)
                        {
                            txt.Text = li.qty.ToString();
                            li.qtyreceived = li.qty;
                        }
                        else
                        {
                            txt.Text = li.qtyreceived.ToString();
                        }
                    }
                    tc.Controls.Add(txt);
                    tr.Cells.Add(tc);

                    tc = new TableCell();
                    CheckBox ch = new CheckBox();
                    ch.ID = "chkReceived_" + rowid.ToString();
                    ch.Text = "Received";
                    if (initValues && li != null)
                        ch.Checked = li.state.HasFlag(IMETPOClasses.LineItem.LineItemState.received);
                    if (mode == "close")
                        ch.Enabled = false;
                    tc.Controls.Add(ch);
                    tr.Cells.Add(tc);
                }
                // Hide or show the purchase fields, based on the mode of the request.
                if ((mode == "purchase") && CurrentUser.UserPermissions.Contains(IMETPOClasses.User.Permission.purchaser))
                {
                    tc = new TableCell();
                    CheckBox ch = new CheckBox();
                    ch.ID = "chkInventoryIMET_" + rowid.ToString();
                    ch.Text = "IMET Inventory";
                    if (initValues && li != null)
                        ch.Checked = li.inventoryIMET;
                    if (mode == "close")
                        ch.Enabled = false;
                    tc.Controls.Add(ch);

                    System.Web.UI.HtmlControls.HtmlGenericControl gen = new System.Web.UI.HtmlControls.HtmlGenericControl("br");
                    tc.Controls.Add(gen);

                    ch = new CheckBox();
                    ch.ID = "chkInventoryMD_" + rowid.ToString();
                    ch.Text = "MD Inventory";
                    if (initValues && li != null)
                        ch.Checked = li.inventoryMD;
                    if (mode == "close")
                        ch.Enabled = false;
                    tc.Controls.Add(ch);

                    tr.Cells.Add(tc);
                }
                string link = string.Empty;
                if (mode == "submit" || mode == "edit")
                {
                    link += "<a class=\"squarebutton\" href=\"javascript:deleteLineItem('" + rowid.ToString() + "')\"><span>Delete</span></a>";
                }
                tc = new TableCell();
                tc.Text = link;
                tr.Cells.Add(tc);
                tblLineItems.Rows.AddAt(row_index, tr);
                row_index += 1;
            }
            rows = ids[0];
            for (i = 1; i < ids.Count; i++)
                rows = rows + "," + ids[i];
            hiddenLineItemIDs.Value = rows;
            SetSessionValue("rowids", rows);
            // Populate the miscellaneous charges.
            if (string.IsNullOrEmpty(txtMiscCharges.Text))
                txtMiscCharges.Text = string.Format("{0:0.00}", working.misccharge);
            if (string.IsNullOrEmpty(txtShippingCharges.Text))
                txtShippingCharges.Text = string.Format("{0:0.00}", working.shipcharge);
            if (string.IsNullOrEmpty(txtTaxCharges.Text))
                txtTaxCharges.Text = string.Format("{0:0.00}", working.taxcharge);
            working.misccharge = float.Parse(txtMiscCharges.Text);
            working.shipcharge = float.Parse(txtShippingCharges.Text);
            working.taxcharge = float.Parse(txtTaxCharges.Text);
            lblTotalPrice.Text = string.Format("{0:C}", working.TotalPrice);
            
        }

        // Add some blank rows to the line items table.
        protected void btnAddRows_Click(object sender, EventArgs e)
        {            
            SqlConnection conn = ConnectToConfigString("imetpsconnection");
            PurchaseRequest working = (PurchaseRequest)GetSessionValue("WorkingPurchaseRequest");
            string mode = GetExecMode(working);
            string rows = (string)GetSessionValue("rowids");// hiddenLineItemIDs.Value;
            string[] tokens = rows.Split(",".ToCharArray());
            int num_rows = tokens.Length + 5;
            int i = 0;
            int row_index = 0;
            for (i = 0; i < tblLineItems.Rows.Count; i++)
            {
                if (tblLineItems.Rows[i].GetType() == typeof(TableHeaderRow))
                {
                    row_index = i + 1;
                }
                if (tblLineItems.Rows[i].GetType() == typeof(TableFooterRow))
                {
                    row_index = i;
                }
            }
            // This is a re-creation of a lot of the logic above, which is inefficient, but eh.  There's no check for existing line items, as all new rows are going to be blank.
            for (i = 0; i < num_rows; i++)
            {
                Guid rowid = Guid.Empty;
                rowid = Guid.NewGuid();
                if (string.IsNullOrEmpty(rows))
                    rows = rowid.ToString();
                else
                    rows = rows + "," + rowid.ToString();
                TableRow tr = new TableRow();
                TableCell tc = new TableCell();
                TextBox txt = new TextBox();
                txt.CssClass = "TextQuantity";
                txt.Width = Unit.Percentage(100);
                txt.ID = "txtQuantity_" + rowid.ToString();
                txt.Attributes.Add("onchange", "updateTotal();");
                if (mode == "submit" || mode == "edit" || mode == "approve")
                {
                    txt.Enabled = true;
                }
                else
                {
                    txt.Enabled = false;
                }
                tc.Controls.Add(txt);
                tr.Cells.Add(tc);

                tc = new TableCell();
                txt = new TextBox();
                txt.CssClass = "TextUOM";
                txt.Width = Unit.Percentage(100);
                txt.ID = "txtUOM_" + rowid.ToString();
                if (mode == "submit" || mode == "edit" || mode == "approve")
                {
                    txt.Enabled = true;
                }
                else
                {
                    txt.Enabled = false;
                }
                tc.Controls.Add(txt);
                tr.Cells.Add(tc);

                tc = new TableCell();
                txt = new TextBox();
                txt.CssClass = "TextLineItemNumber";
                txt.ID = "txtLineItemNumber_" + rowid.ToString();
                txt.Width = Unit.Percentage(100);
                if (mode == "submit" || mode == "edit" || mode == "approve")
                {
                    txt.Enabled = true;
                }
                else
                {
                    txt.Enabled = false;
                }
                tc.Controls.Add(txt);
                tr.Cells.Add(tc);

                tc = new TableCell();
                txt = new TextBox();
                txt.CssClass = "TextLineItemDesc";
                txt.Width = Unit.Percentage(100);
                txt.ID = "txtLineItemDesc_" + rowid.ToString();
                if (mode == "submit" || mode == "edit" || mode == "approve")
                {
                    txt.Enabled = true;
                }
                else
                {
                    txt.Enabled = false;
                }
                tc.ColumnSpan = 2;
                tc.Controls.Add(txt);
                tr.Cells.Add(tc);



                tc = new TableCell();
                txt = new TextBox();
                txt.CssClass = "TextUnitPrice";
                txt.ID = "txtUnitPrice_" + rowid.ToString();
                txt.Attributes.Add("onchange", "updateTotal();");
                txt.Width = Unit.Percentage(100);
                if (mode == "submit" || mode == "edit" || mode == "approve")
                {
                    txt.Enabled = true;
                }
                else
                {
                    txt.Enabled = false;
                }
                tc.Controls.Add(txt);
                tr.Cells.Add(tc);

                tc = new TableCell();
                Label lbl = new Label();
                lbl.CssClass = "LabelUnitPrice";
                lbl.ID = "lblTotalPrice_" + rowid.ToString();
                if (mode == "submit" || mode == "edit" || mode == "approve")
                {
                    txt.Enabled = true;
                }
                else
                {
                    txt.Enabled = false;
                }
                tc.Controls.Add(lbl);
                tr.Cells.Add(tc);

                if (mode == "receive")
                {
                    tc = new TableCell();
                    tr.Cells.Add(tc);

                    txt = new TextBox();
                    txt.CssClass = "TextQtyReceived";
                    txt.ID = "txtQuantityReceived_" + rowid.ToString();
                    txt.Attributes.Add("onchange", "checkInt('" + txt.ID + "');");
                    txt.Width = Unit.Percentage(100);
                    if (mode == "close")
                        txt.Enabled = false;
                    tc.Controls.Add(txt);
                    tr.Cells.Add(tc);

                    tc = new TableCell();
                    CheckBox ch = new CheckBox();
                    ch.ID = "chkReceived_" + rowid.ToString();
                    ch.Text = "Received";
                    if (mode == "close")
                        ch.Enabled = false;
                    tc.Controls.Add(ch);
                    tr.Cells.Add(tc);
                }
                if (mode == "purchase")
                {
                    tc = new TableCell();
                    CheckBox ch = new CheckBox();
                    ch.ID = "chkInventoryIMET_" + rowid.ToString();
                    ch.Text = "IMET Inventory";
                    if (mode == "close")
                        ch.Enabled = false;
                    tc.Controls.Add(ch);

                    System.Web.UI.HtmlControls.HtmlGenericControl gen = new System.Web.UI.HtmlControls.HtmlGenericControl("br");
                    tc.Controls.Add(gen);

                    ch = new CheckBox();
                    ch.ID = "chkInventoryMD_" + rowid.ToString();
                    ch.Text = "MD Inventory";
                    if (mode == "close")
                        ch.Enabled = false;
                    tc.Controls.Add(ch);

                    tr.Cells.Add(tc);
                }
                string link = string.Empty;
                if (mode == "submit" || mode == "edit")
                {
                    link += "<a class=\"squarebutton\" href=\"javascript:deleteLineItem('" + rowid.ToString() + "')\"><span>Delete</span></a>";
                }
                tc = new TableCell();
                tc.Text = link;
                tr.Cells.Add(tc);
                tblLineItems.Rows.AddAt(row_index, tr);
                row_index += 1;
            }
            hiddenLineItemIDs.Value = rows;
            SetSessionValue("rowids", rows);
            conn.Close();
        }

        // The user clicked the submit button.  We have to check a bunch of fields, and then copy the data to the underlying object.
        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            SqlConnection conn = ConnectToConfigString("imetpsconnection");
            PurchaseRequest working = (PurchaseRequest)GetSessionValue("WorkingPurchaseRequest");
            string mode = GetExecMode(working);
            bool purchaser_notes_updated = false;
            // Create a flag indicating that the purchaser notes have been updated.
            if (!txtPurchaserNotes.ReadOnly && txtPurchaserNotes.Visible)
            {
                if (!string.IsNullOrEmpty(txtPurchaserNotes.Text))
                {
                    purchaser_notes_updated = true;
                }
            }            
            try
            {
                List<LineItem> new_received_items = new List<LineItem>();
                // Read the inputs.
                if (!ReadInputs(working, new_received_items))
                {
                    return;
                }
                if (working.state == PurchaseRequest.RequestState.opened)
                {
                    if (comboVendors.SelectedValue == "NEW")
                    {
                        if (string.IsNullOrEmpty(txtVendorName.Text))
                        {
                            ShowAlert("You must enter a vendor name for a new vendor.");
                            return;
                        }
                    }
                    if (string.IsNullOrEmpty(comboFASNumber.SelectedValue))
                    {
                        ShowAlert("You must select an account number to continue.");
                        return;
                    }
                    if (string.IsNullOrEmpty(txtDescription.Text))
                    {
                        ShowAlert("You must enter a justification for this request.");
                        return;
                    }
                }
                if (mode == "purchase")
                {
                    if (chkPurchaseComplete.Checked)
                    {
                        if (string.IsNullOrEmpty(txtRequisitionNumber.Text))
                        {
                            ShowAlert("You must enter a requisition number for this request to be flagged as purchased.");
                            return;
                        }
                    }
                }
                if (comboVendors.SelectedValue != "NEW")
                {
                    // Load the current vendor, if the vendor already exists.
                    Guid vid = new Guid(comboVendors.SelectedValue);
                    Vendor v = new Vendor();
                    v.Load(conn, vid);
                    working.vendorid = v;
                }
                else
                {
                    // Otherwise, create a new vendor and notify the administrators.
                    Vendor v = new Vendor();
                    v.vendorname = txtVendorName.Text;
                    v.url = txtVendorURL.Text;
                    v.description = txtVendorDescription.Text;
                    v.Save(conn);
                    List<IMETPOClasses.User> users = IMETPOClasses.User.LoadUsersWithPermission(conn, IMETPOClasses.User.Permission.admin);
                    if (users.Count > 0)
                    {
                        List<string> to = new List<string>();
                        List<string> bcc = new List<string>();
                        bcc.Add("smcginnis@umces.edu");
                        foreach (IMETPOClasses.User u in users)
                        {
                            if (!string.IsNullOrEmpty(u.email) && !u.UserPermissions.Contains(IMETPOClasses.User.Permission.noemail))
                                to.Add(u.email);
                        }
                        if (to.Count > 0)
                        {
                            string subject = "[" + GetApplicationSetting("applicationTitle") + "]: New Vendor Created";
                            string body = "<p>A new vendor has been created with the following information:</p><ul>";
                            body += "<li>Name: " + v.vendorname + "</li>";
                            body += "<li>URL: " + v.url + "</li>";
                            body += "<li>Description: " + v.description + "</li>";
                            body += "</ul>";
                            body += "<p><a href='http://" + GetApplicationSetting("hostAddress") + "/ModifyVendor.aspx?VENDORID=" + v.vendorid.ToString() + "'>Click here to view this vendor.</a></p>";
                            body += "Thank you,<br/>" + GetApplicationSetting("applicationTitle") + " System";
                            SendEmail(to.ToArray<string>(), null, bcc.ToArray<string>(), subject, body);
                        }
                    }
                    working.vendorid = v;
                }
                // Load the FAS number selected.
                if (!string.IsNullOrEmpty(comboFASNumber.SelectedValue))
                {
                    working.fasnumber = new FASNumber();
                    working.fasnumber.Load(conn, comboFASNumber.SelectedValue);
                }
                if (!string.IsNullOrEmpty(comboalt_FASnumber.SelectedValue))
                {
                    working.alt_fasnumber = new FASNumber();
                    working.alt_fasnumber.Load(conn, comboalt_FASnumber.SelectedValue);
                }
                if (string.IsNullOrEmpty(working.tagnumber))
                {
                    working.tagnumber = PurchaseRequest.GenerateTagNumber(conn, CurrentUser.username);
                }                

                bool needs_acknowledgement = false;
                PurchaseRequest.RequestState old_state = working.state;
                // If the state is new, there are a whole bunch of things to do.
                if (working.state == PurchaseRequest.RequestState.opened)
                {
                    needs_acknowledgement = true;
                    working.state = PurchaseRequest.RequestState.pending;
                    working.SetLineItemState(LineItem.LineItemState.pending);

                    // Create a "new request" transaction.
                    Guid requestorid = new Guid(comboRequester.SelectedValue);
                    if (requestorid != CurrentUser.userid)
                    {
                        User requestor = new IMETPOClasses.User();
                        requestor.Load(conn, requestorid);
                        working.history.Add(new RequestTransaction(RequestTransaction.TransactionType.Opened, requestor.userid, comboRequester.SelectedItem.Text, working.description, requestor.FullName));
                    }
                    else
                    {
                        working.history.Add(new RequestTransaction(RequestTransaction.TransactionType.Opened, CurrentUser.userid, comboRequester.SelectedItem.Text, working.description, CurrentUser.FullName));
                    }
                    working.history.Add(new RequestTransaction(RequestTransaction.TransactionType.Created, CurrentUser.userid, CurrentUser.username, working.description, CurrentUser.FullName));                    
                    bool can_approve = false;
                    string str_bypass_limit = Utils.GetSystemSetting(conn, "accountbypasslimit");
                    double bypass_limit = double.NaN;
                    if (!string.IsNullOrEmpty(str_bypass_limit))
                    {
                        bypass_limit = double.Parse(str_bypass_limit);
                    }
                    if (working.fasnumber != null)
                    {
                        foreach (FASPermission p in working.fasnumber.Permissions)
                        {
                            if (p.userid == CurrentUser.userid)
                            {
                                if (p.permission == IMETPOClasses.User.Permission.approver || (p.permission == IMETPOClasses.User.Permission.accountbypasser && !double.IsNaN(bypass_limit) && working.TotalPrice <= bypass_limit))
                                {
                                    can_approve = true;
                                    break;
                                }
                            }
                        }
                    }
                    // If the user can approve for this account, auto-approve it.
                    if (can_approve)
                    {
                        working.state = PurchaseRequest.RequestState.approved;
                        working.SetLineItemState(LineItem.LineItemState.approved);
                        working.history.Add(new RequestTransaction(RequestTransaction.TransactionType.Approved, CurrentUser.userid, CurrentUser.username, "Auto-approved because requestor has approval permissions: " + working.executornotes, CurrentUser.FullName));
                    }
                }
                else if (mode == "purchase")
                {
                    working.requisitionnumber = txtRequisitionNumber.Text;
                    if (chkPurchaseComplete.Checked)
                    {
                        working.state = PurchaseRequest.RequestState.purchased;
                        working.SetLineItemState(LineItem.LineItemState.purchased);
                        working.history.Add(new RequestTransaction(RequestTransaction.TransactionType.Purchased, CurrentUser.userid, CurrentUser.username, working.purchasernotes, CurrentUser.FullName));
                    }
                }                
                else if (mode == "receive")
                {
                    bool is_received = true;
                    foreach (LineItem li in working.lineitems)
                    {
                        if (!li.state.HasFlag(IMETPOClasses.LineItem.LineItemState.received))
                        {
                            is_received = false;
                        }
                    }
                    if (is_received)
                    {
                        working.state = PurchaseRequest.RequestState.received;
                        working.history.Add(new RequestTransaction(RequestTransaction.TransactionType.Received, CurrentUser.userid, CurrentUser.username, string.Empty, CurrentUser.FullName));                        
                    }
                }
                else
                {
                    working.history.Add(new RequestTransaction(RequestTransaction.TransactionType.Modified, CurrentUser.userid, CurrentUser.username, "Request revised.", CurrentUser.FullName));
                }
                if (purchaser_notes_updated)
                {
                    working.history.Add(new RequestTransaction(RequestTransaction.TransactionType.Modified, CurrentUser.userid, CurrentUser.username, working.purchasernotes, CurrentUser.FullName));
                }
                // Save the working request.
                working.Save(conn);
                // Repopulate the page with the saved request information.
                PopulateData(conn, working, true);

                // If it is a new request, send an email to all approvers if the requestor is not an approver.
                if (old_state == PurchaseRequest.RequestState.opened)
                {
                    bool can_approve = false;
                    if (working.fasnumber != null)
                    {
                        foreach (FASPermission p in working.fasnumber.Permissions)
                        {
                            if (p.permission == IMETPOClasses.User.Permission.approver && p.userid == CurrentUser.userid)
                            {
                                can_approve = true;
                                break;
                            }
                        }
                    }
                    if (!can_approve)
                    {
                        List<string> to = new List<string>();
                        foreach (FASPermission f in working.fasnumber.Permissions)
                        {
                            if (f.permission == IMETPOClasses.User.Permission.approver)
                            {
                                IMETPOClasses.User u = new User();
                                u.Load(conn, f.userid);
                                if (!string.IsNullOrEmpty(u.email) && !to.Contains(u.email) && !u.UserPermissions.Contains(IMETPOClasses.User.Permission.noemail))
                                {
                                    to.Add(u.email);
                                }
                            }
                        }
                        List<string> bcc = new List<string>();
                        bcc.Add("smcginnis@umces.edu");
                        // to.Add("smcginnis@hpl.umces.edu");
                        if (to.Count > 0)
                        {
                            string subject = "[" + GetApplicationSetting("applicationTitle") + "]: Purchase made by " + CurrentUser.username + " against " + working.fasnumber.Number;
                            string body = "<p>A Purchase Request has been made against account number " + working.fasnumber.Number + " (" + working.fasnumber.Description + ") ";
                            body += "by " + CurrentUser.username + " using " + GetApplicationSetting("applicationSubtitle") + ". You are being notified because the ";
                            body += "system indicates you are an executor of this FAS. This request will not be ";
                            body += "sent on for purchase until you approve it. You can also choose to reject the request, ";
                            body += "causing it to go back to the requestor for modification. Following is a summary of ";
                            body += "the request:</p>";
                            body += "<ul><li>IPS Number: " + working.tagnumber + "</li>";
                            body += "<li>Description: " + working.description + "</li>";
                            body += "<li>Requestor Notes: " + working.requestornotes + "</li>";
                            body += "<li>Action needed: APPROVAL/REJECTION</li></ul>";
                            body += "<p><a href='http://" + GetApplicationSetting("hostAddress") + "/SubmitRequest.aspx?REQUESTID=" + working.requestid.ToString() + "'>Click here to view this request.</a></p>";
                            body += "Thank you,<br/>" + GetApplicationSetting("applicationTitle") + " System";
                            SendEmail(to.ToArray<string>(), null, bcc.ToArray<string>(), subject, body);
                        }
                    }
                }
                // If the request has been purchased, or the purchase notes have been updated, send a note to the requestor and the approver.
                if (mode == "purchase" || purchaser_notes_updated)
                {
                    if (chkPurchaseComplete.Checked || purchaser_notes_updated)
                    {
                        Guid requestorid = working.GetTransactionUser(RequestTransaction.TransactionType.Opened);
                        Guid ownerid = working.fasnumber.OwnerID;
                        Guid approverid = working.GetTransactionUser(RequestTransaction.TransactionType.Approved);
                        if (requestorid != Guid.Empty)
                        {
                            User u = new User();
                            u.Load(conn, requestorid);

                            List<string> to = new List<string>();
                            List<string> bcc = new List<string>();
                            bcc.Add("smcginnis@umces.edu");
                            if (!u.UserPermissions.Contains(IMETPOClasses.User.Permission.noemail))
                                to.Add(u.email);
                            if (ownerid != requestorid)
                            {
                                u = new User();
                                u.Load(conn, ownerid);
                                if (!u.UserPermissions.Contains(IMETPOClasses.User.Permission.noemail))
                                    to.Add(u.email);
                            }
                            if (ownerid != approverid && requestorid != approverid && approverid != Guid.Empty)
                            {
                                u = new User();
                                u.Load(conn, approverid);
                                if (!u.UserPermissions.Contains(IMETPOClasses.User.Permission.noemail))
                                    to.Add(u.email);
                            }
                            // to.Add("smcginnis@hpl.umces.edu");
                            if (to.Count > 0)
                            {
                                string subject = string.Empty;
                                string body = string.Empty;
                                if (purchaser_notes_updated)
                                {
                                    subject = "[" + GetApplicationSetting("applicationTitle") + "]: Purchase Request Updates";
                                    body = "<p>The purchaser has added additional notes to this request:</p><ul>";
                                }
                                else
                                {
                                    subject = "[" + GetApplicationSetting("applicationTitle") + "]: Purchase Request Purchased";
                                    body = "<p>Your purchase request has been purchased:</p><ul>";
                                }                                
                                body += "<li>Account Number: " + working.fasnumber.Number + "</li>";
                                body += "<li>Description: " + working.description + "</li>";
                                body += "<li>Purchased by: " + CurrentUser.username + "</li>";
                                body += "<li>Purchaser notes: " + working.purchasernotes + "</li>";
                                body += "</ul>";
                                body += "<p><a href='http://" + GetApplicationSetting("hostAddress") + "/ViewRequest.aspx?REQUESTID=" + working.requestid.ToString() + "'>Click here to view this request.</a></p>";
                                body += "Thank you,<br/>" + GetApplicationSetting("applicationTitle") + " System";
                                SendEmail(to.ToArray<string>(), null, bcc.ToArray<string>(), subject, body);
                            }
                        }
                    }
                    ShowAlert("Information saved. Email has been sent to the requestor and approver.");
                }
                // If the request has been received, and the receiver is not the requester, send out a message.
                else if (mode == "receive")
                {
                    bool is_received = true;
                    foreach (LineItem li in working.lineitems)
                    {
                        if (!li.state.HasFlag(LineItem.LineItemState.received))
                        {
                            is_received = false;
                            break;
                        }
                    }
                    if (is_received)
                    {
                        Guid requestorid = working.GetTransactionUser(RequestTransaction.TransactionType.Opened);
                        Guid approverid = working.GetTransactionUser(RequestTransaction.TransactionType.Approved);
                        Guid ownerid = working.fasnumber.OwnerID;

                        if (requestorid != Guid.Empty)
                        {
                            User u = new User();
                            u.Load(conn, requestorid);

                            List<string> to = new List<string>();
                            List<string> bcc = new List<string>();
                            bcc.Add("smcginnis@umces.edu");
                            if (!u.UserPermissions.Contains(IMETPOClasses.User.Permission.noemail))
                                to.Add(u.email);
                            if (ownerid != requestorid)
                            {
                                u = new User();
                                u.Load(conn, ownerid);
                                if (!u.UserPermissions.Contains(IMETPOClasses.User.Permission.noemail))
                                    to.Add(u.email);
                            }
                            if (approverid != requestorid)
                            {
                                u = new User();
                                u.Load(conn, approverid);
                                if (!u.UserPermissions.Contains(IMETPOClasses.User.Permission.noemail))
                                    to.Add(u.email);
                            }
                            // to.Add("smcginnis@hpl.umces.edu");
                            if (to.Count > 0)
                            {
                                string subject = "[" + GetApplicationSetting("applicationTitle") + "]: Purchase Request Received";
                                string body = "<p>Your purchase request has been received:</p><ul>";
                                body += "<li>Account Number: " + working.fasnumber.Number + "</li>";
                                body += "<li>Description: " + working.description + "</li>";
                                body += "<li>Received by: " + CurrentUser.username + "</li>";
                                body += "</ul>";
                                body += "<p><a href='http://" + GetApplicationSetting("hostAddress") + "/ViewRequest.aspx?REQUESTID=" + working.requestid.ToString() + "'>Click here to view this request.</a></p>";
                                body += "Thank you,<br/>" + GetApplicationSetting("applicationTitle") + " System";
                                SendEmail(to.ToArray<string>(), null, bcc.ToArray<string>(), subject, body);
                            }
                        }
                    }
                    ShowAlert("Changes saved.");
                }
                else
                {
                    ShowAlert("Changes saved.");
                }
                // Go through newly received items, and if the items need to be inventoried, send an email to all inventoriers.
                if (new_received_items.Count > 0)
                {
                    int i = 0;
                    while (i < new_received_items.Count)
                    {
                        if (!new_received_items[i].inventoryIMET && !new_received_items[i].inventoryMD)
                        {
                            new_received_items.RemoveAt(i);
                        }
                        else
                        {
                            i += 1;
                        }
                    }
                    if (new_received_items.Count > 0)
                    {
                        List<User> inventoriers = IMETPOClasses.User.LoadUsersWithPermission(conn, IMETPOClasses.User.Permission.inventory);
                        if (inventoriers.Count > 0)
                        {
                            List<string> to = new List<string>();
                            foreach (User u in inventoriers)
                            {
                                if (!string.IsNullOrEmpty(u.email) && !to.Contains(u.email) && !u.UserPermissions.Contains(IMETPOClasses.User.Permission.noemail))
                                {
                                    to.Add(u.email);
                                }
                            }
                            List<string> bcc = new List<string>();
                            bcc.Add("smcginnis@umces.edu");
                            // to.Add("smcginnis@hpl.umces.edu");
                            if (to.Count > 0)
                            {
                                string subject = "[" + GetApplicationSetting("applicationTitle") + "]: Request " + working.tagnumber + " line item(s) awaiting inventory";
                                string body = "<p>One or more line items has been received that is flagged for inventory. Following is a summary ";
                                body += "of the request:</p>";
                                body += "<ul><li>IPS Number: " + working.tagnumber + "</li>";
                                body += "<li>Description: " + working.description + "</li>";
                                foreach (LineItem li in new_received_items)
                                {
                                    body += "<li>Item: (" + li.itemnumber + ") " + li.description;
                                    if (li.inventoryIMET)
                                    {
                                        body += " (IMET)";
                                    }
                                    if (li.inventoryMD)
                                    {
                                        body += " (MD)";
                                    }
                                    body += "</li>";
                                }
                                body += "<li>Action Required: INVENTORY</li></ul>";                                
                                body += "<p><a href='http://" + GetApplicationSetting("hostAddress") + "/SubmitRequest.aspx?REQUESTID=" + working.requestid.ToString() + "'>Click here to view this request.</a></p>";
                                body += "Thank you,<br/>" + GetApplicationSetting("applicationTitle") + " System";
                                SendEmail(to.ToArray<string>(), null, bcc.ToArray<string>(), subject, body);
                            }
                        }
                    }
                }
                // If the request needs an acknowledgement, bounce to the acknowledge version of the View page.
                if (needs_acknowledgement)
                {
                    string ack_url = "ViewRequest.aspx?REQUESTID=" + working.requestid.ToString() + "&ACK=1";
                    Response.Redirect(ack_url, false);
                }
                SetSessionValue("WorkingPurchaseRequest", working);
            }
            catch (Exception ex)
            {
                ShowAlert("An error occurred while saving your request.  It may not be saved.\n  Message: " + ex.Message);
                SendErrorNotification(ex.Message, ex.StackTrace);
                throw ex;
            }
            finally
            {
                conn.Close();
            }
        }

        /// <summary>
        /// Copy the values in the page's controls to the request.
        /// </summary>
        /// <param name="working">The working version of the request</param>
        /// <param name="new_received_items">A list (output) that needs to be populated with any newly-received items.</param>
        /// <returns></returns>
        protected bool ReadInputs(PurchaseRequest working, List<LineItem> new_received_items)
        {
            // Copy the header-level text fields.
            if (!txtDescription.ReadOnly && txtDescription.Visible)
                working.description = txtDescription.Text;
            if (!txtRequestorNotes.ReadOnly && txtRequestorNotes.Visible)
                working.requestornotes = txtRequestorNotes.Text;
            // Copy the miscellaneous charge fields.
            if (!txtMiscCharges.ReadOnly)
            {
                if (!string.IsNullOrEmpty(txtMiscCharges.Text))
                {
                    try
                    {
                        working.misccharge = float.Parse(txtMiscCharges.Text);
                    }
                    catch (FormatException)
                    {
                        ShowAlert("The miscellaneous charge is not a valid number.");
                        return false;
                    }
                }
                else
                    working.misccharge = 0;
            }
            if (!txtShippingCharges.ReadOnly)
            {
                if (!string.IsNullOrEmpty(txtShippingCharges.Text))
                {
                    try
                    {
                        working.shipcharge = float.Parse(txtShippingCharges.Text);
                    }
                    catch (FormatException)
                    {
                        ShowAlert("The shipping charge is not a valid number.");
                        return false;
                    }
                }
                else
                    working.shipcharge = 0;
            }
            if (!txtTaxCharges.ReadOnly)
            {
                if (!string.IsNullOrEmpty(txtTaxCharges.Text))
                {
                    try
                    {
                        working.taxcharge = float.Parse(txtTaxCharges.Text);
                    }
                    catch (FormatException)
                    {
                        ShowAlert("The tax charge is not a valid number.");
                        return false;
                    }
                }
                else
                    working.taxcharge = 0;
            }
            // Copy all of the notes values.
            if (!txtExecutorNotes.ReadOnly && txtExecutorNotes.Visible)
                working.executornotes = txtExecutorNotes.Text;
            if (!txtPurchaserNotes.ReadOnly && txtPurchaserNotes.Visible)
                working.purchasernotes = txtPurchaserNotes.Text;
            if (!txtRequisitionNumber.ReadOnly && txtRequisitionNumber.Visible)
                working.requisitionnumber = txtRequisitionNumber.Text;
            if (!txtCartLink.ReadOnly && txtCartLink.Visible)
                working.shoppingcarturl = txtCartLink.Text;
            string rowids = (string)GetSessionValue("rowids");
            string[] ids = rowids.Split(",".ToCharArray());
            List<LineItem> new_items = new List<LineItem>();
            // Iterate through the line items and copy them based on the IDS in the rowids hidden controls.
            for (int i = 0; i < ids.Length; i++)
            {
                if (string.IsNullOrEmpty(ids[i]))
                    continue;
                Guid rowid = new Guid(ids[i]);                
                TextBox uom_txt = (TextBox)Page.FindControl("txtUOM_" + rowid.ToString());
                TextBox desc_txt = (TextBox)Page.FindControl("txtLineItemDesc_" + rowid.ToString());
                TextBox num_txt = (TextBox)Page.FindControl("txtLineItemNumber_" + rowid.ToString());
                TextBox price_txt = (TextBox)Page.FindControl("txtUnitPrice_" + rowid.ToString());
                TextBox qty_txt = (TextBox)Page.FindControl("txtQuantity_" + rowid.ToString());
                if (uom_txt == null || desc_txt == null || num_txt == null || price_txt == null || qty_txt == null)
                    continue;
                bool is_filled = !string.IsNullOrEmpty(uom_txt.Text) || !string.IsNullOrEmpty(desc_txt.Text) || !string.IsNullOrEmpty(num_txt.Text) || !string.IsNullOrEmpty(price_txt.Text) || !string.IsNullOrEmpty(qty_txt.Text);
                bool is_complete = !string.IsNullOrEmpty(uom_txt.Text) && !string.IsNullOrEmpty(desc_txt.Text) && !string.IsNullOrEmpty(num_txt.Text) && !string.IsNullOrEmpty(price_txt.Text) && !string.IsNullOrEmpty(qty_txt.Text);
                bool read_only = !uom_txt.Enabled && !desc_txt.Enabled && !num_txt.Enabled && !price_txt.Enabled && !qty_txt.Enabled;

                if (!is_filled && !read_only)
                    continue;
                if (is_filled && !is_complete && !read_only)
                {
                    ShowAlert("There is at least one partially-filled line item.  Please delete all unused line items, or complete the line item in question.");
                    return false;
                }
                LineItem li = null;
                foreach (LineItem li2 in working.lineitems)
                {
                    if (li2.lineitemid == rowid)
                    {
                        li = li2;
                        break;
                    }
                }
                if (li == null)
                    li = new LineItem();
                if (!read_only)
                {
                    try
                    {
                        li.qty = int.Parse(qty_txt.Text);
                    }
                    catch (Exception)
                    {
                        ShowAlert("You must enter a valid quantity for that line item: " + desc_txt.Text);
                        return false;
                    }
                    li.unit = uom_txt.Text;
                    li.description = desc_txt.Text;
                    li.itemnumber = num_txt.Text;
                    try
                    {
                        li.unitprice = float.Parse(price_txt.Text);
                    }
                    catch (Exception)
                    {
                        ShowAlert("You must enter a valid price for that line item: " + desc_txt.Text);
                        return false;
                    }
                }
                TextBox qty_rcd = (TextBox)Page.FindControl("txtQuantityReceived_" + rowid.ToString());
                if (qty_rcd != null)
                {
                    try
                    {
                        li.qtyreceived = int.Parse(qty_rcd.Text);
                    }
                    catch (FormatException)
                    {
                    }
                }
                CheckBox chk = (CheckBox)Page.FindControl("chkReceived_" + rowid.ToString());
                if (chk != null)
                {
                    if (chk.Checked)
                    {
                        if (new_received_items != null)
                        {
                            if (!li.state.HasFlag(LineItem.LineItemState.received) && !new_received_items.Contains(li))
                            {
                                new_received_items.Add(li);
                            }
                        }
                        li.state |= LineItem.LineItemState.received;
                    }
                }
                chk = (CheckBox)Page.FindControl("chkInventoryIMET_" + rowid.ToString());
                if (chk != null)
                {
                    if (chk.Checked)
                    {
                        li.inventoryIMET = true;
                    }
                    else
                    {
                        li.inventoryIMET = false;
                    }
                }
                chk = (CheckBox)Page.FindControl("chkInventoryMD_" + rowid.ToString());
                if (chk != null)
                {
                    if (chk.Checked)
                    {
                        li.inventoryMD = true;
                    }
                    else
                    {
                        li.inventoryMD = false;
                    }
                }
                new_items.Add(li);
            }
            working.lineitems.Clear();
            working.lineitems.AddRange(new_items);            
            return true;
        }

        // The user clicked the "approve" button.
        protected void btnApprove_Click(object sender, EventArgs e)
        {
            PurchaseRequest working = (PurchaseRequest)GetSessionValue("WorkingPurchaseRequest");
            SqlConnection conn = ConnectToConfigString("imetpsconnection");
            try
            {
                // Copy any changes to the request.
                if (!ReadInputs(working, null))
                    return;
                // Put the relevant approval transactions in the history.
                working.state = PurchaseRequest.RequestState.approved;
                working.SetLineItemState(LineItem.LineItemState.approved);
                working.history.Add(new RequestTransaction(RequestTransaction.TransactionType.Approved, CurrentUser.userid, CurrentUser.username, working.executornotes, CurrentUser.FullName));
                working.Save(conn);                
                Guid requestorid = working.GetTransactionUser(RequestTransaction.TransactionType.Opened);
                Guid ownerid = working.fasnumber.OwnerID;
                // Send an email to the requestor if the requestor is not the approver.
                if (requestorid != Guid.Empty && requestorid != CurrentUser.userid)
                {
                    User u = new User();
                    u.Load(conn, requestorid);
                    List<string> to = new List<string>();
                    List<string> bcc = new List<string>();
                    bcc.Add("smcginnis@umces.edu");
                    if (!u.UserPermissions.Contains(IMETPOClasses.User.Permission.noemail))
                        to.Add(u.email);
                    if (ownerid != requestorid)
                    {
                        u = new User();
                        u.Load(conn, ownerid);
                        if (!u.UserPermissions.Contains(IMETPOClasses.User.Permission.noemail))
                            to.Add(u.email);
                    }
                    // to.Add("smcginnis@hpl.umces.edu");
                    if (to.Count > 0)
                    {
                        string subject = "[" + GetApplicationSetting("applicationTitle") + "]: Request " + working.tagnumber + " accepted by " + CurrentUser.username;
                        string body = "<p>A request on your Account Number " + working.fasnumber.Number + " (" + working.fasnumber.Description + ") was accepted by " + CurrentUser.username + ", who has the ability ";
                        body += "to approve requests on your behalf. No action is required on your part. The request will be ";
                        body += "forwarded for purchase. Following is a summary of the request:</p>";
                        body += "<ul><li>IPS Number: " + working.tagnumber + "</li>";
                        body += "<li>Requestor: <a href='mailto:" + CurrentUser.email + "'>" + CurrentUser.username + "</a></li>";
                        body += "<li>Requestor Notes: " + working.requestornotes + "</li>";
                        body += "<li>Description: " + working.description + "</li>";
                        body += "<li>Executor Notes: " + working.executornotes + "</li>";
                        body += "<li>Action Required: NONE</li></ul>";
                        body += "<p><a href='http://" + GetApplicationSetting("hostAddress") + "/ViewRequest.aspx?REQUESTID=" + working.requestid.ToString() + "'>Click here to view this request.</a></p>";
                        body += "Thank you,<br/>" + GetApplicationSetting("applicationTitle") + " System";
                        SendEmail(to.ToArray<string>(), null, bcc.ToArray<string>(), subject, body);
                    }
                }
                // Send an email to the purchasers in the system telling them that the request is ready to purchase.
                List<User> purchasers = IMETPOClasses.User.LoadUsersWithPermission(conn, IMETPOClasses.User.Permission.purchaser);
                if (purchasers.Count > 0)
                {
                    List<string> to = new List<string>();
                    foreach (User u in purchasers)
                    {
                        if (!string.IsNullOrEmpty(u.email) && !to.Contains(u.email) && !u.UserPermissions.Contains(IMETPOClasses.User.Permission.noemail))
                        {
                            to.Add(u.email);
                        }
                    }
                    List<string> bcc = new List<string>();
                    bcc.Add("smcginnis@umces.edu");
                    // to.Add("smcginnis@hpl.umces.edu");
                    if (to.Count > 0)
                    {
                        string subject = "[" + GetApplicationSetting("applicationTitle") + "]: Request " + working.tagnumber + " awaiting purchase";
                        string body = "<p>A Purchase Request has been approved and is awaiting purchase. Following is a summary ";
                        body += "of the request:</p>";
                        body += "<ul><li>IPS Number: " + working.tagnumber + "</li>";
                        body += "<li>Description: " + working.description + "</li>";
                        body += "<li>Requestor Notes: " + working.requestornotes + "</li>";
                        body += "<li>Executor Notes: " + working.executornotes + "</li>";
                        body += "<li>Action Required: PURCHASE</li></ul>";
                        body += "<p><a href='http://" + GetApplicationSetting("hostAddress") + "/SubmitRequest.aspx?REQUESTID=" + working.requestid.ToString() + "'>Click here to view this request.</a></p>";
                        body += "Thank you,<br/>" + GetApplicationSetting("applicationTitle") + " System";
                        SendEmail(to.ToArray<string>(), null, bcc.ToArray<string>(), subject, body);
                    }
                }
                PopulateData(conn, working, true);
                ShowAlertWithRedirect("Purchase approved.  E-mail sent to IMET Admin Office.", "Default.aspx");
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
            finally
            {
                if (conn != null)
                    conn.Close();
            }
        }

        // The user hit the "reject" button.
        protected void btnReject_Click(object sender, EventArgs e)
        {
            PurchaseRequest working = (PurchaseRequest)GetSessionValue("WorkingPurchaseRequest");
            SqlConnection conn = ConnectToConfigString("imetpsconnection");
            try
            {
                if (!ReadInputs(working, null))
                    return;
                // Put a reject transaction in the history.
                working.state = PurchaseRequest.RequestState.rejected;
                working.SetLineItemState(LineItem.LineItemState.rejected);
                working.history.Add(new RequestTransaction(RequestTransaction.TransactionType.Rejected, CurrentUser.userid, CurrentUser.username, working.executornotes, CurrentUser.FullName));
                working.Save(conn);
                ShowAlert("Request rejected.");
                Guid requestorid = working.GetTransactionUser(RequestTransaction.TransactionType.Opened);
                Guid ownerid = working.fasnumber.OwnerID;
                // Send an email to the requestor, telling them that the request was rejected.
                if (requestorid != Guid.Empty && requestorid != CurrentUser.userid)
                {
                    User u = new User();
                    u.Load(conn, requestorid);
                    List<string> to = new List<string>();
                    List<string> bcc = new List<string>();
                    bcc.Add("smcginnis@umces.edu");
                    if (!u.UserPermissions.Contains(IMETPOClasses.User.Permission.noemail))
                        to.Add(u.email);
                    if (ownerid != requestorid)
                    {
                        u = new User();
                        u.Load(conn, ownerid);
                        if (!u.UserPermissions.Contains(IMETPOClasses.User.Permission.noemail))
                            to.Add(u.email);
                    }
                    // to.Add("smcginnis@hpl.umces.edu");
                    if (to.Count > 0)
                    {
                        string subject = "[" + GetApplicationSetting("applicationTitle") + "]: Request " + working.tagnumber + " rejected by " + CurrentUser.username;
                        string body = "<p>Your request, IPS Number " + working.tagnumber + ", was rejected by " + CurrentUser.username + ". You must now modify ";
                        body += "this request according to the executor or remove it from the system. Following is a summary of the request:</p>";
                        body += "<ul><li>IPS Number: " + working.tagnumber + "</li>";
                        body += "<li>Description: " + working.description + "</li>";
                        body += "<li>Executor Notes: " + working.executornotes + "</li>";
                        body += "<li>Action Required: MODIFY</li></ul>";
                        body += "<p><a href='http://" + GetApplicationSetting("hostAddress") + "/ViewRequest.aspx?REQUESTID=" + working.requestid.ToString() + "'>Click here to view this request.</a></p>";
                        body += "Thank you,<br/>" + GetApplicationSetting("applicationTitle") + " System";
                        SendEmail(to.ToArray<string>(), null, bcc.ToArray<string>(), subject, body);
                    }
                }
                PopulateData(conn, working, true);
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
            finally
            {
                if (conn != null)
                    conn.Close();
            }
        }

        // The user hit the "Override Auto-Approve" button.
        protected void btnOverrideAutoApprove_Click(object sender, EventArgs e)
        {
            PurchaseRequest working = (PurchaseRequest)GetSessionValue("WorkingPurchaseRequest");
            SqlConnection conn = ConnectToConfigString("imetpsconnection");
            try
            {
                // Update the request.
                if (!ReadInputs(working, null))
                    return;
                // Add the request to the history.
                working.state = PurchaseRequest.RequestState.pending;
                working.SetLineItemState(LineItem.LineItemState.pending);
                working.history.Add(new RequestTransaction(RequestTransaction.TransactionType.Modified, CurrentUser.userid, CurrentUser.username, "Auto-approved request returned for approval.", CurrentUser.FullName));
                working.Save(conn);
                List<string> to = new List<string>();
                foreach (FASPermission f in working.fasnumber.Permissions)
                {
                    if (f.permission == IMETPOClasses.User.Permission.approver)
                    {
                        IMETPOClasses.User u = new User();
                        u.Load(conn, f.userid);
                        if (!string.IsNullOrEmpty(u.email) && !to.Contains(u.email) && !u.UserPermissions.Contains(IMETPOClasses.User.Permission.noemail))
                        {
                            to.Add(u.email);
                        }
                    }
                }
                List<string> bcc = new List<string>();
                bcc.Add("smcginnis@umces.edu");
                Guid requestorid = working.GetTransactionUser(RequestTransaction.TransactionType.Opened);
                User requestor = new User();
                // Send an email to the requestor and any approvers for this account.
                requestor.Load(conn, requestorid);
                // to.Add("smcginnis@hpl.umces.edu");
                if (to.Count > 0)
                {
                    string subject = "[" + GetApplicationSetting("applicationTitle") + "]: Purchase made by " + requestor.username + " against " + working.fasnumber.Number;
                    string body = "<p>A Purchase Request has been made against account number " + working.fasnumber.Number + " (" + working.fasnumber.Description + ") ";
                    body += "by " + requestor.username + " using " + GetApplicationSetting("applicationSubtitle") + ". You are being notified because the ";
                    body += "system indicates you are an executor of this FAS. This request will not be ";
                    body += "sent on for purchase until you approve it. You can also choose to reject the request, ";
                    body += "causing it to go back to the requestor for modification. Following is a summary of ";
                    body += "the request:</p>";
                    body += "<ul><li>IPS Number: " + working.tagnumber + "</li>";
                    body += "<li>Description: " + working.description + "</li>";
                    body += "<li>Requestor Notes: " + working.requestornotes + "</li>";
                    body += "<li>Action needed: APPROVAL/REJECTION</li></ul>";
                    body += "<p><a href='http://" + GetApplicationSetting("hostAddress") + "/SubmitRequest.aspx?REQUESTID=" + working.requestid.ToString() + "'>Click here to view this request.</a></p>";
                    body += "Thank you,<br/>" + GetApplicationSetting("applicationTitle") + " System";
                    SendEmail(to.ToArray<string>(), null, bcc.ToArray<string>(), subject, body);
                }
                ShowAlert("Request returned.");                
                PopulateData(conn, working, true);
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
            finally
            {
                if (conn != null)
                    conn.Close();
            }
        }
       
        // The user hit the delete button.
        protected void btnDelete_Click(object sender, EventArgs e)
        {
            PurchaseRequest working = (PurchaseRequest)GetSessionValue("WorkingPurchaseRequest");
            SqlConnection conn = ConnectToConfigString("imetpsconnection");
            try
            {
                if (working != null)
                {
                    // Set the request to "deleted" and email the requestor.
                    working.state = PurchaseRequest.RequestState.deleted;
                    working.SetLineItemState(LineItem.LineItemState.deleted);
                    working.history.Add(new RequestTransaction(RequestTransaction.TransactionType.Deleted, CurrentUser.userid, CurrentUser.username, string.Empty, CurrentUser.FullName));
                    ShowAlert("Request deleted.");
                    working.Save(conn);
                    Guid requestorid = working.GetTransactionUser(RequestTransaction.TransactionType.Opened);
                    Guid ownerid = working.fasnumber.OwnerID;
                    if (requestorid != Guid.Empty)
                    {
                        User u = new User();
                        u.Load(conn, requestorid);
                        List<string> to = new List<string>();
                        List<string> bcc = new List<string>();
                        bcc.Add("smcginnis@umces.edu");
                        if (!u.UserPermissions.Contains(IMETPOClasses.User.Permission.noemail))
                            to.Add(u.email);
                        if (ownerid != requestorid)
                        {
                            u = new User();
                            u.Load(conn, ownerid);
                            if (!u.UserPermissions.Contains(IMETPOClasses.User.Permission.noemail))
                                to.Add(u.email);
                        }
                        if (to.Count > 0)
                        {
                            string subject = "[" + GetApplicationSetting("applicationTitle") + "]: Request " + working.tagnumber + " deleted";
                            string body = "<p>Your request, IPS Number " + working.tagnumber + ", has been marked as deleted. Following is a ";
                            body += "summary of the request:</p>";
                            body += "<ul><li>IPS Number: " + working.tagnumber + "</li>";
                            body += "<li>Description: " + working.description + "</li>";
                            body += "<li>Purchaser Notes: " + working.purchasernotes + "</li>";
                            body += "<li>Action Required: NONE</li></ul>";
                            body += "<p><a href='http://" + GetApplicationSetting("hostAddress") + "/ViewRequest.aspx?REQUESTID=" + working.requestid.ToString() + "'>Click here to view this request.</a></p>";
                            body += "Thank you,<br/>" + GetApplicationSetting("applicationTitle") + " System";
                            SendEmail(to.ToArray<string>(), null, bcc.ToArray<string>(), subject, body);
                        }
                    }
                }
                working = new PurchaseRequest();
                SetSessionValue("WorkingPurchaseRequest", working);
                PopulateData(conn, working, true);
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

        // Load the details of the vendor when the selected vendor changes.
        protected void comboVendors_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboVendors.SelectedValue == "NEW")
            {
                txtVendorName.Enabled = true;
                txtVendorURL.Enabled = true;
                txtVendorURL.Text = string.Empty;
                txtVendorName.Text = string.Empty;
                txtVendorAddress1.Text = string.Empty;
                txtVendorAddress2.Text = string.Empty;
                txtVendorCity.Text = string.Empty;
                txtVendorDescription.Text = string.Empty;
                txtVendorFax.Text = string.Empty;
                txtVendorFEIN.Text = string.Empty;
                txtVendorPhone.Text = string.Empty;
                txtVendorPostalCode.Text = string.Empty;
                txtVendorState.Text = string.Empty;
                txtVendor_customer_account_number.Text = string.Empty;
                txtVendorcontact_name.Text = string.Empty;
                txtVendorcontact_email.Text = string.Empty;
                txtVendorcontact_phone.Text = string.Empty;
                return;
            }
            else
            {
                PurchaseRequest working = (PurchaseRequest)GetSessionValue("WorkingPurchaseRequest");
                SqlConnection conn = ConnectToConfigString("imetpsconnection");
                try
                {
                    txtVendorName.Enabled = false;
                    txtVendorURL.Enabled = false;
                    working.vendorid = new Vendor();
                    working.vendorid.Load(conn, new Guid(comboVendors.SelectedValue));
                    txtVendorName.Text = working.vendorid.vendorname;
                    txtVendorURL.Text = working.vendorid.url;
                    txtVendorAddress1.Text = working.vendorid.address1;
                    txtVendorAddress2.Text = working.vendorid.address2;
                    txtVendorCity.Text = working.vendorid.city;
                    txtVendorDescription.Text = working.vendorid.description;
                    txtVendorFax.Text = working.vendorid.fax;
                    txtVendorFEIN.Text = working.vendorid.fein;
                    txtVendorPhone.Text = working.vendorid.phone;
                    txtVendorPostalCode.Text = working.vendorid.phone;
                    txtVendorState.Text = working.vendorid.st;
                    txtVendor_customer_account_number.Text = working.vendorid.customer_account_number;
                    txtVendorcontact_name.Text = working.vendorid.contact_name;
                    txtVendorcontact_email.Text = working.vendorid.contact_email;
                    txtVendorcontact_phone.Text = working.vendorid.contact_phone;
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

        // Process the "upload" button, which adds an attachment to the request.
        protected void btnUpload_Click(object sender, EventArgs e)
        {
            SqlConnection conn = ConnectToConfigString("imetpsconnection");
            PurchaseRequest working = (PurchaseRequest)GetSessionValue("WorkingPurchaseRequest");
            try
            {
                // Create a new attached file.
                AttachedFile af = new AttachedFile();
                af.ID = Guid.NewGuid();
                string filepath = GetApplicationSetting("filesavepath") + af.ID.ToString();
                af.Filename = uploadAttachment.FileName;
                af.Path = filepath;
                // Save the attachment.
                uploadAttachment.SaveAs(filepath);
                af.Save(conn, Guid.Empty);

                // working.attachments.Clear();
                working.attachments.Add(af);
                // PopulateData(conn, working, false);
                SetSessionValue("WorkingPurchaseRequest", working);
                PopulateAttachments(working);
                // PopulateData(conn, working, true);
            }
            catch (Exception ex)
            {
                ShowAlert("Could not upload file: " + ex.Message + "\n" + ex.StackTrace);
                HandleError(ex);
            }
            finally
            {
                conn.Close();
            }
        }

        /// <summary>
        /// Populate the list of attachments for this current request
        /// </summary>
        /// <param name="working">The current request.</param>
        protected void PopulateAttachments(PurchaseRequest working)
        {
            if (working.attachments.Count > 0)
            {
                string linkurl = "<table border='0'>";
                foreach (AttachedFile f in working.attachments)
                {
                    string download_link = "<a href='DownloadAttachment.aspx?ATTACHMENTID=" + f.ID.ToString() + "' target='_blank'>Download " + f.Filename + "</a>";
                    string remove_link = "<a href='javascript:removeAttachment(\"" + f.ID.ToString() + "\")'>Remove " + f.Filename + "</a>";
                    // It's just a table; add a download link in a table cell for each row.
                    linkurl += "<tr><td>" + download_link + "</td><td>" + remove_link + "</td></tr>";
                }
                linkurl += "</table>";
                filedownloadlink.InnerHtml = linkurl;
            }
            else
            {
                filedownloadlink.InnerHtml = string.Empty;
            }
        }

        // Remove an attachment.
        protected void RemoveAttachment(Guid attachmentid)
        {
            SqlConnection conn = ConnectToConfigString("imetpsconnection");
            try
            {
                PurchaseRequest working = (PurchaseRequest)GetSessionValue("WorkingPurchaseRequest");
                for (int i = 0; i < working.attachments.Count; i++)
                {
                    AttachedFile af = working.attachments[i];
                    if (af.ID == attachmentid)
                    {
                        af.DeleteLocalCopy();
                        working.attachments.RemoveAt(i);
                        break;
                    }
                }
                PopulateAttachments(working);
                if (working.state != PurchaseRequest.RequestState.opened)
                {
                    working.Save(conn);
                }
            }
            catch (Exception ex)
            {
                ShowAlert("An error occurred: " + ex.Message + " " + ex.StackTrace);
                HandleError(ex);
            }
            finally
            {
                if (conn != null)
                    conn.Close();
            }
        }
    }
}
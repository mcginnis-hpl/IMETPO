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
    public partial class SubmitRequest : imetspage
    {
        protected override void OnInit(EventArgs e)
        {
            try {
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
            try {
            PopulateHeader(appTitle, appSubtitle);
            if (!IsPostBack)
            {
                if (!IsAuthenticated)
                {
                    string url = "Default.aspx?RETURNURL=" + Request.Url.ToString();
                    Response.Redirect(url);
                    return;
                }
                Guid requestid = Guid.Empty;
                for (int i = 0; i < Request.Params.Count; i++)
                {
                    if (Request.Params.GetKey(i).ToUpper() == "REQUESTID")
                    {
                        requestid = new Guid(Request.Params[i]);
                    }
                }
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
                    if (tokens[0] == "EDIT")
                    {
                        EditLineItem(new Guid(tokens[1]));
                    }
                    else if (tokens[0] == "DELETE")
                    {
                        DeleteLineItem(new Guid(tokens[1]));
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

        protected void EditLineItem(Guid lineitemid)
        {
            SqlConnection conn = ConnectToConfigString("imetpsconnection");
            PurchaseRequest working = (PurchaseRequest)GetSessionValue("WorkingPurchaseRequest");
            try
            {
                foreach (LineItem li in working.lineitems)
                {
                    if (li.lineitemid == lineitemid)
                    {
                        TextBox txt = (TextBox)Page.FindControl("txtQuantity_" + lineitemid.ToString());
                        int new_quantity = -1;
                        float new_price = -1;
                        if (!txt.ReadOnly)
                        {                            
                            try
                            {
                                new_quantity = int.Parse(txt.Text);
                            }
                            catch (Exception)
                            {
                                ShowAlert("You must enter a valid quantity for that line item.");
                                break;
                            }
                        }
                        txt = (TextBox)Page.FindControl("txtUOM_" + lineitemid.ToString());
                        if (!txt.ReadOnly)
                        {
                            li.unit = txt.Text;
                        }

                        txt = (TextBox)Page.FindControl("txtLineItemDesc_" + lineitemid.ToString());
                        if (!txt.ReadOnly)
                        {
                            li.description = txt.Text;
                        }

                        txt = (TextBox)Page.FindControl("txtUnitPrice_" + lineitemid.ToString());
                        if (!txt.ReadOnly)
                        {                            
                            try
                            {
                                new_price = float.Parse(txt.Text);
                            }
                            catch (Exception)
                            {
                                ShowAlert("You must enter a valid unit price for that line item.");
                                break;
                            }
                        }
                        if(new_quantity >= 0)
                            li.qty = new_quantity;
                        if(new_price >= 0)
                            li.unitprice = new_price;

                        txt = (TextBox)Page.FindControl("txtQuantityReceived_" + lineitemid.ToString());
                        new_quantity = -1;
                        if (!txt.ReadOnly)
                        {
                            try
                            {
                                new_quantity = int.Parse(txt.Text);
                            }
                            catch (Exception)
                            {
                                break;
                            }
                            li.qtyreceived = new_quantity;
                        }

                        CheckBox ch = (CheckBox)Page.FindControl("chkInventoryIMET_" + lineitemid.ToString());
                        if (ch != null)
                        {
                            if (ch.Enabled)
                            {
                                li.inventoryIMET = ch.Checked;
                            }
                        }
                        ch = (CheckBox)Page.FindControl("chkInventoryMD_" + lineitemid.ToString());
                        if (ch != null)
                        {
                            if (ch.Enabled)
                            {
                                li.inventoryMD = ch.Checked;
                            }
                        }
                        break;
                    }
                }
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

        protected void DeleteLineItem(Guid lineitemid)
        {
            SqlConnection conn = ConnectToConfigString("imetpsconnection");
            PurchaseRequest working = (PurchaseRequest)GetSessionValue("WorkingPurchaseRequest");
            try
            {
                for (int i = 0; i < working.lineitems.Count; i++)
                {
                    if (working.lineitems[i].lineitemid == lineitemid)
                    {
                        working.lineitems.RemoveAt(i);
                        break;
                    }
                }
                // PopulateData(conn, working, true);
                PopulateLineItems(conn, working, true);
                SetSessionValue("WorkingPurchaseRequest", working);
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
            if (initValues)
            {
                comboFASNumber.Items.Clear();
                List<FASNumber> fasnumbers = null;
                if (mode == "submit" || mode == "edit" || working.fasnumber == null)
                {
                    comboFASNumber.Items.Add(new ListItem(string.Empty, string.Empty));
                    fasnumbers = CurrentUser.LoadFASNumbers(conn, IMETPOClasses.User.Permission.requestor);
                    foreach (FASNumber f in fasnumbers)
                    {
                        ListItem li = new ListItem();
                        li.Text = f.Number + " (" + f.Description + ")";
                        li.Value = f.Number;
                        comboFASNumber.Items.Add(li);
                        if (li.Value == working.fasnumberstring)
                        {
                            comboFASNumber.SelectedIndex = comboFASNumber.Items.Count - 1;
                        }
                    }
                    comboFASNumber.Enabled = true;
                }
                else
                {
                    fasnumbers = new List<FASNumber>();
                    fasnumbers.Add(working.fasnumber);
                    ListItem li = new ListItem();
                    li.Text = working.fasnumber.Number + " (" + working.fasnumber.Description + ")";
                    li.Value = working.fasnumber.Number;
                    comboFASNumber.Items.Add(li);
                    comboFASNumber.Enabled = false;
                }
                Guid requestorid = working.GetTransactionUser(RequestTransaction.TransactionType.Opened);

                comboRequester.Items.Clear();
                comboRequester.Items.Add(new ListItem(CurrentUser.username, CurrentUser.userid.ToString()));
                User tmpuser = CurrentUser.parentuser;
                while (tmpuser != null)
                {
                    comboRequester.Items.Add(new ListItem(tmpuser.username, tmpuser.userid.ToString()));
                    if (tmpuser.userid == requestorid)
                    {
                        comboRequester.SelectedIndex = comboRequester.Items.Count - 1;
                        break;
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
                    comboVendors.Enabled = false;
                }
                else
                {
                    comboVendors.Enabled = true;
                }
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
                }
                else
                {
                    vendorDetails.Visible = true;
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
                }
                if (!string.IsNullOrEmpty(working.tagnumber))
                {
                    lblTagNumber.Text = working.tagnumber;
                }
                else
                {
                    lblTagNumber.Text = string.Empty;
                }
                lblStatus.Text = PurchaseRequest.GetRequestStateString(working.state);

                Guid curr_requestor = new Guid(comboRequester.SelectedValue);
                if (curr_requestor != CurrentUser.userid)
                {
                    rowBypass.Visible = true;
                }
                else
                {
                    rowBypass.Visible = false;
                }

                if (working.state != PurchaseRequest.RequestState.opened && working.state != PurchaseRequest.RequestState.pending && working.state != PurchaseRequest.RequestState.rejected)
                {
                    btnDelete.Visible = false;
                }
                else
                {
                    btnDelete.Visible = true;
                }
                if (working.state == PurchaseRequest.RequestState.opened)
                {
                    btnSubmit.Text = "Submit this Request";
                }
                else
                {
                    btnSubmit.Text = "Save Changes to this Request";
                }

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

                if (mode == "close")
                {
                    btnClose.Visible = true;
                }
                else
                {
                    btnClose.Visible = false;
                }
                if (mode == "submit" || mode == "edit" || mode == "approve")
                {
                    rowRequisitionNumber.Visible = false;
                }
                else if (mode == "purchase")
                {
                    rowRequisitionNumber.Visible = true;
                    txtRequisitionNumber.Text = working.requisitionnumber;
                    lblRequisitionNumber.Visible = false;
                }
                else
                {
                    rowRequisitionNumber.Visible = true;
                    lblRequisitionNumber.Text = working.requisitionnumber;
                    txtRequisitionNumber.Visible = false;
                }
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
                    lblDescription.Text = working.description;
                    lblDescription.Visible = true;
                    txtDescription.Visible = false;

                    lblRequestorNotes.Text = working.requestornotes;
                    txtRequestorNotes.Visible = false;
                    lblRequestorNotes.Visible = true;
                }
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
                    lblExecutorNotes.Text = working.executornotes;
                    txtExecutorNotes.Visible = false;
                    lblExecutorNotes.Visible = true;
                }

                if (mode == "purchase")
                {
                    if (!string.IsNullOrEmpty(working.purchasernotes))
                    {
                        txtPurchaserNotes.Text = working.purchasernotes;
                    }
                    else
                    {
                        txtPurchaserNotes.Text = string.Empty;
                    }
                    txtPurchaserNotes.Visible = true;
                    lblPurchaserNotes.Visible = false;
                }
                else if (mode == "submit" || mode == "edit" || mode == "approve")
                {
                    rowPurchaserNotes.Visible = false;
                    rowPurchaseComplete.Visible = false;
                }
                else
                {
                    lblPurchaserNotes.Text = working.purchasernotes;
                    txtPurchaserNotes.Visible = false;
                    lblPurchaserNotes.Visible = true;
                }

                if (mode == "purchase")
                {
                    rowPurchaseComplete.Visible = true;
                }
                else
                {
                    rowPurchaseComplete.Visible = false;
                }

                if (mode == "receive")
                {
                    rowReceiveComplete.Visible = true;
                }
                else
                {
                    rowReceiveComplete.Visible = false;
                }


            }
            PopulateLineItems(conn, working, initValues);
            BuildHistory(working);
            if (working.requestid != Guid.Empty)
            {
                string html = "<a href='ViewRequest.aspx?REQUESTID=" + working.requestid.ToString() + "' target='_blank'>Printer-friendly version</a>";
                printlink.InnerHtml = html;
            }
        }

        protected void PopulateLineItems(SqlConnection conn, PurchaseRequest working, bool initValues)
        {
            int i = 0;
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
            if (initValues)
            {
                txtLineItemDesc_new.Text = string.Empty;
                txtQuantity_new.Text = string.Empty;
                txtUnitPrice_new.Text = string.Empty;
                txtUOM_new.Text = string.Empty;
                txtLineItemNumber_new.Text = string.Empty;
            }
            string mode = GetExecMode(working);
            if (mode == "submit" || mode == "edit")
            {
                newItemRow.Visible = true;
            }
            else
            {
                newItemRow.Visible = false;
            }
            if (mode == "receive" || mode == "close")
            {
                headerQuantityReceived.Visible = true;
                headerToBeInventoried.Visible = true;
            }
            else
            {
                headerQuantityReceived.Visible = false;
                headerToBeInventoried.Visible = false;
            }
            int row_index = 0;
            for (i = 0; i < tblLineItems.Rows.Count; i++)
            {
                if (tblLineItems.Rows[i].GetType() == typeof(TableHeaderRow))
                {
                    row_index = i + 1;
                }
            }
            foreach (LineItem li in working.lineitems)
            {
                TableRow tr = new TableRow();
                TableCell tc = new TableCell();
                TextBox txt = new TextBox();
                txt.CssClass = "TextQuantity";
                txt.ID = "txtQuantity_" + li.lineitemid.ToString();
                if (initValues)
                    txt.Text = li.qty.ToString();
                if (mode == "submit" || mode == "edit")
                {
                    txt.ReadOnly = false;
                }
                else
                {
                    txt.ReadOnly = true;
                }
                tc.Controls.Add(txt);
                tr.Cells.Add(tc);

                tc = new TableCell();
                txt = new TextBox();
                txt.CssClass = "TextUOM";
                txt.ID = "txtUOM_" + li.lineitemid.ToString();
                if (initValues)
                    txt.Text = li.unit;
                if (mode == "submit" || mode == "edit")
                {
                    txt.ReadOnly = false;
                }
                else
                {
                    txt.ReadOnly = true;
                }
                tc.Controls.Add(txt);
                tr.Cells.Add(tc);

                tc = new TableCell();
                txt = new TextBox();
                txt.CssClass = "TextLineItemDesc";
                txt.ID = "txtLineItemDesc_" + li.lineitemid.ToString();
                if (initValues)
                    txt.Text = li.description;
                if (mode == "submit" || mode == "edit")
                {
                    txt.ReadOnly = false;
                }
                else
                {
                    txt.ReadOnly = true;
                }
                tc.Controls.Add(txt);
                tr.Cells.Add(tc);

                tc = new TableCell();
                txt = new TextBox();
                txt.CssClass = "TextLineItemNumber";
                txt.ID = "txtLineItemNumber_" + li.lineitemid.ToString();
                if (initValues)
                    txt.Text = li.itemnumber;
                if (mode == "submit" || mode == "edit")
                {
                    txt.ReadOnly = false;
                }
                else
                {
                    txt.ReadOnly = true;
                }
                tc.Controls.Add(txt);
                tr.Cells.Add(tc);

                tc = new TableCell();
                txt = new TextBox();
                txt.CssClass = "TextUnitPrice";
                txt.ID = "txtUnitPrice_" + li.lineitemid.ToString();
                if (initValues)
                    txt.Text = li.unitprice.ToString();
                if (mode == "submit" || mode == "edit")
                {
                    txt.ReadOnly = false;
                }
                else
                {
                    txt.ReadOnly = true;
                }
                tc.Controls.Add(txt);
                tr.Cells.Add(tc);

                tc = new TableCell();
                Label lbl = new Label();
                lbl.CssClass = "LabelUnitPrice";
                lbl.ID = "lblTotalPrice_" + li.lineitemid.ToString();
                if (initValues)
                    lbl.Text = (li.qty * li.unitprice).ToString();
                if (mode == "submit" || mode == "edit")
                {
                    txt.ReadOnly = false;
                }
                else
                {
                    txt.ReadOnly = true;
                }
                tc.Controls.Add(lbl);
                tr.Cells.Add(tc);

                if (mode == "receive" || mode == "close")
                {
                    tc = new TableCell();
                    tr.Cells.Add(tc);

                    txt = new TextBox();
                    txt.CssClass = "TextQtyReceived";
                    txt.ID = "txtQuantityReceived_" + li.lineitemid.ToString();
                    if (mode == "close")
                        txt.ReadOnly = true;
                    if (initValues)
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

                    tc = new TableCell();
                    CheckBox ch = new CheckBox();
                    ch.ID = "chkInventoryIMET_" + li.lineitemid.ToString();
                    ch.Text = "IMET Inventory";
                    if (initValues)
                        ch.Checked = li.inventoryIMET;
                    if (mode == "close")
                        ch.Enabled = false;
                    tc.Controls.Add(ch);

                    ch = new CheckBox();
                    ch.ID = "chkInventoryMD_" + li.lineitemid.ToString();
                    ch.Text = "MD Inventory";
                    if (initValues)
                        ch.Checked = li.inventoryMD;
                    if (mode == "close")
                        ch.Enabled = false;
                    tc.Controls.Add(ch);

                    tr.Cells.Add(tc);
                }
                else
                {
                    tc = new TableCell();
                    tc.Visible = false;
                    tr.Cells.Add(tc);
                    tc = new TableCell();
                    tc.Visible = false;
                    tr.Cells.Add(tc);
                }
                string link = string.Empty;
                if (mode == "submit" || mode == "edit")
                {
                    link += "<a href=\"javascript:submitLineItem('" + li.lineitemid.ToString() + "')\">Update</a>&nbsp;&nbsp;&nbsp;<a href=\"javascript:deleteLineItem('" + li.lineitemid.ToString() + "')\">Delete</a>";
                }
                else if (mode == "receive")
                {
                    link += "<a href=\"javascript:submitLineItem('" + li.lineitemid.ToString() + "')\">Update</a>";
                }
                tc = new TableCell();
                tc.Text = link;
                tr.Cells.Add(tc);
                tblLineItems.Rows.AddAt(row_index, tr);
                row_index += 1;
            }
            if (string.IsNullOrEmpty(txtMiscCharges.Text))
                txtMiscCharges.Text = string.Format("{0:0.00}", working.misccharge);
            if (string.IsNullOrEmpty(txtShippingCharges.Text))
                txtShippingCharges.Text = string.Format("{0:0.00}", working.misccharge);
            if (string.IsNullOrEmpty(txtTaxCharges.Text))
                txtTaxCharges.Text = string.Format("{0:0.00}", working.taxcharge);
            working.misccharge = float.Parse(txtMiscCharges.Text);
            working.shipcharge = float.Parse(txtShippingCharges.Text);
            working.taxcharge = float.Parse(txtTaxCharges.Text);
            lblTotalPrice.Text = string.Format("{0:C}", working.TotalPrice);
            hiddenLineItemTotal.Value = working.LineItemTotal.ToString();
        }

        protected void btnAddNewLineItem_Click(object sender, EventArgs e)
        {
            SqlConnection conn = ConnectToConfigString("imetpsconnection");
            PurchaseRequest working = (PurchaseRequest)GetSessionValue("WorkingPurchaseRequest");
            try
            {
                LineItem li = new LineItem();
                try
                {
                    li.qty = int.Parse(txtQuantity_new.Text);
                }
                catch (Exception)
                {
                    ShowAlert("You must enter a valid quantity for that line item.");
                    return;
                }
                li.unit = txtUOM_new.Text;
                li.description = txtLineItemDesc_new.Text;
                li.itemnumber = txtLineItemNumber_new.Text;
                try
                {
                    li.unitprice = float.Parse(txtUnitPrice_new.Text);
                }
                catch (Exception)
                {
                    ShowAlert("You must enter a valid unit price for that line item.");
                    return;
                }
                li.lineitemid = Guid.NewGuid();
                working.lineitems.Add(li);
                PopulateLineItems(conn, working, true);
                // PopulateData(conn, working, false);
                SetSessionValue("WorkingPurchaseRequest", working);
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

        protected void btnSubmit_Click(object sender, EventArgs e)
        {            
            SqlConnection conn = ConnectToConfigString("imetpsconnection");
            PurchaseRequest working = (PurchaseRequest)GetSessionValue("WorkingPurchaseRequest");
            string mode = GetExecMode(working);
            ReadInputs(working);
            try
            {
                if (working.state == PurchaseRequest.RequestState.opened)
                {
                    if (comboVendors.SelectedValue == "NEW")
                    {
                        if (string.IsNullOrEmpty(txtVendorName.Text))
                        {
                            ShowAlert("You must enter a vendor name for a new vendor.");
                            return;
                        }
                        if (string.IsNullOrEmpty(txtVendorURL.Text))
                        {
                            ShowAlert("You must enter a vendor url for a new vendor.");
                            return;
                        }
                    }
                    if (string.IsNullOrEmpty(comboFASNumber.SelectedValue))
                    {
                        ShowAlert("You must select a FAS number to continue.");
                        return;
                    }
                    if (string.IsNullOrEmpty(txtDescription.Text))
                    {
                        ShowAlert("You must enter a justification for this request.");
                        return;
                    }
                }
                if (comboVendors.SelectedValue != "NEW")
                {
                    Guid vid = new Guid(comboVendors.SelectedValue);
                    Vendor v = new Vendor();
                    v.Load(conn, vid);
                    working.vendorid = v;
                }
                else
                {
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
                            if (!string.IsNullOrEmpty(u.email))
                                to.Add(u.email);
                        }
                        if (to.Count > 0)
                        {
                            string subject = "IMETPS: New Vendor Created";
                            string body = "<p>A new vendor has been created with the following information:</p><ul>";
                            body += "<li>Name: " + v.vendorname + "</li>";
                            body += "<li>URL: " + v.url + "</li>";
                            body += "<li>Description: " + v.description + "</li>";
                            body += "</ul>";
                            body += "<p><a href='http://10.1.17.29/ModifyVendor.aspx?VENDORID=" + v.vendorid.ToString() + "'>Click here to view this vendor.</a></p>";
                            body += "Thank you,<br/>IMETPS System";
                            SendEmail(to.ToArray<string>(), null, bcc.ToArray<string>(), subject, body);
                        }
                    }
                    working.vendorid = v;
                }
                if (!string.IsNullOrEmpty(comboFASNumber.SelectedValue))
                {
                    working.fasnumber = new FASNumber();
                    working.fasnumber.Load(conn, comboFASNumber.SelectedValue);
                }
                if (string.IsNullOrEmpty(working.tagnumber))
                {
                    working.tagnumber = PurchaseRequest.GenerateTagNumber(conn, CurrentUser.username);
                }

                if (working.state == PurchaseRequest.RequestState.opened)
                {
                    working.state = PurchaseRequest.RequestState.pending;
                    Guid requestorid = new Guid(comboRequester.SelectedValue);
                    working.history.Add(new RequestTransaction(RequestTransaction.TransactionType.Created, CurrentUser.userid, CurrentUser.username, working.description));
                    working.history.Add(new RequestTransaction(RequestTransaction.TransactionType.Opened, requestorid, comboRequester.SelectedItem.Text, working.description));
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
                    if (can_approve)
                    {
                        working.state = PurchaseRequest.RequestState.approved;
                        working.history.Add(new RequestTransaction(RequestTransaction.TransactionType.Approved, CurrentUser.userid, CurrentUser.username, "Auto-approved because requestor has approval permissions."));
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
                                if (!string.IsNullOrEmpty(u.email) && !to.Contains(u.email))
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
                            string subject = "[IMETPS] Purchase made by " + CurrentUser.username + " against " + working.fasnumber.Number;
                            string body = "<p>A Purchase Request has been made against FAS number " + working.fasnumber.Number + " (" + working.fasnumber.Description + ") ";
                            body += "by " + CurrentUser.username + " using the IMET Purchasing System. You are being notified because the ";
                            body += "system indicates you are an executor of this FAS. This request will not be ";
                            body += "sent on for purchase until you approve it. You can also choose to reject the request, ";
                            body += "causing it to go back to the requestor for modification. Following is a summary of ";
                            body += "the request:</p>";
                            body += "<ul><li>IMETPS Request Tag Number: $tagnumber</li>";
                            body += "<li>Description: $r_desc</li>";
                            body += "<li>Requestor Notes: $r_notes</li>";
                            body += "<li>Action needed: APPROVAL/REJECTION</li></ul>";
                            body += "<p><a href='http://10.1.17.29/SubmitRequest.aspx?REQUESTID=" + working.requestid.ToString() + "'>Click here to view this request.</a></p>";
                            body += "Thank you,<br/>IMETPS System";
                            SendEmail(to.ToArray<string>(), null, bcc.ToArray<string>(), subject, body);
                        }
                    }
                }
                else if (mode == "purchase")
                {
                    if (chkPurchaseComplete.Checked)
                    {
                        working.state = PurchaseRequest.RequestState.purchased;
                        working.history.Add(new RequestTransaction(RequestTransaction.TransactionType.Purchased, CurrentUser.userid, CurrentUser.username, working.description));
                        Guid requestorid = working.GetTransactionUser(RequestTransaction.TransactionType.Opened);
                        if (requestorid != Guid.Empty)
                        {
                            User u = new User();
                            u.Load(conn, requestorid);
                            List<string> to = new List<string>();
                            List<string> bcc = new List<string>();
                            bcc.Add("smcginnis@umces.edu");
                            to.Add(u.email);
                            // to.Add("smcginnis@hpl.umces.edu");
                            if (to.Count > 0)
                            {
                                string subject = "IMETPS: Purchase Request Purchased";
                                string body = "<p>Your purchase request has been purchased:</p><ul>";
                                body += "<li>Account Number: " + working.fasnumber.Number + "</li>";
                                body += "<li>Description: " + working.description + "</li>";
                                body += "<li>Purchased by: " + CurrentUser.username + "</li>";
                                body += "</ul>";
                                body += "<p><a href='http://10.1.17.29/ViewRequest.aspx?REQUESTID=" + working.requestid.ToString() + "'>Click here to view this request.</a></p>";
                                body += "Thank you,<br/>IMETPS System";
                                SendEmail(to.ToArray<string>(), null, bcc.ToArray<string>(), subject, body);
                            }
                        }
                    }
                }
                else if (mode == "receive")
                {                    
                    if (chkReceiveComplete.Checked)
                    {
                        working.state = PurchaseRequest.RequestState.received;
                        working.history.Add(new RequestTransaction(RequestTransaction.TransactionType.Received, CurrentUser.userid, CurrentUser.username, working.description));
                    }
                }
                else
                {
                    working.history.Add(new RequestTransaction(RequestTransaction.TransactionType.Created, CurrentUser.userid, CurrentUser.username, "Request revised."));
                }
                working.Save(conn);
                PopulateData(conn, working, true);

                SetSessionValue("WorkingPurchaseRequest", working);
            }
            catch (Exception ex)
            {
                ShowAlert("An error occurred while saving your request.  It may not be saved.\n  Message: " + ex.Message);
                SendErrorNotification(ex.Message, ex.StackTrace);
            }
            finally
            {
                conn.Close();
            }
        }

        protected void ReadInputs(PurchaseRequest working)
        {
            if (!txtDescription.ReadOnly && txtDescription.Visible)
                working.description = txtDescription.Text;
            if (!txtRequestorNotes.ReadOnly && txtRequestorNotes.Visible)
                working.requestornotes = txtRequestorNotes.Text;
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
                        return;
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
                        return;
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
                        return;
                    }
                }
                else
                    working.taxcharge = 0;
            }
            if (!txtExecutorNotes.ReadOnly && txtExecutorNotes.Visible)
                working.executornotes = txtExecutorNotes.Text;
            if (!txtPurchaserNotes.ReadOnly && txtPurchaserNotes.Visible)
                working.purchasernotes = txtPurchaserNotes.Text;
        }

        protected void btnApprove_Click(object sender, EventArgs e)
        {
            PurchaseRequest working = (PurchaseRequest)GetSessionValue("WorkingPurchaseRequest");
            SqlConnection conn = ConnectToConfigString("imetpsconnection");
            try
            {
                working.state = PurchaseRequest.RequestState.approved;
                working.history.Add(new RequestTransaction(RequestTransaction.TransactionType.Approved, CurrentUser.userid, CurrentUser.username, string.Empty));
                working.Save(conn);
                Guid requestorid = working.GetTransactionUser(RequestTransaction.TransactionType.Opened);
                if (requestorid != Guid.Empty && requestorid != CurrentUser.userid)
                {
                    User u = new User();
                    u.Load(conn, requestorid);
                    List<string> to = new List<string>();
                    List<string> bcc = new List<string>();
                    bcc.Add("smcginnis@umces.edu");
                    to.Add(u.email);
                    // to.Add("smcginnis@hpl.umces.edu");
                    if (to.Count > 0)
                    {
                        string subject = "[IMETPS] [INFO] Request " + working.tagnumber + " accepted by " + CurrentUser.username;
                        string body = "<p>A request on your FAS Number " + working.fasnumber.Number + " (" + working.fasnumber.Description + ") was accepted by " + CurrentUser.username + ", who has the ability ";
                        body += "to approve requests on your behalf. No action is required on your part. The request will be ";
                        body += "forwarded for purchase. Following is a summary of the request:</p>";
                        body += "<ul><li>IPS Request Tag Number: " + working.tagnumber + "</li>";
                        body += "<li>Requestor: <a href='mailto:" + CurrentUser.email + "'>" + CurrentUser.username + "</a></li>";
                        body += "<li>Requestor Notes: " + working.requestornotes + "</li>";
                        body += "<li>Description: " + working.description + "</li>";
                        body += "<li>Executor Notes: " + working.executornotes + "</li>";
                        body += "<li>Action Required: NONE</li></ul>";
                        body += "<p><a href='http://10.1.17.29/ViewRequest.aspx?REQUESTID=" + working.requestid.ToString() + "'>Click here to view this request.</a></p>";
                        body += "Thank you,<br/>IMETPS System";
                        SendEmail(to.ToArray<string>(), null, bcc.ToArray<string>(), subject, body);
                    }
                }
                List<User> purchasers = IMETPOClasses.User.LoadUsersWithPermission(conn, IMETPOClasses.User.Permission.purchaser);
                if (purchasers.Count > 0)
                {
                    List<string> to = new List<string>();
                    foreach (User u in purchasers)
                    {
                        if (!string.IsNullOrEmpty(u.email) && !to.Contains(u.email))
                        {
                            to.Add(u.email);
                        }
                    }
                    List<string> bcc = new List<string>();
                    bcc.Add("smcginnis@umces.edu");
                    // to.Add("smcginnis@hpl.umces.edu");
                    if (to.Count > 0)
                    {
                        string subject = "[IMETPS] Request " + working.tagnumber + " awaiting purchase";
                        string body = "<p>A Purchase Request has been approved and is awaiting purchase. Following is a summary ";
                        body += "of the request:</p>";
                        body += "<ul><li>IPS Tag Number: " + working.tagnumber + "</li>";
                        body += "<li>Description: " + working.description + "</li>";
                        body += "<li>Requestor Notes: " + working.requestornotes + "</li>";
                        body += "<li>Executor Notes: " + working.executornotes + "</li>";
                        body += "<li>Action Required: PURCHASE</li></ul>";
                        body += "<p><a href='http://10.1.17.29/SubmitRequest.aspx?REQUESTID=" + working.requestid.ToString() + "'>Click here to view this request.</a></p>";
                        body += "Thank you,<br/>IMETPS System";
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
                if(conn != null)
                    conn.Close();
            }
        }

        protected void btnReject_Click(object sender, EventArgs e)
        {
            PurchaseRequest working = (PurchaseRequest)GetSessionValue("WorkingPurchaseRequest");
            SqlConnection conn = ConnectToConfigString("imetpsconnection");
            try
            {
                working.state = PurchaseRequest.RequestState.rejected;
                working.history.Add(new RequestTransaction(RequestTransaction.TransactionType.Rejected, CurrentUser.userid, CurrentUser.username, string.Empty));
                working.Save(conn);
                Guid requestorid = working.GetTransactionUser(RequestTransaction.TransactionType.Opened);
                if (requestorid != Guid.Empty && requestorid != CurrentUser.userid)
                {
                    User u = new User();
                    u.Load(conn, requestorid);
                    List<string> to = new List<string>();
                    List<string> bcc = new List<string>();
                    bcc.Add("smcginnis@umces.edu");
                    to.Add(u.email);
                    // to.Add("smcginnis@hpl.umces.edu");
                    if (to.Count > 0)
                    {
                        string subject = "[IMETPS] [INFO] Request " + working.tagnumber + " rejected by " + CurrentUser.username;
                        string body = "<p>Your request, IPS Tag Number " + working.tagnumber + ", was rejected by " + CurrentUser.username + ". You must now modify ";
                        body += "this request according to the executor or remove it from the system. Following is a summary of the request:</p>";
                        body += "<ul><li>IPS Request Tag Number: " + working.tagnumber + "</li>";
                        body += "<li>Description: " + working.description + "</li>";
                        body += "<li>Executor Notes: " + working.executornotes + "</li>";
                        body += "<li>Action Required: MODIFY</li></ul>";
                        body += "<p><a href='http://10.1.17.29/ViewRequest.aspx?REQUESTID=" + working.requestid.ToString() + "'>Click here to view this request.</a></p>";
                        body += "Thank you,<br/>IMETPS System";
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

        protected void btnClose_Click(object sender, EventArgs e)
        {
            PurchaseRequest working = (PurchaseRequest)GetSessionValue("WorkingPurchaseRequest");
            SqlConnection conn = ConnectToConfigString("imetpsconnection");
            try
            {
                working.state = PurchaseRequest.RequestState.closed;
                working.history.Add(new RequestTransaction(RequestTransaction.TransactionType.Closed, CurrentUser.userid, CurrentUser.username, string.Empty));
                working.Save(conn);
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

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            PurchaseRequest working = (PurchaseRequest)GetSessionValue("WorkingPurchaseRequest");
            SqlConnection conn = ConnectToConfigString("imetpsconnection");
            try
            {
                working.state = PurchaseRequest.RequestState.deleted;
                working.history.Add(new RequestTransaction(RequestTransaction.TransactionType.Deleted, CurrentUser.userid, CurrentUser.username, string.Empty));
                working.Save(conn);
                Guid requestorid = working.GetTransactionUser(RequestTransaction.TransactionType.Opened);
                if (requestorid != Guid.Empty)
                {
                    User u = new User();
                    u.Load(conn, requestorid);
                    List<string> to = new List<string>();
                    List<string> bcc = new List<string>();
                    bcc.Add("smcginnis@umces.edu");
                    to.Add(u.email);
                    // to.Add("smcginnis@hpl.umces.edu");
                    if (to.Count > 0)
                    {
                        string subject = "[IMETPS] [INFO] Request " + working.tagnumber + " deleted";
                        string body = "<p>Your request, IPS Tag Number " + working.tagnumber + ", has been marked as deleted. Following is a ";
                        body += "summary of the request:</p>";
                        body += "<ul><li>IPS Tag Number: " + working.tagnumber + "</li>";
                        body += "<li>Description: " + working.description + "</li>";
                        body += "<li>Purchaser Notes: " + working.purchasernotes + "</li>";
                        body += "<li>Action Required: NONE</li></ul>";
                        body += "<p><a href='http://10.1.17.29/ViewRequest.aspx?REQUESTID=" + working.requestid.ToString() + "'>Click here to view this request.</a></p>";
                        body += "Thank you,<br/>IMETPS System";
                        SendEmail(to.ToArray<string>(), null, bcc.ToArray<string>(), subject, body);
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
    }
}
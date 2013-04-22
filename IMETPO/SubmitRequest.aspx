<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SubmitRequest.aspx.cs"
    Inherits="IMETPO.SubmitRequest" EnableViewState="true" EnableViewStateMac="true" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>IMET Purchasing System</title>
    <link href="Styles/imetps.css?t=<%= DateTime.Now.Ticks %>" type="text/css" rel="stylesheet" />
    <script type="text/javascript" language="javascript">

        function deleteLineItem(lineid) {
            var el = document.getElementById("txtUOM_" + lineid);
            if (el) {
                el.value = "";
            }
            el = document.getElementById("txtLineItemDesc_" + lineid);
            if (el) {
                el.value = "";
            }
            el = document.getElementById("txtLineItemNumber_" + lineid);
            if (el) {
                el.value = "";
            }
            el = document.getElementById("txtUnitPrice_" + lineid);
            if (el) {
                el.value = "";
            }
            el = document.getElementById("txtQuantity_" + lineid);
            if (el) {
                el.value = "";
            }
        }

        function updateTotal() 
        {
            var ids = document.getElementById("hiddenLineItemIDs");
            var rows = ids.value.split(",");
            if (!rows) {
                return;
            }
            var total = 0;
            for (var i = 0; i < rows.length; i++) 
            {
                var lineitemid = rows[i];
                if (!lineitemid || lineitemid.length <= 0) 
                {
                    continue;
                }
                var qty = document.getElementById("txtQuantity_" + lineitemid);
                var price = document.getElementById("txtUnitPrice_" + lineitemid);
                var total_label = document.getElementById("lblTotalPrice_" + lineitemid);
                if (!qty || !price || qty.value.length <= 0 || price.value.length <= 0) 
                {
                    continue;
                }
                if (!total_label) 
                {
                    continue;
                }
                total_label.innerHTML = "";
                try {
                    var qty_value = parseFloat(qty.value);
                    if (isNaN(qty_value)) {
                        alert("One of your line items has an invalid quantity.");
                        total_label.innerHTML = "ERROR";
                        continue;
                    }
                    var price_value = parseFloat(price.value);
                    if (isNaN(price_value)) {
                        alert("One of your line items has an invalid price.");
                        total_label.innerHTML = "ERROR";
                        continue;
                    }
                    var subtotal = qty_value * price_value;
                    total_label.innerHTML = "$" + subtotal.toFixed(2).toString();
                    total += subtotal;
                }
                catch (err) 
                {
                    alert("Price error!  Make sure you have entered a valid quantity and unit price.");
                }
            }
            var el = document.getElementById("hiddenLineItemTotal");
            if (el) 
            {
                total += parseFloat(el.value);
            }
            el = document.getElementById("txtMiscCharges");
            if (el && el.value.length > 0) 
            {
                total += parseFloat(el.value);
            }
            el = document.getElementById("txtTaxCharges");
            if (el && el.value.length > 0) 
            {
                total += parseFloat(el.value);
            }
            el = document.getElementById("txtShippingCharges");
            if (el && el.value.length > 0) 
            {
                total += parseFloat(el.value);
            }
            el = document.getElementById("lblTotalPrice");
            if (el) 
            {
                el.innerHTML = "$" + total.toFixed(2).toString();
            }
        }

        function removeAttachment(attachmentid) {
            var el = document.getElementById("hiddenLineItems");
            el.value = "REMOVEATTACHMENT:" + attachmentid;
            form1.submit();
        }

        function sstchur_SmartScroller_GetCoords() {
            var scrollX, scrollY;

            if (document.all) {
                if (!document.documentElement.scrollTop)
                    scrollY = document.body.scrollTop;
                else
                    scrollY = document.documentElement.scrollTop;
            }
            else {
                scrollY = window.pageYOffset;
            }

            document.forms["form1"].hiddenScrollPos.value = scrollY;
        }

        function sstchur_SmartScroller_Scroll() {
            var y = document.forms["form1"].hiddenScrollPos.value;
            window.scrollTo(0, y);
        }

        function checkInt(controlid) {
            var el = document.getElementById(controlid);
            if (!el) {
                return;
            }
            if (el.value.length <= 0) {
                return;
            }
            try {
                var test = parseInt(el.value);
            }
            catch (err) {
                alert("Error: That field can only contain integers.  Please revise.");
                el.value = "";
            }
        }

        window.onload = sstchur_SmartScroller_Scroll;
        window.onscroll = sstchur_SmartScroller_GetCoords;
        window.onkeypress = sstchur_SmartScroller_GetCoords;
        window.onclick = sstchur_SmartScroller_GetCoords;
    </script>
</head>
<body>
    <form id="form1" runat="server">
    <div id="headerTitle" class="banner" runat="server">
        <img class="logo" src="images/IMET_logo.png" width="398" height="72" alt="IMET" />
    </div>
    <div id="Div1" class="title">
        <center>
            <span id="titlespan" runat="server"></span>
        </center>
    </div>
    <div id="mainbody">
        <div id="backlink">
            <a href="Default.aspx">Return to main menu</a>
        </div>
        <div id="header" runat="server" class="menu">
            <h3>
                <span id="title" runat="server">Submit a purchase request</span></h3>
            <p>
                <font color="red"><sup>*</sup></font>Indicates required information.</p>
            <table border="0px" class="submitheader">
                <tr>
                    <td style="width: 20%">
                        Requestor<font color="red"><sup>*</sup></font>
                    </td>
                    <td style="width: 20%">
                        Account Number<font color="red"><sup>*</sup></font>
                    </td>
                    <td style="width: 20%">
                        Second Account Number
                    </td>
                    <td style="width: 20%">
                        IPS Number
                    </td>
                    <td style="width: 20%">
                        Status
                    </td>
                </tr>
                <tr>
                    <td>
                        <asp:DropDownList ID="comboRequester" runat="server" Width="100%">
                        </asp:DropDownList>
                    </td>
                    <td>
                        <asp:DropDownList ID="comboFASNumber" runat="server" Width="100%">
                        </asp:DropDownList>
                    </td>
                    <td>
                        <asp:DropDownList ID="comboalt_FASnumber" runat="server" Width="100%">
                        </asp:DropDownList>
                        <p style="font-size: x-small">
                            If More than one account is being charged, please provide the dollar amount or percentage
                            of purchase to be charged to each account in the Special Instructions/Notes field.</p>
                    </td>
                    <td>
                        <asp:Label ID="lblTagNumber" runat="server" Text=""></asp:Label>
                    </td>
                    <td>
                        <asp:Label ID="lblStatus" runat="server" Text=""></asp:Label>
                    </td>
                </tr>
                <tr id="rowVendorDetails" runat="server">
                    <td colspan="2" valign="top">
                        <h5>
                            Requested Vendor Information</h5>
                        <table border="0px" style="width: 100%">
                            <tr>
                                <td style="width: 25%">
                                    Existing Vendors
                                </td>
                                <td style="width: 75%">
                                    <asp:DropDownList ID="comboVendors" runat="server" AutoPostBack="true" OnSelectedIndexChanged="comboVendors_SelectedIndexChanged"
                                        Width="100%">
                                    </asp:DropDownList>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    Vendor Name
                                </td>
                                <td>
                                    <asp:TextBox ID="txtVendorName" runat="server" Width="100%"></asp:TextBox>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    Vendor URL
                                </td>
                                <td>
                                    <asp:TextBox ID="txtVendorURL" runat="server" Width="100%"></asp:TextBox>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    Vendor Description
                                </td>
                                <td>
                                    <asp:TextBox ID="txtVendorDescription" runat="server" Width="100%"></asp:TextBox>
                                </td>
                            </tr>
                            <tr id="rowVendorContactName" runat="server">
                                <td>
                                    Contact Name:
                                </td>
                                <td>
                                    <asp:TextBox ID="txtVendorcontact_name" runat="server" Width="100%" ReadOnly="true"></asp:TextBox>
                                </td>
                            </tr>
                            <tr id="rowVendorContactPhone" runat="server">
                                <td>
                                    Contact Phone:
                                </td>
                                <td>
                                    <asp:TextBox ID="txtVendorcontact_phone" runat="server" Width="100%" ReadOnly="true"></asp:TextBox>
                                </td>
                            </tr>
                            <tr id="rowVendorContactEmail" runat="server">
                                <td>
                                    Contact email:
                                </td>
                                <td>
                                    <asp:TextBox ID="txtVendorcontact_email" runat="server" Width="100%" ReadOnly="true"></asp:TextBox>
                                </td>
                            </tr>
                            <tr id="rowVendorContactAccountNumber" runat="server">
                                <td>
                                    Customer Account #:
                                </td>
                                <td>
                                    <asp:TextBox ID="txtVendor_customer_account_number" runat="server" Width="100%" ReadOnly="true"></asp:TextBox>
                                </td>
                            </tr>
                        </table>
                    </td>
                    <td colspan="3" valign="top">
                        <h5 id="vendorDetailsHeader" runat="server">
                            Requested Vendor Details</h5>
                        <table border="0" id="vendorDetails" runat="server" width="100%" class="vendorDetails">
                            <tr style="display: none">
                                <td width="10%">
                                </td>
                                <td width="40%">
                                </td>
                                <td width="10%">
                                </td>
                                <td width="40%">
                                </td>
                            </tr>
                            <tr>
                                <td colspan="1">
                                    Address Line 1:
                                </td>
                                <td colspan="3">
                                    <asp:TextBox ID="txtVendorAddress1" runat="server" Width="100%" ReadOnly="true"></asp:TextBox>
                                </td>
                            </tr>
                            <tr>
                                <td colspan="1">
                                    Address Line 2:
                                </td>
                                <td colspan="3">
                                    <asp:TextBox ID="txtVendorAddress2" runat="server" Width="100%" ReadOnly="true"></asp:TextBox>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    City:
                                </td>
                                <td>
                                    <asp:TextBox ID="txtVendorCity" runat="server" Width="100%" ReadOnly="true"></asp:TextBox>
                                </td>
                                <td>
                                    State:
                                </td>
                                <td>
                                    <asp:TextBox ID="txtVendorState" runat="server" Width="100%" ReadOnly="true"></asp:TextBox>
                                </td>
                            </tr>
                            <tr>
                                <td colspan="1">
                                    Postal Code:
                                </td>
                                <td colspan="3">
                                    <asp:TextBox ID="txtVendorPostalCode" runat="server" ReadOnly="true" Width="100%"></asp:TextBox>
                                </td>
                            </tr>
                            <tr>
                                <td colspan="1">
                                    FEIN:
                                </td>
                                <td colspan="3">
                                    <asp:TextBox ID="txtVendorFEIN" runat="server" Width="100%" ReadOnly="true"></asp:TextBox>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    Phone #:
                                </td>
                                <td>
                                    <asp:TextBox ID="txtVendorPhone" runat="server" Width="100%" ReadOnly="true"></asp:TextBox>
                                </td>
                                <td>
                                    Fax #:
                                </td>
                                <td>
                                    <asp:TextBox ID="txtVendorFax" runat="server" Width="100%" ReadOnly="true"></asp:TextBox>
                                </td>
                            </tr>
                        </table>
                    </td>
                </tr>
            </table>
        </div>
        <div id="lineitems" class="menu">
            <h3>
                Order Information</h3>
            <p>
                Please Enter your order information then click “Add Item(s)” to add the line to
                your order</p>
            <asp:Table ID="tblLineItems" runat="server" CssClass="lineitems" Width="100%">
                <asp:TableHeaderRow>
                    <asp:TableHeaderCell Width="5%">Qty<font color="red"><sup>*</sup></font></asp:TableHeaderCell>
                    <asp:TableHeaderCell Width="5%">Unit<font color="red"><sup>*</sup></font></asp:TableHeaderCell>
                    <asp:TableHeaderCell Width="10%">Item Number</asp:TableHeaderCell>
                    <asp:TableHeaderCell Width="40%" ColumnSpan="2">Description<font color="red"><sup>*</sup></font></asp:TableHeaderCell>
                    <asp:TableHeaderCell Width="7%">Unit Price<font color="red"><sup>*</sup></font></asp:TableHeaderCell>
                    <asp:TableHeaderCell Width="7%">Total Price<font color="red"><sup>*</sup></font></asp:TableHeaderCell>
                    <asp:TableHeaderCell ID="headerQuantityReceived" runat="server" Width="7%">Qty Rec'd</asp:TableHeaderCell>
                    <asp:TableHeaderCell ID="headerReceived" runat="server" Width="7%">Item Received</asp:TableHeaderCell>
                    <asp:TableHeaderCell ID="headerToBeInventoried" runat="server" Width="7%">To be Inventoried</asp:TableHeaderCell>
                    <asp:TableHeaderCell Width="7%">Action</asp:TableHeaderCell>
                </asp:TableHeaderRow>
                <asp:TableFooterRow>
                    <asp:TableHeaderCell ColumnSpan="4">
                        <div id="attachments" runat="server" style="padding-left: 20%">
                            <table border="0px" class="controls">
                                <tr runat="server" id="rowUploadHelp">
                                    <td>
                                        <span style="font-size: small">
                                            <p>
                                                <strong>To Place an Order with a Quote:</strong> Please upload a copy of the quote
                                                here. Complete the order with the following information: Quantity and Unit as 1,
                                                and enter the total quoted price as the unit price. Write “See attached quote” in
                                                the Description field.</p>
                                            <p>
                                                Click ‘Choose File’, select the file from your documents, then click upload.</p>
                                        </span>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <span id="filedownloadlink" runat="server"></span>
                                    </td>
                                </tr>
                                <tr runat="server" id="rowUpload">
                                    <td>
                                        <asp:FileUpload ID="uploadAttachment" runat="server" Width="50%" /><br />
                                        <asp:LinkButton CssClass="squarebutton" ID="btnUpload" runat="server" OnClick="btnUpload_Click"><span>Upload</span></asp:LinkButton>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <span style="font-size: small">Enter link to shopping cart or items to purchase here.
                                            Include pricing information in the table above.</span>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <span id="cartlink" runat="server"></span>
                                        <asp:TextBox ID="txtCartLink" Width="100%" runat="server"></asp:TextBox>
                                    </td>
                                </tr>
                            </table>
                        </div>
                    </asp:TableHeaderCell>
                    <asp:TableHeaderCell ColumnSpan="3">
                        <table border="0px" width="100%" style="text-align: right">
                            <tr>
                                <td width="80%">
                                    Miscellaneous charges:
                                </td>
                                <td align="right" width="20%">
                                    <asp:TextBox ID="txtMiscCharges" runat="server" onchange="updateTotal()"></asp:TextBox>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    Shipping and Handling charges: (enter 1.00 if unknown)<font color="red"><sup>*</sup></font>
                                </td>
                                <td align="right">
                                    <asp:TextBox ID="txtShippingCharges" runat="server" onchange="updateTotal()"></asp:TextBox>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    Tax Charges (exempt for State of MD)
                                </td>
                                <td align="right">
                                    <asp:TextBox ID="txtTaxCharges" runat="server" onchange="updateTotal()"></asp:TextBox>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    <b>Total order price:</b>
                                </td>
                                <td align="right">
                                    <asp:Label ID="lblTotalPrice" runat="server" Text="$0.00"></asp:Label>
                                </td>
                            </tr>
                        </table>
                    </asp:TableHeaderCell>
                    <asp:TableHeaderCell ColumnSpan="3"></asp:TableHeaderCell>
                    <asp:TableHeaderCell>
                        <asp:LinkButton ID="btnAddRows" runat="server" OnClick="btnAddRows_Click">Add Line Items</asp:LinkButton></asp:TableHeaderCell>
                </asp:TableFooterRow>
            </asp:Table>
        </div>
        <div id="additionalinfo" class="menu">
            <asp:HiddenField ID="hiddenLineItems" runat="server" />
            <asp:HiddenField ID="hiddenLineItemIDs" runat="server" />
            <asp:HiddenField ID="hiddenScrollPos" runat="server" />
            <table border="0px" width="90%">
                <!--<tr id="rowBypass" runat="server">
                    <td colspan="2">
                        I, hereby certify that I am the P.I., or have obtained approval from the P.I. to
                        request this purchase:
                        <asp:CheckBox ID="chkBypassApproved" runat="server" />
                    </td>
                </tr>-->
                <tr id="rowRequisitionNumber" runat="server">
                    <td colspan="2">
                        <span style="font-weight: bold">Requisition Number</span><font color="red"><sup>*</sup></font>:
                        <asp:Label ID="lblRequisitionNumber" runat="server" Text=""></asp:Label>
                        <asp:TextBox ID="txtRequisitionNumber" runat="server"></asp:TextBox>
                    </td>
                </tr>
                <tr>
                    <td colspan="2">
                        <p>
                            Justification for how the items requested above relate solely to the grant(s) being
                            charged<font color="red"><sup>*</sup></font></p>
                        <div id="lblDescription" runat="server" class="textfield" width="80%">
                        </div>
                        <asp:TextBox ID="txtDescription" runat="server" Width="80%" Rows="4" TextMode="MultiLine"></asp:TextBox>
                    </td>
                </tr>
                <tr>
                    <td colspan="2">
                        <p>
                            Special Instructions (charges split between multiple accounts, rush order, radioactive
                            materials, etc.):</p>
                        <div id="lblRequestorNotes" runat="server" class="textfield" width="80%">
                        </div>
                        <asp:TextBox ID="txtRequestorNotes" runat="server" Rows="4" TextMode="MultiLine"
                            Width="80%"></asp:TextBox>
                    </td>
                </tr>
                <tr id="rowExecutorNotes" runat="server">
                    <td colspan="2">
                        <p>
                            Approver's Notes:</p>
                        <div id="lblExecutorNotes" runat="server" class="textfield" width="80%">
                        </div>
                        <asp:TextBox ID="txtExecutorNotes" runat="server" Rows="4" TextMode="MultiLine" Width="80%"></asp:TextBox>
                    </td>
                </tr>
                <tr id="rowPurchaserNotes" runat="server">
                    <td colspan="2">
                        <p>
                            Purchaser's Notes:</p>
                        <asp:TextBox ID="txtPurchaserNotes" runat="server" Rows="4" TextMode="MultiLine"
                            Width="80%" CssClass="textfield" BorderWidth="1" BorderStyle="Solid"></asp:TextBox>
                    </td>
                </tr>
                <tr id="rowPurchaseComplete" runat="server">
                    <td colspan="2">
                        Checking this box will send an e-mail to the requestor stating that the items requested
                        have been purchased:
                        <asp:CheckBox ID="chkPurchaseComplete" runat="server" Text="This request has been purchased." />
                    </td>
                </tr>
            </table>
        </div>
        <div id="controls" class="menu">
            <table class="controls">
                <tr>
                    <td>
                        <asp:LinkButton CssClass="squarebutton" ID="btnDelete" runat="server" OnClick="btnDelete_Click"><span>Delete this Purchase Request</span></asp:LinkButton>
                    </td>
                    <td>
                        <asp:LinkButton CssClass="squarebutton" ID="btnSubmit" runat="server" OnClick="btnSubmit_Click"><span>Submit this Purchase Request</span></asp:LinkButton>
                    </td>
                    <td>
                        <asp:LinkButton CssClass="squarebutton" ID="btnApprove" runat="server" OnClick="btnApprove_Click"><span>Approve this Purchase Request</span></asp:LinkButton>
                    </td>
                    <td>
                        <asp:LinkButton CssClass="squarebutton" ID="btnReject" runat="server" OnClick="btnReject_Click"><span>Reject this Purchase Request</span></asp:LinkButton>
                    </td>
                    <td>
                        <asp:LinkButton CssClass="squarebutton" ID="btnOverrideAutoApprove" runat="server"
                            OnClick="btnOverrideAutoApprove_Click"><span>Return Request for Approval</span></asp:LinkButton>
                    </td>
                    <td>
                        <span id="printlink" runat="server"></span>
                    </td>
                    <td>
                        <a href="Default.aspx" class="squarebutton"><span>Return to Main Menu</span></a>
                    </td>
                </tr>
            </table>
        </div>
        <div id="history" runat="server" class="menu">
        </div>
    </div>
    </form>
</body>
</html>

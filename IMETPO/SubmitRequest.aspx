<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SubmitRequest.aspx.cs"
    Inherits="IMETPO.SubmitRequest" EnableViewState="true" EnableViewStateMac="true" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>IMET Purchasing System</title>
    <link href="Styles/imetps.css?t=<%= DateTime.Now.Ticks %>" type="text/css" rel="stylesheet" />
    <script type="text/javascript" language="javascript">
        function submitLineItem(lineid) {
            var el = document.getElementById("hiddenLineItems");
            el.value = "EDIT:" + lineid;
            form1.submit();
        }

        function deleteLineItem(lineid) {
            var el = document.getElementById("hiddenLineItems");
            el.value = "DELETE:" + lineid;
            form1.submit();
        }

        function updateTotal() {
            var total = 0;
            var el = document.getElementById("hiddenLineItemTotal");
            if (el) {
                total += parseFloat(el.value);
            }
            el = document.getElementById("txtMiscCharges");
            if (el && el.value.length > 0) {
                total += parseFloat(el.value);
            }
            el = document.getElementById("txtTaxCharges");
            if (el && el.value.length > 0) {
                total += parseFloat(el.value);
            }
            el = document.getElementById("txtShippingCharges");
            if (el && el.value.length > 0) {
                total += parseFloat(el.value);
            }
            el = document.getElementById("lblTotalPrice");
            if (el) {
                el.innerHTML = "$" + total.toFixed(2).toString();
            }
        }
    </script>
</head>
<body>
    <form id="form1" runat="server">
    <div class="top">
        <h1>
            <span id="appTitle" runat="server"></span></h1>
        <p>
            <span id="appSubtitle" runat="server"></span></p>
    </div>
    <div id="mainbody">
        <div id="backlink">
            <a href="Default.aspx">Return to main menu</a>
        </div>
        <div id="header" runat="server" class="menu">
            <h3>
                Submit a purchase request</h3>
            <p>
                <font color="red"><sup>*</sup></font>Indicates required information.</p>
            <table border="0px" width="90%">
                <tr>
                    <td>
                        Requestor<font color="red"><sup>*</sup></font>
                    </td>
                    <td>
                        FAS Number<font color="red"><sup>*</sup></font>
                    </td>
                    <td>
                        Tag Number
                    </td>
                    <td>
                        Status
                    </td>
                </tr>
                <tr>
                    <td>
                        <asp:DropDownList ID="comboRequester" runat="server">
                        </asp:DropDownList>
                    </td>
                    <td>
                        <asp:DropDownList ID="comboFASNumber" runat="server">
                        </asp:DropDownList>
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
                        <h5>Requested Vendor Information</h5>
                        <table border="0px">
                            <tr>
                                <td>
                                    Existing Vendors
                                </td>
                                <td>
                                    <asp:DropDownList ID="comboVendors" runat="server" AutoPostBack="true" OnSelectedIndexChanged="comboVendors_SelectedIndexChanged">
                                    </asp:DropDownList>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    Vendor Name
                                </td>
                                <td>
                                    <asp:TextBox ID="txtVendorName" runat="server"></asp:TextBox>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    Vendor URL
                                </td>
                                <td>
                                    <asp:TextBox ID="txtVendorURL" runat="server"></asp:TextBox>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    Vendor Description
                                </td>
                                <td>
                                    <asp:TextBox ID="txtVendorDescription" runat="server"></asp:TextBox>
                                </td>
                            </tr>
                        </table>
                    </td>
                    <td colspan="2" valign="top">
                        <h5>Requested Vendor Details</h5>
                        <table border="0" id="vendorDetails" runat="server">
                            <tr>
                                <td colspan="1">
                                    Address Line 1:
                                </td>
                                <td colspan="3">
                                    <asp:TextBox ID="txtVendorAddress1" runat="server" Width="320px" ReadOnly="true"></asp:TextBox>
                                </td>
                            </tr>
                            <tr>
                                <td colspan="1">
                                    Address Line 2:
                                </td>
                                <td colspan="3">
                                    <asp:TextBox ID="txtVendorAddress2" runat="server" Width="320px" ReadOnly="true"></asp:TextBox>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    City:
                                </td>
                                <td>
                                    <asp:TextBox ID="txtVendorCity" runat="server" Width="180px" ReadOnly="true"></asp:TextBox>
                                </td>
                                <td>
                                    State:
                                </td>
                                <td>
                                    <asp:TextBox ID="txtVendorState" runat="server" Width="180px" ReadOnly="true"></asp:TextBox>
                                </td>
                            </tr>
                            <tr>
                                <td colspan="1">
                                    Postal Code:
                                </td>
                                <td colspan="3">
                                    <asp:TextBox ID="txtVendorPostalCode" runat="server" ReadOnly="true"></asp:TextBox>
                                </td>
                            </tr>
                            <tr>
                                <td colspan="1">
                                    FEIN:
                                </td>
                                <td colspan="3">
                                    <asp:TextBox ID="txtVendorFEIN" runat="server" Width="320px" ReadOnly="true"></asp:TextBox>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    Phone #:
                                </td>
                                <td>
                                    <asp:TextBox ID="txtVendorPhone" runat="server" Width="160px" ReadOnly="true"></asp:TextBox>
                                </td>
                                <td>
                                    Fax #:
                                </td>
                                <td>
                                    <asp:TextBox ID="txtVendorFax" runat="server" Width="160px" ReadOnly="true"></asp:TextBox>
                                </td>
                            </tr>
                        </table>
                    </td>
                </tr>
            </table>
            </div>
            <div id="lineitems" class="menu">
            <asp:Table ID="tblLineItems" runat="server" CssClass="lineitems">
                <asp:TableHeaderRow>
                    <asp:TableHeaderCell>Quantity<font color="red"><sup>*</sup></font></asp:TableHeaderCell>
                    <asp:TableHeaderCell>Unit of Quantity<font color="red"><sup>*</sup></font></asp:TableHeaderCell>
                    <asp:TableHeaderCell>Description<font color="red"><sup>*</sup></font></asp:TableHeaderCell>
                    <asp:TableHeaderCell>Item Number</asp:TableHeaderCell>
                    <asp:TableHeaderCell>Unit Price<font color="red"><sup>*</sup></font></asp:TableHeaderCell>
                    <asp:TableHeaderCell>Total Price<font color="red"><sup>*</sup></font></asp:TableHeaderCell>
                    <asp:TableHeaderCell ID="headerQuantityReceived" runat="server">Quantity Received</asp:TableHeaderCell>
                    <asp:TableHeaderCell ID="headerToBeInventoried" runat="server">To be Inventoried</asp:TableHeaderCell>
                    <asp:TableHeaderCell>Action</asp:TableHeaderCell>
                </asp:TableHeaderRow>
                <asp:TableRow ID="newItemRow">
                    <asp:TableCell>
                        <asp:TextBox ID="txtQuantity_new" runat="server"></asp:TextBox></asp:TableCell>
                    <asp:TableCell>
                        <asp:TextBox ID="txtUOM_new" runat="server"></asp:TextBox></asp:TableCell>
                    <asp:TableCell>
                        <asp:TextBox ID="txtLineItemDesc_new" runat="server"></asp:TextBox></asp:TableCell>
                        <asp:TableCell>
                        <asp:TextBox ID="txtLineItemNumber_new" runat="server"></asp:TextBox></asp:TableCell>
                    <asp:TableCell>
                        <asp:TextBox ID="txtUnitPrice_new" runat="server"></asp:TextBox></asp:TableCell>
                    <asp:TableCell>
                        <asp:Label ID="lblTotalPrice_new" runat="server" Text=""></asp:Label></asp:TableCell>
                    <asp:TableCell></asp:TableCell>
                    <asp:TableCell></asp:TableCell>
                    <asp:TableCell>
                        <asp:LinkButton ID="btnAddNewLineItem" runat="server" OnClick="btnAddNewLineItem_Click">Add Item</asp:LinkButton>
                    </asp:TableCell>
                </asp:TableRow>
                <asp:TableFooterRow>
                    <asp:TableHeaderCell></asp:TableHeaderCell>
                    <asp:TableHeaderCell></asp:TableHeaderCell>
                    <asp:TableHeaderCell></asp:TableHeaderCell>
                    <asp:TableHeaderCell ColumnSpan="3">
                        <table border="0px" width="100%">
                            <tr>
                                <td>
                                    Miscellaneous charges:
                                </td>
                                <td align="right">
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
                                    Tax charges:
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
                    <asp:TableHeaderCell></asp:TableHeaderCell>
                    <asp:TableHeaderCell></asp:TableHeaderCell>
                    <asp:TableHeaderCell></asp:TableHeaderCell>
                </asp:TableFooterRow>
            </asp:Table>
            </div>
            <div id="additionalinfo" class="menu">
            <asp:HiddenField ID="hiddenLineItems" runat="server" />
            <asp:HiddenField ID="hiddenLineItemTotal" runat="server" />
            <table border="0px" width="90%">
                <tr id="rowBypass" runat="server">
                    <td>
                        I, hereby certify that I am the P.I., or have obtained approval from the P.I. to
                        request this purchase:
                    </td>
                    <td>
                        <asp:CheckBox ID="chkBypassApproved" runat="server" />
                    </td>
                </tr>
                <tr id="rowRequisitionNumber" runat="server">
                    <td>
                        Requisition Number:
                    </td>
                    <td>
                        <asp:Label ID="lblRequisitionNumber" runat="server" Text=""></asp:Label>
                        <asp:TextBox ID="txtRequisitionNumber" runat="server"></asp:TextBox>
                    </td>
                </tr>
                <tr>
                    <td>
                        Justification for this request:<font color="red"><sup>*</sup></font>
                    </td>
                    <td>
                        <asp:Label ID="lblDescription" runat="server" Text=""></asp:Label>
                        <asp:TextBox ID="txtDescription" runat="server" Width="812px" Rows="2" TextMode="MultiLine"></asp:TextBox>
                    </td>
                </tr>
                <tr>
                    <td>
                        Special Instructions/Notes:
                    </td>
                    <td>
                        <asp:Label ID="lblRequestorNotes" runat="server" Text=""></asp:Label>
                        <asp:TextBox ID="txtRequestorNotes" runat="server" Rows="4" TextMode="MultiLine"
                            Width="632px"></asp:TextBox>
                    </td>
                </tr>
                <tr id="rowExecutorNotes" runat="server">
                    <td>
                        Executor's Notes:
                    </td>
                    <td>
                        <asp:Label ID="lblExecutorNotes" runat="server" Text=""></asp:Label>
                        <asp:TextBox ID="txtExecutorNotes" runat="server" Rows="4" TextMode="MultiLine" Width="632px"></asp:TextBox>
                    </td>
                </tr>
                <tr id="rowPurchaserNotes" runat="server">
                    <td>
                        Purchaser's Notes:
                    </td>
                    <td>
                        <asp:Label ID="lblPurchaserNotes" runat="server" Text=""></asp:Label>
                        <asp:TextBox ID="txtPurchaserNotes" runat="server" Rows="4" TextMode="MultiLine"
                            Width="632px"></asp:TextBox>
                    </td>
                </tr>
                <tr id="rowPurchaseComplete" runat="server">
                    <td><asp:CheckBox ID="chkPurchaseComplete" runat="server" Text="This request has been purchased." /></td>
                </tr>
                <tr id="rowReceiveComplete" runat="server">
                    <td><asp:CheckBox ID="chkReceiveComplete" runat="server" Text="This request has been received." /></td>
                </tr>
            </table>
            </div>
            <div id="controls" class="menu">
            <table class="controls">
                <tr>
                    <td>
                        <asp:LinkButton ID="btnDelete" runat="server" OnClick="btnDelete_Click">Delete this Purchase Request</asp:LinkButton>
                    </td>
                    <td>
                        <asp:LinkButton ID="btnSubmit" runat="server" OnClick="btnSubmit_Click">Submit this Purchase Request</asp:LinkButton>
                    </td>
                    <td>
                        <asp:LinkButton ID="btnApprove" runat="server" OnClick="btnApprove_Click">Approve this Purchase Request</asp:LinkButton>
                    </td>
                    <td>
                        <asp:LinkButton ID="btnReject" runat="server" OnClick="btnReject_Click">Reject this Purchase Request</asp:LinkButton>
                    </td>
                    <td>
                        <asp:LinkButton ID="btnClose" runat="server" OnClick="btnClose_Click">Close this Purchase Request</asp:LinkButton>
                    </td>
                    <td><span id="printlink" runat="server"></span></td>
                </tr>
            </table>
            </div>
            <div id="history" runat="server" class="menu">
            </div>
        </div>
    </form>
</body>
</html>

<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ViewRequest.aspx.cs" Inherits="IMETPO.ViewRequest" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>IMET Purchasing System</title>
    <link href="Styles/imetps_print.css?t=<%= DateTime.Now.Ticks %>" type="text/css"
        rel="stylesheet" />
</head>
<body>
    <form id="form1" runat="server">
    <div class="top">
        <table border="0" width="100%">
            <tr>
                <td align="left" style="width:60%">
                    <img src="images/IMET_logo.png" style="width: 100%" alt="IMET: Institute of Marine & Environmental Technology"
                        runat="server" id="logoImage" />
                </td>
                <td align="right">
                    <h1>
                        <span id="span_fasnumber" runat="server"></span>
                    </h1>
                    <p>
                        <span id="span_reqnumber" runat="server"></span>
                    </p>
                </td>
            </tr>
        </table>
    </div>   
    <div style="margin-left:auto; margin-right:auto;width: 400px;">
        <h1><span id="top_requisition" runat="server"></span></h1>
    </div>
    <div id="mainbody" class="content">
        <div id="header" class="menu">
            <div id="acknowledgement" runat="server">
                <h3>
                    Purchase Request Submitted</h3>
                <p>
                    Your purchase request has been received. Your purchase request number is <span id="ack_tagnumber"
                        runat="server"></span>.</p>
            </div>
            <table border="0px" width="90%">
                <tr>
                    <td>
                        <h4>
                            Vendor Information</h4>
                    </td>
                    <td>
                        <h4>
                            Account Number</h4>
                    </td>
                    <td>
                        <h4>
                            IPS Number</h4>
                    </td>
                    <td>
                        <h4>
                            Requestor</h4>
                    </td>
                </tr>
                <tr>
                    <td>
                        <span id="span_vendorinfo" runat="server"></span>
                    </td>
                    <td>
                        <span id="span_fasnumber2" runat="server"></span>
                    </td>
                    <td>
                        <span id="span_tagnumber" runat="server"></span>
                    </td>
                    <td>
                        <span id="span_requestor" runat="server"></span>
                    </td>
                </tr>
            </table>
        </div>
        <div id="details" runat="server" class="menu">
            <asp:Table ID="tbldetails" runat="server" CssClass="lineitems">
                <asp:TableHeaderRow>
                    <asp:TableHeaderCell>Quantity</asp:TableHeaderCell>
                    <asp:TableHeaderCell>Unit of Quantity</asp:TableHeaderCell>
                    <asp:TableHeaderCell>Item Number</asp:TableHeaderCell>
                    <asp:TableHeaderCell>Description</asp:TableHeaderCell>
                    <asp:TableHeaderCell>Unit Price</asp:TableHeaderCell>
                    <asp:TableHeaderCell>Total Price</asp:TableHeaderCell>
                    <asp:TableHeaderCell>Quantity Received</asp:TableHeaderCell>
                </asp:TableHeaderRow>
                <asp:TableFooterRow>
                    <asp:TableHeaderCell ColumnSpan="4" RowSpan="4">
                        <table border="0">
                            <tr id="rowAttachmentHelp" runat="server">
                                <td>
                                    <span style="font-size: x-small">To place an order with a quote, please upload a copy
                                        of the quote here. Complete the order with the following information: Quantity and
                                        Unit as 1, and enter the total quoted price as the unit price. Write “See attached
                                        quote” in the Description field.</span>
                                </td>
                            </tr>
                            <tr id="rowAttachment" runat="server">
                                <td>
                                    <span id="attachmentlink" runat="server"></span>
                                </td>
                            </tr>
                            <tr id="rowLinkHelp" runat="server">
                                <td>
                                    <span style="font-size: x-small">Enter link to shopping cart or items to purchase here.
                                        Include pricing information in the table above.</span>
                                </td>
                            </tr>
                            <tr id="rowLink" runat="server">
                                <td>
                                    <span id="shoppingcartlink" runat="server"></span>
                                </td>
                            </tr>
                        </table>
                    </asp:TableHeaderCell>
                    <asp:TableHeaderCell>Miscellaneous charges:</asp:TableHeaderCell>
                    <asp:TableHeaderCell>
                        <span id="span_misccharges" runat="server"></span>
                    </asp:TableHeaderCell>
                    <asp:TableHeaderCell></asp:TableHeaderCell>
                </asp:TableFooterRow>
                <asp:TableFooterRow>
                    <asp:TableHeaderCell>Shipping and Handling charges:</asp:TableHeaderCell>
                    <asp:TableHeaderCell>
                        <span id="span_shippingcharges" runat="server"></span>
                    </asp:TableHeaderCell>
                    <asp:TableHeaderCell></asp:TableHeaderCell>
                </asp:TableFooterRow>
                <asp:TableFooterRow>
                    <asp:TableHeaderCell>Tax charges:</asp:TableHeaderCell>
                    <asp:TableHeaderCell>
                        <span id="span_taxcharges" runat="server"></span>
                    </asp:TableHeaderCell>
                    <asp:TableHeaderCell></asp:TableHeaderCell>
                </asp:TableFooterRow>
                <asp:TableFooterRow>
                    <asp:TableHeaderCell>Total order price:</asp:TableHeaderCell>
                    <asp:TableHeaderCell>
                        <span id="span_totalprice" runat="server"></span>
                    </asp:TableHeaderCell>
                    <asp:TableHeaderCell></asp:TableHeaderCell>
                </asp:TableFooterRow>
            </asp:Table>
        </div>
        <div id="footer" runat="server" class="menu">
            <table border="0px" width="90%">
                <tr>
                    <td>
                        <strong>Requestor's Justification: </strong><span id="span_justification" runat="server"></span>
                    </td>
                </tr>
                <tr>
                    <td>
                        <strong>Requestor Notes: </strong><span id="span_requestornotes" runat="server"></span>
                    </td>
                </tr>
                <tr>
                    <td>
                        <strong>Approver Notes: </strong><span id="span_executornotes" runat="server"></span>
                    </td>
                </tr>
                <tr>
                    <td>
                        <strong>Purchaser Notes: </strong><span id="span_purchasernotes" runat="server"></span>
                    </td>
                </tr>
            </table>
        </div>
        <div id="history" runat="server" class="menu">
        </div>
        <div id="ack_controls" runat="server">
        </div>
    </div>
    </form>
</body>
</html>

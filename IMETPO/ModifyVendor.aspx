<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ModifyVendor.aspx.cs" Inherits="IMETPO.ModifyVendor"
    EnableViewState="true" EnableViewStateMac="true" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>IMET Purchasing System</title>
    <link href="Styles/imetps.css?t=<%= DateTime.Now.Ticks %>" type="text/css" rel="stylesheet" />
</head>
<body>
    <form id="form1" runat="server">
    <div id="headerTitle" class="banner" runat="server">
        <img class="logo" src="images/IMET_logo.png" width="398" height="72" alt="IMET" />
    </div>
    <div id="title" class="title">
        <center><span id="titlespan" runat="server"></span></center>
    </div>
    <div id="mainbody" class="content">
        <div id="backlink">
            <a href="Default.aspx">Return to main menu</a><br />
            <a href="ListVendors.aspx">Return to vendor list</a>
        </div>
        <div id="content" class="menu">
            <h3>
                Edit a Vendor</h3>
            <table border="0" id="tblVendorInfo" runat="server" width="80%">
                <tr><td width="15%"></td><td width="35%"></td><td width="15%"></td><td width="35%"></td></tr>
                <tr>
                    <td colspan="1">
                        Vendor ID:
                    </td>
                    <td colspan="3">
                        <asp:Label ID="lblVendorID" runat="server" Text=""></asp:Label>
                    </td>
                </tr>
                <tr>
                    <td colspan="1">
                        Vendor Name:
                    </td>
                    <td colspan="3">
                        <asp:TextBox ID="txtName" runat="server" Width="100%"></asp:TextBox>
                    </td>
                </tr>
                <tr>
                    <td colspan="1">
                        Vendor Description:
                    </td>
                    <td colspan="3">
                        <asp:TextBox ID="txtDescription" runat="server" Rows="3" TextMode="MultiLine" Width="100%"></asp:TextBox>
                    </td>
                </tr>
                <tr>
                    <td colspan="1">
                        URL:
                    </td>
                    <td colspan="3">
                        <asp:TextBox ID="txtURL" runat="server" Width="100%"></asp:TextBox>
                    </td>
                </tr>
                <tr>
                    <td colspan="1">
                        Address Line 1:
                    </td>
                    <td colspan="3">
                        <asp:TextBox ID="txtAddress1" runat="server" Width="100%"></asp:TextBox>
                    </td>
                </tr>
                <tr>
                    <td colspan="1">
                        Address Line 2:
                    </td>
                    <td colspan="3">
                        <asp:TextBox ID="txtAddress2" runat="server" Width="100%"></asp:TextBox>
                    </td>
                </tr>
                <tr>
                    <td>
                        City:
                    </td>
                    <td>
                        <asp:TextBox ID="txtCity" runat="server" Width="100%"></asp:TextBox>
                    </td>
                    <td>
                        State:
                    </td>
                    <td>
                        <asp:TextBox ID="txtState" runat="server" Width="100%"></asp:TextBox>
                    </td>
                </tr>
                <tr>
                    <td colspan="1">
                        Postal Code:
                    </td>
                    <td colspan="3">
                        <asp:TextBox ID="txtPostalCode" runat="server" Width="100%"></asp:TextBox>
                    </td>
                </tr>
                <tr>
                    <td colspan="1">
                        FEIN:
                    </td>
                    <td colspan="3">
                        <asp:TextBox ID="txtFEIN" runat="server" Width="100%"></asp:TextBox>
                    </td>
                </tr>
                <tr>
                    <td>
                        Phone #:
                    </td>
                    <td>
                        <asp:TextBox ID="txtPhone" runat="server" Width="100%"></asp:TextBox>
                    </td>
                    <td>
                        Fax #:
                    </td>
                    <td>
                        <asp:TextBox ID="txtFax" runat="server" Width="100%"></asp:TextBox>
                    </td>
                </tr>
                <tr>
                    <td>Contact Name:</td>
                    <td>
                        <asp:TextBox ID="txtcontact_name" runat="server" Width="100%"></asp:TextBox></td>
                        <td>Contact Phone:</td>
                        <td>
                            <asp:TextBox ID="txtcontact_phone" runat="server" Width="100%"></asp:TextBox></td>
                </tr>
                <tr>
                    <td>Contact Email:</td>
                    <td>
                        <asp:TextBox ID="txtcontact_email" runat="server" Width="100%"></asp:TextBox></td>
                        <td>Customer Account #:</td>
                        <td>
                            <asp:TextBox ID="txtcustomer_account_number" runat="server" Width="100%"></asp:TextBox></td>
                </tr>
                <tr>
                    <td colspan="4">
                        &nbsp;
                    </td>
                </tr>
            </table>
        </div>
        <div id="controls" class="menu">
            <table border="0px" class="controls">
                <tr>
                    <td>
                        <asp:LinkButton CssClass="squarebutton" ID="btnSaveVendor" runat="server" OnClick="btnSaveVendor_Click"><span>Save Changes</span></asp:LinkButton>
                    </td>
                    <td>
                        <asp:LinkButton CssClass="squarebutton" ID="btnDelete" runat="server" OnClick="btnDelete_Click"><span>Delete this Vendor</span></asp:LinkButton>
                    </td>
                    <td>
                        <asp:LinkButton CssClass="squarebutton" ID="btnNewVendor" runat="server" OnClick="btnNewVendor_Click"><span>New Vendor</span></asp:LinkButton>
                    </td>
                </tr>
            </table>
        </div>
    </div>
    </form>
</body>
</html>

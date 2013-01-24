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
    <div class="top">
        <h1>
            <span id="appTitle" runat="server"></span></h1>
        <p>
            <span id="appSubtitle" runat="server"></span></p>
    </div>
    <div id="mainbody" class="content">
        <div id="backlink">
            <a href="Default.aspx">Return to main menu</a><br />
            <a href="ListVendors.aspx">Return to vendor list</a>
        </div>
        <div id="content" class="menu">
            <h3>
                Edit a Vendor</h3>
            <table border="0" id="tblVendorInfo" runat="server">
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
                        <asp:TextBox ID="txtName" runat="server" Width="320px"></asp:TextBox>
                    </td>
                </tr>
                <tr>
                    <td colspan="1">
                        Vendor Description:
                    </td>
                    <td colspan="3">
                        <asp:TextBox ID="txtDescription" runat="server" Rows="3" TextMode="MultiLine" Width="320px"></asp:TextBox>
                    </td>
                </tr>
                <tr>
                    <td colspan="1">
                        URL:
                    </td>
                    <td colspan="3">
                        <asp:TextBox ID="txtURL" runat="server" Width="320px"></asp:TextBox>
                    </td>
                </tr>
                <tr>
                    <td colspan="1">
                        Address Line 1:
                    </td>
                    <td colspan="3">
                        <asp:TextBox ID="txtAddress1" runat="server" Width="320px"></asp:TextBox>
                    </td>
                </tr>
                <tr>
                    <td colspan="1">
                        Address Line 2:
                    </td>
                    <td colspan="3">
                        <asp:TextBox ID="txtAddress2" runat="server" Width="320px"></asp:TextBox>
                    </td>
                </tr>
                <tr>
                    <td>
                        City:
                    </td>
                    <td>
                        <asp:TextBox ID="txtCity" runat="server" Width="180px"></asp:TextBox>
                    </td>
                    <td>
                        State:
                    </td>
                    <td>
                        <asp:TextBox ID="txtState" runat="server" Width="180px"></asp:TextBox>
                    </td>
                </tr>
                <tr>
                    <td colspan="1">
                        Postal Code:
                    </td>
                    <td colspan="3">
                        <asp:TextBox ID="txtPostalCode" runat="server"></asp:TextBox>
                    </td>
                </tr>
                <tr>
                    <td colspan="1">
                        FEIN:
                    </td>
                    <td colspan="3">
                        <asp:TextBox ID="txtFEIN" runat="server" Width="320px"></asp:TextBox>
                    </td>
                </tr>
                <tr>
                    <td>
                        Phone #:
                    </td>
                    <td>
                        <asp:TextBox ID="txtPhone" runat="server" Width="160px"></asp:TextBox>
                    </td>
                    <td>
                        Fax #:
                    </td>
                    <td>
                        <asp:TextBox ID="txtFax" runat="server" Width="160px"></asp:TextBox>
                    </td>
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
                        <asp:LinkButton ID="btnSaveVendor" runat="server" OnClick="btnSaveVendor_Click">Save Changes</asp:LinkButton>
                    </td>
                    <td>
                        <asp:LinkButton ID="btnClear" runat="server" OnClick="btnClear_Click">Clear all fields and create a new vendor</asp:LinkButton>
                    </td>
                    <td>
                        <asp:LinkButton ID="btnDelete" runat="server" OnClick="btnDelete_Click">Delete this Vendor</asp:LinkButton>
                    </td>
                </tr>
            </table>
        </div>
    </div>
    </form>
</body>
</html>

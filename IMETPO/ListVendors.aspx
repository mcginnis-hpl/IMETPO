<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ListVendors.aspx.cs" Inherits="IMETPO.ListVendors" %>

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
            </div>
            <div id="content" runat="server" class="menu">
                <h3>Vendors</h3>
                <p>Click on the vendor name to edit that vendor.</p>
                <asp:Table ID="tblVendors" runat="server">
                    <asp:TableHeaderRow>
                        <asp:TableHeaderCell>Name</asp:TableHeaderCell>
                        <asp:TableHeaderCell>Description</asp:TableHeaderCell>
                        <asp:TableHeaderCell>URL</asp:TableHeaderCell>
                    </asp:TableHeaderRow>
                </asp:Table>
                <a href="ModifyVendor.aspx" class="squarebutton"><span>Add Vendor</span></a><br />
            </div>
    </div>
    </form>
</body>
</html>

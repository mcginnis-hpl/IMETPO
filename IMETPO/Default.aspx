<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="IMETPO._Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>IMET Purchasing System</title>
    <!-- <link href="Styles/imetps.css?t=<%= DateTime.Now.Ticks %>" type="text/css" rel="stylesheet" /> -->
    <style type="text/css" media="all">
        body
        {
            font: 10pt sans-serif;
            background-color: #D8E2E9;
        }
        table
        {
            font: 10pt sans-serif;
        }
        
        th
        {
            text-align: left;
        }
        
        h1
        {
            text-align: left;
            font-size: 150%;
            font-weight: bold;
        }
        
        .top h1
        {
            text-align: right;
            border-bottom: 2px solid black;
            padding-bottom: 0px;
            font-size: 120%;
            font-weight: normal;
            text-transform: lowercase;
            letter-spacing: 5pt;
        }
        
        .top img
        {
            float: left;
            padding-bottom: 0px;
        }
        
        .top p
        {
            text-align: right;
            margin-top: -10px;
            padding-top: 0px;
            font-size: 90%;
            font-weight: normal;
            text-transform: lowercase;
            letter-spacing: 2pt;
            clear: right;
        }
        
        .menu
        {
            border: 2px solid black;
            padding: 10px 10px 10px 10px;
            font-weight: normal;
            background-color: #FFFFFF;
            margin-top: 5px;
            margin-bottom: 5px;
        }
        
        .content
        {
            padding-left: 10px;
            font-weight: normal;
        }
        
        .mainmenu td
        {
            padding: 5px 0px 5px 0px;
        }
        
        a.squarebutton
        {
            background: transparent url('/images/square-blue-left.gif') no-repeat top left;
            display: block;
            float: left;
            font: normal 12px sans-serif; /* Change 12px as desired */
            line-height: 15px; /* This value + 4px + 4px (top and bottom padding of SPAN) must equal height of button background (default is 23px) */
            height: 23px; /* Height of button background height */
            padding-left: 9px; /* Width of left menu image */
            text-decoration: none;
            text-align: center;
        }
        
        a:link.squarebutton, a:visited.squarebutton, a:active.squarebutton
        {
            color: #494949; /*button text color*/
        }
        
        a.squarebutton span
        {
            background: transparent url('/images/square-blue-right.gif') no-repeat top right;
            display: block;
            padding: 4px 9px 4px 0; /*Set 9px below to match value of 'padding-left' value above*/
        }
        
        a.squarebutton:hover
        {
            /* Hover state CSS */
            background-position: bottom left;
        }
        
        a.squarebutton:hover span
        {
            /* Hover state CSS */
            background-position: bottom right;
            color: black;
        }
    </style>
    <script type="text/javascript" language="javascript">
        function processPasswordKeypress() {
            var KeyID = (window.event) ? event.keyCode : e.keyCode;
            if (KeyID == 13) {
                var el = document.getElementById("hiddenDoLogin");
                if (el) {
                    el.value = "1";
                    form1.submit();
                }
            }
        }
    </script>
</head>
<body>
    <form id="form1" runat="server">
    <div class="top">
        <h1>
            <span id="appTitle" runat="server"></span>
        </h1>
        <p>
            <span id="appSubtitle" runat="server"></span>
        </p>
    </div>
    <div id="login" runat="server" class="menu">
        <h3>
            Existing Users</h3>
        <p>
            Type in your assigned username and password below. If you do not have a username
            and password, <a href='mailto:smcginnis@umces.edu??subject=ESMERALDA Account Request'>
                please click here to request one.</a></p>
        <table border="0px">
            <tr>
                <td>
                    Username or Email Address:
                </td>
                <td>
                    <asp:TextBox ID="txtUsername" runat="server"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <td>
                    Password:
                </td>
                <td>
                    <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" OnKeyPress="processPasswordKeypress()"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <td>
                    Stay logged in:
                </td>
                <td>
                    <asp:CheckBox ID="chkPersistCookie" runat="server" />
                </td>
            </tr>
            <tr>
                <td>
                </td>
                <td align="right">
                    <asp:LinkButton CssClass="squarebutton" ID="btnLogon" runat="server" OnClick="btnLogon_Click"><span>Sign In</span></asp:LinkButton>
                </td>
            </tr>
        </table>
    </div>
    <div id="menu" class="menu" runat="server">
        <h3>
            Please select from the following menu</h3>
        <table border="0px" class="mainmenu">
            <tr id="rowSubmit" runat="server">
                <td>
                    <a href='SubmitRequest.aspx'>Submit a Purchase Request</a>
                </td>
            </tr>
            <tr id="rowModify" runat="server">
                <td>
                    <a href='SearchRequests.aspx?STATUS=0|4&USERONLY=1&USERROLE=0&LINKMODE=edit' id="linkModify"
                        runat="server">Modify an open Purchase Request</a><span id="modifyCount" runat="server"></span>
                </td>
            </tr>
            <tr id="rowReview" runat="server">
                <td>
                    <a href='SearchRequests.aspx?STATUS=0&USERONLY=1&USERROLE=8&LINKMODE=approve' id="linkReview"
                        runat="server">Review pending Purchase Requests</a><span id="reviewCount" runat="server"></span>
                </td>
            </tr>
            <tr id="rowPurchase" runat="server">
                <td>
                    <a href='SearchRequests.aspx?STATUS=2&LINKMODE=purchase' id="linkPurchase" runat="server">
                        Purchase items from a Purchase Request</a><span id="purchaseCount" runat="server"></span>
                </td>
            </tr>
            <tr id="rowUpdate" runat="server">
                <td>
                    <a href='SearchRequests.aspx?STATUS=32&LINKMODE=receive' id="linkUpdate" runat="server">
                        Update an ongoing Purchase Request</a><span id="updateCount" runat="server"></span>
                </td>
            </tr>
            <tr id="rowClose" runat="server">
                <td>
                    <a href='SearchRequests.aspx?STATUS=8&LINKMODE=close' id="linkClose" runat="server">
                        Close out a Purchase Request</a><span id="closeCount" runat="server"></span>
                </td>
            </tr>
            <tr id="rowInventory" runat="server">
                <td>
                    <a href='Inventory.aspx' id="linkInventory" runat="server">Inventory items</a><span
                        id="inventoryCount" runat="server"></span>
                </td>
            </tr>
            <tr id="rowSupervise" runat="server">
                <td>
                    <a href='SearchRequests.aspx?STATUS=0'>Approve Request for any FRS</a>
                </td>
            </tr>
            <tr id="rowFASManage" runat="server">
                <td>
                    <a href='FASNumbers.aspx'>Manage FRS Numbers and Executors</a>
                </td>
            </tr>
            <tr id="rowVendorManage" runat="server">
                <td>
                    <a href='ListVendors.aspx'>Manage Vendor List</a>
                </td>
            </tr>
            <tr id="rowRecover" runat="server">
                <td>
                    <a href='SearchRequests.aspx?STATUS=16' id="linkRecover" runat="server">Recover deleted
                        request</a>
                </td>
            </tr>
            <tr id="rowAdministrate" runat="server">
                <td>
                    <a href='ModUser.aspx'>Administrate IPS</a>
                </td>
            </tr>
            <tr id="rowSearch" runat="server">
                <td>
                    <a href='SearchRequests.aspx'>Search Requests</a>
                </td>
            </tr>
            <tr id="rowModUser" runat="server">
                <td>
                    <a href='ModUser.aspx?SELF=1'>Change your password</a>
                </td>
            </tr>
            <tr id="rowLogout" runat="server">
                <td>
                    <asp:LinkButton ID="btnLogout" runat="server" OnClick="btnLogout_Click">Logout of IPS</asp:LinkButton>
                </td>
            </tr>
        </table>
    </div>
    <div id="hiddenControls">
        <asp:HiddenField ID="hiddenDoLogin" runat="server" />
    </div>
    </form>
</body>
</html>

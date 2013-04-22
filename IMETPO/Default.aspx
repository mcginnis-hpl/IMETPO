<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="IMETPO._Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>IMET Purchasing System</title>
    <link href="Styles/imetps.css?t=<%= DateTime.Now.Ticks %>" type="text/css" rel="stylesheet" />
    
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
    <div id="headerTitle" class="banner" runat="server">
        <img class="logo" src="images/IMET_logo.png" width="398" height="72" alt="IMET" />
                <div class="top">
        <p><span id="versionnumber" runat="server"></span></p>
        </div>

    </div>
    <div id="title" class="title">
        <center><span id="titlespan" runat="server"></span></center>
    </div>
    <div id="login" runat="server" class="menu">
        <h3>
            <span id="greeting" runat="server"></span></h3>
        <p>
            Type in your assigned username and password below. If you do not have a username
            and password, <a href='mailto:smcginnis@umces.edu??subject=IMETPS Account Request'>
                please click here to request one.</a></p>
        <table border="0px" class="loginarea">
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
            <span id="name_greeting" runat="server">Please select from the following menu</span></h3>
        <table border="0px" class="mainmenu">
            <tr id="rowSubmit" runat="server">
                <td>
                    <a href='SubmitRequest.aspx'>Submit a Purchase Request</a><span id="pendingCount" runat="server"></span>
                </td>
            </tr>
            <tr id="rowModify" runat="server">
                <td>
                    <a href='SearchRequests.aspx?STATUS=0|4|2&USERONLY=1&USERROLE=0&LINKMODE=edit' id="linkModify"
                        runat="server">Modify an open Purchase Request</a><span id="modifyCount" runat="server"></span>
                </td>
            </tr>
            <tr id="rowReview" runat="server">
                <td>
                    <a href='SearchRequests.aspx?STATUS=0&USERONLY=1&USERROLE=8&LINKMODE=approve' id="linkReview"
                        runat="server">Approve or Reject Pending Purchase Requests</a><span id="reviewCount" runat="server"></span>
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
                        Receive a Purchase Request</a><span id="updateCount" runat="server"></span>
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
                    <a href='FASNumbers.aspx'>Manage Account Numbers and Executors</a>
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

<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ModUser.aspx.cs" Inherits="IMETPO.ModUser" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>IMET Purchasing System</title>
    <link href="Styles/imetps.css?t=<%= DateTime.Now.Ticks %>" type="text/css" rel="stylesheet" />
    <script language="javascript" type="text/javascript">
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
    <div id="title" class="title">
        <center>
            <span id="titlespan" runat="server"></span>
        </center>
    </div>
    <div id="mainbody" class="content">
        <div id="backlink">
            <a href="Default.aspx">Return to main menu</a>
        </div>
        <div id="adminInfo" runat="server" class="menu">
            <h3>
                System Values</h3>
            <table class="mainmenu">
                <tr>
                    <td>
                        Account Bypass Limit:
                    </td>
                    <td>
                        <asp:TextBox ID="txtAccountBypassLimit" runat="server"></asp:TextBox>
                    </td>
                </tr>
                <tr>
                    <td>
                        Email Nag Cutoff in Days (Approval):
                    </td>
                    <td>
                        <asp:TextBox ID="txtApproverNagFrequency" runat="server"></asp:TextBox>
                    </td>
                </tr>
                <tr>
                    <td>
                        Email Nag Cutoff in Days (Purchase Pending):
                    </td>
                    <td>
                        <asp:TextBox ID="txtPurchaserNagFrequency" runat="server"></asp:TextBox>
                    </td>
                </tr>
                <tr>
                    <td>
                        Email Nag Cutoff in Days (Receiver):
                    </td>
                    <td>
                        <asp:TextBox ID="txtReceiverNagFrequency" runat="server"></asp:TextBox>
                    </td>
                </tr>
                <tr>
                    <td>
                        <asp:LinkButton CssClass="squarebutton" ID="btnUpdateSystemValues" runat="server"
                            OnClick="btnUpdateSystemValues_Click"><span>Update System Values</span></asp:LinkButton>
                    </td>
                    <td>
                    </td>
                </tr>
            </table>
        </div>
        <div id="userInfo" runat="server" class="menu">
            <h3>
                User Information</h3>
            <table class="mainmenu">
                <tr id="rowSelectUser" runat="server">
                    <td>
                        Select a user:
                    </td>
                    <td>
                        <asp:DropDownList ID="comboUsers" runat="server" OnSelectedIndexChanged="comboUsers_SelectedIndexChanged"
                            AutoPostBack="true">
                        </asp:DropDownList>
                    </td>
                </tr>
                <tr id="rowParentUser" runat="server">
                    <td>
                        Parent user:
                    </td>
                    <td>
                        <asp:DropDownList ID="comboParentUser" runat="server">
                        </asp:DropDownList>
                    </td>
                </tr>
                <tr>
                    <td>
                        Username:
                    </td>
                    <td>
                        <asp:TextBox ID="txtUsername" runat="server" Width="320px"></asp:TextBox>
                    </td>
                </tr>
                <tr>
                    <td>
                        Email address:
                    </td>
                    <td>
                        <asp:TextBox ID="txtEmail" runat="server" Width="320px"></asp:TextBox>
                    </td>
                </tr>
                <tr>
                    <td>
                        First name:
                    </td>
                    <td>
                        <asp:TextBox ID="txtFirstName" runat="server" Width="320px"></asp:TextBox>
                    </td>
                </tr>
                <tr>
                    <td>
                        Last name:
                    </td>
                    <td>
                        <asp:TextBox ID="txtLastName" runat="server" Width="320px"></asp:TextBox>
                    </td>
                </tr>
                <tr>
                    <td>
                        Password:
                    </td>
                    <td>
                        <asp:TextBox ID="txtPassword" TextMode="Password" runat="server"></asp:TextBox>
                    </td>
                </tr>
                <tr>
                    <td>
                        Confirm Password:
                    </td>
                    <td>
                        <asp:TextBox ID="txtPasswordConfirm" TextMode="Password" runat="server"></asp:TextBox>
                    </td>
                </tr>
                <tr>
                    <td colspan="2">
                        <asp:CheckBox ID="chkDoNotEmail" runat="server" Text="Do not send this user email notifications." />
                    </td>
                </tr>
                <tr>
                    <td colspan="2">
                        <asp:LinkButton CssClass="squarebutton" ID="btnSubmit" runat="server" OnClick="btnSubmit_Click"><span>Submit</span></asp:LinkButton>
                    </td>
                </tr>
            </table>
            <asp:HiddenField ID="hiddenUserID" runat="server" />
            <asp:HiddenField ID="hiddenScrollPos" runat="server" />
        </div>
        <div id="permissions" runat="server" class="menu">
            <h3>
                User permissions</h3>
            <table border="0px">
                <tr>
                    <td>
                        <asp:ListBox ID="listAvailablePermissions" runat="server"></asp:ListBox>
                    </td>
                    <td>
                        <table border="0px">
                            <tr>
                                <td>
                                    <asp:LinkButton CssClass="squarebutton" ID="btnAddPermission" runat="server" OnClick="btnAddPermission_Click"><span>-></span>
                                    </asp:LinkButton>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    <asp:LinkButton CssClass="squarebutton" ID="btnRemovePermission" runat="server" OnClick="btnRemovePermission_Click"><span><-</span>
                                    </asp:LinkButton>
                                </td>
                            </tr>
                        </table>
                    </td>
                    <td>
                        <asp:ListBox ID="listUserPermissions" runat="server"></asp:ListBox>
                    </td>
                </tr>
            </table>
            <p style="font-style: italic">
                Permissions changes are saved automatically -- you do not need to hit "submit" to
                save your work.</p>
        </div>
    </div>
    </form>
</body>
</html>

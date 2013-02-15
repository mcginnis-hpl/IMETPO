<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ModUser.aspx.cs" Inherits="IMETPO.ModUser" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
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
            <a href="Default.aspx">Return to main menu</a>
        </div>
        <div id="adminInfo" runat="server" class="menu">
            <h3>System Values</h3>
            <table class="mainmenu">
                <tr><td>Fiscal Year:</td><td>
                    <asp:TextBox ID="txtFiscalYear" runat="server"></asp:TextBox></td></tr>
                    <tr><td>
                        <asp:LinkButton CssClass="squarebutton" ID="btnUpdateSystemValues" runat="server" 
                            onclick="btnUpdateSystemValues_Click"><span>Update System Values</span></asp:LinkButton></td><td></td></tr>
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
                        <asp:CheckBox ID="chkDoNotEmail" runat="server" Text="Do not send this user email notifications." /></td>
                </tr>
            </table>
            <asp:LinkButton CssClass="squarebutton" ID="btnSubmit" runat="server" OnClick="btnSubmit_Click"><span>Submit</span></asp:LinkButton>
            <asp:HiddenField ID="hiddenUserID" runat="server" />
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
            <p style="font-style: italic">Permissions changes are saved automatically -- you do not need to hit "submit" to save your work.</p>
        </div>
    </div>
    </form>
</body>
</html>

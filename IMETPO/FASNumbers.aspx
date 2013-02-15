<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="FASNumbers.aspx.cs" Inherits="IMETPO.FASNumbers"
    EnableViewState="true" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>IMET Purchasing System</title>
    <link href="Styles/imetps.css?t=<%= DateTime.Now.Ticks %>" type="text/css" rel="stylesheet" />
    <script language="javascript" type="text/javascript">
        function submitFASNumber(num) {
            var el = document.getElementById("hiddenUpdateNumber");
            el.value = num;
            form1.submit();
        }

        function editPermissions(num) {
            var el = document.getElementById("hiddenPermissions");
            el.value = "NEW:" + num;
            form1.submit();
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
    <div id="mainbody" class="content">
        <div id="backlink">
            <a href="Default.aspx">Return to main menu</a>
        </div>
        <div id="main" class="menu">
            <h3>
                Manage Account Numbers</h3>
            <asp:Table ID="tblFRSNumbers" runat="server">
                <asp:TableHeaderRow>
                    <asp:TableHeaderCell>Account Number</asp:TableHeaderCell>
                    <asp:TableHeaderCell>Description</asp:TableHeaderCell>
                    <asp:TableHeaderCell>Account Owner</asp:TableHeaderCell>
                    <asp:TableHeaderCell>Expire</asp:TableHeaderCell>
                    <asp:TableHeaderCell>Action</asp:TableHeaderCell>
                </asp:TableHeaderRow>
                <asp:TableRow ID="newrow">
                    <asp:TableCell>
                        <asp:TextBox ID="txtFRSNumber_new" runat="server"></asp:TextBox></asp:TableCell>
                    <asp:TableCell>
                        <asp:TextBox ID="txtFRSDesc_new" runat="server"></asp:TextBox></asp:TableCell>
                    <asp:TableCell>
                        <asp:DropDownList ID="comboFRSExecutor_new" runat="server">
                        </asp:DropDownList>
                    </asp:TableCell>
                    <asp:TableCell>
                        <asp:CheckBox ID="chkFRSDisabled_new" runat="server" /></asp:TableCell>
                    <asp:TableCell>
                        <a class="squarebutton" href="javascript:submitFASNumber('new')"><span>Add FRS Number</span></a></asp:TableCell>
                </asp:TableRow>
            </asp:Table>
            <asp:HiddenField ID="hiddenUpdateNumber" runat="server" />
            <asp:HiddenField ID="hiddenPermissions" runat="server" />
            <asp:HiddenField ID="hiddenOwnerid" runat="server" />
        </div>
        <div id="permissions" runat="server" class="menu">
            <table border="0px">
                <tr>
                    <th>
                        Available Requestors
                    </th>
                    <th>
                    </th>
                    <th>
                        Authorized Requestors
                    </th>
                    <th>
                    </th>
                    <th>
                        Authorized Approvers
                    </th>
                </tr>
                <tr>
                    <td>
                        <asp:ListBox ID="listAvailableRequestors" runat="server"></asp:ListBox>
                    </td>
                    <td>
                        <table border="0">
                        <tr><td><asp:LinkButton CssClass="squarebutton" ID="btnAddRequestor" runat="server" OnClick="btnAddRequestor_Click" Width="140"><span>Add Requestor-></span></asp:LinkButton></td></tr>
                        <tr><td><asp:LinkButton CssClass="squarebutton" ID="btnRemoveRequestor" runat="server" OnClick="btnRemoveRequestor_Click" Width="140"><span><- Remove Requestor</span></asp:LinkButton></td></tr>
                        </table>
                    </td>
                    <td>
                        <asp:ListBox ID="listRequestors" runat="server"></asp:ListBox>
                    </td>
                    <td>
                        <table border="0">
                        <tr><td><asp:LinkButton CssClass="squarebutton" ID="btnAddApprover" runat="server" OnClick="btnAddApprover_Click" Width="140"><span>Add Approver-></span></asp:LinkButton></td></tr>
                        <tr><td><asp:LinkButton CssClass="squarebutton" ID="btnRemoveApprover" runat="server" OnClick="btnRemoveApprover_Click" Width="140"><span><- Remove Approver</span></asp:LinkButton></td></tr>
                        </table>                        
                    </td>
                    <td>
                        <asp:ListBox ID="listApprovers" runat="server"></asp:ListBox>
                    </td>
                </tr>
            </table>
            <asp:LinkButton CssClass="squarebutton" ID="btnSavePermissionsChanges" runat="server" OnClick="btnSavePermissionsChanges_Click"><span>Save Changes</span></asp:LinkButton><br />
        </div>
    </div>
    </form>
</body>
</html>

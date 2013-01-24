<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SearchRequests.aspx.cs" Inherits="IMETPO.SearchRequests" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>IMET Purchasing System</title>
    <link href="Styles/imetps.css?t=<%= DateTime.Now.Ticks %>" type="text/css" rel="stylesheet" />
    <link href="table.css?t=<%= DateTime.Now.Ticks %>" type="text/css" rel="stylesheet" />
    <script src="Scripts/table.js" type="text/javascript"></script>
    <script type="text/javascript">
        function undelete(requestid) {
            var control = document.getElementById('hiddenUndelete');
            control.value = requestid;
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
        <div id="filters" runat="server" class="menu">
            <h3>Filter by...</h3>
            <table border="0" class="filters">
                <tr>
                    <td>Status: 
                        <asp:DropDownList ID="comboFilterStatus" runat="server">
                            <asp:ListItem Value="-1" Text="All"></asp:ListItem>
                            <asp:ListItem Value="0" Text="Pending"></asp:ListItem>
                            <asp:ListItem Value="2" Text="Approved"></asp:ListItem>
                            <asp:ListItem Value="4" Text="Rejected"></asp:ListItem>
                            <asp:ListItem Value="8" Text="Received"></asp:ListItem>
                            <asp:ListItem Value="32" Text="Purchased"></asp:ListItem>
                            <asp:ListItem Value="16" Text="Deleted"></asp:ListItem>
                        </asp:DropDownList>
                    </td>
                    <td>Requestor: 
                        <asp:DropDownList ID="comboRequestor" runat="server">
                        </asp:DropDownList>
                    </td>
                    <td>Date: 
                        <asp:TextBox ID="txtStartDate" runat="server"></asp:TextBox> to 
                        <asp:TextBox ID="txtEndDate" runat="server"></asp:TextBox></td>
                </tr>
            </table>
            <br />
            <asp:LinkButton ID="btnSearch" runat="server" onclick="btnSearch_Click">Search</asp:LinkButton>
        </div>
        <div id="resultspane" class="menu">
            <h3>Results</h3>
            <div id="results" runat="server">            
            </div>
        </div>
        <asp:HiddenField ID="hiddenUndelete" runat="server" />
    </div>
    </form>
</body>
</html>

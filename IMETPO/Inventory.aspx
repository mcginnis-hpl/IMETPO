<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Inventory.aspx.cs" Inherits="IMETPO.Inventory" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>IMET Purchasing System</title>
    <link href="Styles/imetps.css?t=<%= DateTime.Now.Ticks %>" type="text/css" rel="stylesheet" />
    <link href="table.css?t=<%= DateTime.Now.Ticks %>" type="text/css" rel="stylesheet" />
    <script src="Scripts/table.js" type="text/javascript"></script>
    <script type="text/javascript">
        function FlagAsInventoried(lineitemid) {
            var control = document.getElementById('hiddenInventoried');
            control.value = lineitemid;
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
            <h5>
                Filter by...</h5>
            <table border="0" class="filters">
                <tr>
                    <td>
                        <asp:CheckBox ID="chkIMETInventory" runat="server" Text="IMET Inventory" 
                            AutoPostBack="true" oncheckedchanged="chkIMETInventory_CheckedChanged" />
                    </td>
                    <td>
                        <asp:CheckBox ID="chkMDInventory" runat="server" Text="MD Inventory" 
                            AutoPostBack="true" oncheckedchanged="chkMDInventory_CheckedChanged" />
                    </td>
                </tr>
            </table>
        </div>        
        <div id="resultsPane" class="menu">
        <h5>Results</h5>
        <div id="results" runat="server">            
        </div>
        </div>
        <asp:HiddenField ID="hiddenInventoried" runat="server" />
    </div>
    </form>
</body>
</html>

using System;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.Security;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Collections;
using System.Collections.Generic;
using IMETPOClasses;

namespace IMETPO
{
    public partial class _Default : imetspage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (hiddenDoLogin.Value != null)
            {
                if (hiddenDoLogin.Value == "1")
                {
                    Login();
                }
            }
            hiddenDoLogin.Value = string.Empty;
            PopulateHeader(appTitle, appSubtitle);
            if (!IsPostBack)
            {
                PopulateMenu(false, false);
            }
        }

        protected void PopulateMenu(bool forceNotAuthenticated, bool forceAuthenticated)
        {
            // If the user is not logged in, only show them the login screen.
            if ((!IsAuthenticated && !forceAuthenticated) || forceNotAuthenticated)
            {
                login.Visible = true;
                menu.Visible = false;
                return;
            }
            else
            {
                login.Visible = false;
                menu.Visible = true;
            }
            // From here down, the menu is populated based on the user's roles, and the current state of the system
            SqlConnection conn = ConnectToConfigString("imetpsconnection");
            try {                
                int pendingcount = 0;
                int rejectedcount = 0;
                int receivedcount = 0;
                // This query get the count of all requests for which the user is the requestor and groups them by state
                string query = "SELECT COUNT(*) AS c, state FROM v_requests_with_executor WHERE userid='" + CurrentUser.userid.ToString() + "' AND permission=0 GROUP BY state";
                SqlCommand cmd = new SqlCommand()
                {
                    CommandType = CommandType.Text,
                    CommandText = query,
                    Connection = conn
                };
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    PurchaseRequest.RequestState state = (PurchaseRequest.RequestState)int.Parse(reader["state"].ToString());
                    if (state == PurchaseRequest.RequestState.pending)
                        pendingcount = int.Parse(reader["c"].ToString());
                    else if (state == PurchaseRequest.RequestState.rejected)
                        rejectedcount = int.Parse(reader["c"].ToString());                    
                }
                reader.Close();
                string colortext = string.Empty;
                if (pendingcount > 0 || rejectedcount > 0)
                {
                    rowModify.Visible = true;
                    if (pendingcount > 0)
                    {
                        colortext += "<br/><font size='-1'><font color='red'>" + pendingcount.ToString() + "</font> requests awaiting approval</font>";
                    }
                    if (rejectedcount > 0)
                    {
                        colortext += "<br/><font size='-1'><font color='red'>" + rejectedcount.ToString() + "</font> rejected requests</font>";
                    }
                    modifyCount.InnerHtml = colortext;
                }
                else
                {
                    rowModify.Visible = false;
                }            
    
                if (CurrentUser.UserPermissions.Contains(IMETPOClasses.User.Permission.purchaser))
                {
                    int approvedcount = 0;
                    int purchasedcount = 0;
                    query = "SELECT COUNT(*) AS c, state FROM requests GROUP BY state";
                    cmd = new SqlCommand()
                    {
                        CommandType = CommandType.Text,
                        CommandText = query,
                        Connection = conn
                    };
                    reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        PurchaseRequest.RequestState state = (PurchaseRequest.RequestState)int.Parse(reader["state"].ToString());
                        if (state == PurchaseRequest.RequestState.approved)
                            approvedcount = int.Parse(reader["c"].ToString());
                        else if (state == PurchaseRequest.RequestState.purchased)
                            purchasedcount = int.Parse(reader["c"].ToString());
                        else if (state == PurchaseRequest.RequestState.received)
                            receivedcount = int.Parse(reader["c"].ToString());
                    }
                    reader.Close();
                    if (approvedcount > 0)
                    {
                        linkPurchase.Visible = true;
                        colortext = "<br/><font size='-1'><font color='red'>" + approvedcount.ToString() + "</font> requests awaiting purchase</font>";
                        purchaseCount.InnerHtml = colortext;
                    }
                    else
                    {
                        linkPurchase.Visible = false;
                        purchaseCount.InnerHtml = "No requests awaiting purchase";
                    }

                    if (purchasedcount > 0)
                    {
                        linkUpdate.Visible = true;
                        colortext = "<br/><font size='-1'><font color='red'>" + purchasedcount.ToString() + "</font> requests purchased</font>";
                        updateCount.InnerHtml = colortext;
                    }
                    else
                    {
                        linkUpdate.Visible = false;
                        updateCount.InnerHtml = "No requests purchased";
                    }

                    if (receivedcount > 0)
                    {
                        linkClose.Visible = true;
                        colortext = "<br/><font size='-1'><font color='red'>" + receivedcount.ToString() + "</font> requests awaiting closure</font>";
                        closeCount.InnerHtml = colortext;
                    }
                    else
                    {
                        linkClose.Visible = false;
                        closeCount.InnerHtml = "No requests awaiting closure";
                    }
                }
                else
                {
                    rowUpdate.Visible = false;
                    rowPurchase.Visible = false;
                    rowClose.Visible = false;
                }

                colortext = string.Empty;
                query = "SELECT COUNT(*) AS c, fasnumber, description FROM v_requests_with_executor WHERE userid='" + CurrentUser.userid.ToString() + "' AND permission=8 AND state=0 GROUP BY fasnumber, description";
                cmd = new SqlCommand()
                {
                    CommandType = CommandType.Text,
                    CommandText = query,
                    Connection = conn
                };
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    colortext += "<br /><font size=\"-1\"><font color=\"red\">" + reader["c"].ToString() + "</font> Requests pending for " + reader["fasnumber"].ToString() + " (" + reader["description"].ToString() + ")</font>";
                }
                reader.Close();

                if (!string.IsNullOrEmpty(colortext))
                {
                    linkReview.Visible = true;
                    reviewCount.InnerHtml = colortext;
                }
                else
                {
                    linkReview.Visible = false;
                    reviewCount.InnerHtml = "No Purchase Requests pending";
                }

                if (CurrentUser.UserPermissions.Contains(IMETPOClasses.User.Permission.inventory))
                {
                    int inventorycount = 0;
                    query = "SELECT COUNT(*) AS c FROM lineitems WHERE state=" + ((int)LineItem.LineItemState.inventory).ToString();
                    cmd = new SqlCommand()
                    {
                        CommandType = CommandType.Text,
                        CommandText = query,
                        Connection = conn
                    };
                    reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        inventorycount = int.Parse(reader["c"].ToString());
                    }
                    reader.Close();
                    if (inventorycount > 0)
                    {
                        colortext = "<br/><font size=\"-1\"><font color=\"red\">" + inventorycount.ToString() + "</font> items need to be inventoried</font>";
                        linkInventory.Visible = true;
                        inventoryCount.InnerHtml = colortext;
                    }
                    else
                    {
                        linkInventory.Visible = false;
                        inventoryCount.InnerHtml = "No items pending inventory";
                    }
                }
                if (CurrentUser.UserPermissions.Contains(IMETPOClasses.User.Permission.globalapprover))
                {
                    rowSupervise.Visible = true;
                }
                else
                {
                    rowSupervise.Visible = false;
                }
                if (CurrentUser.UserPermissions.Contains(IMETPOClasses.User.Permission.admin))
                {                    
                    rowFASManage.Visible = true;
                    rowRecover.Visible = true;
                    rowVendorManage.Visible = true;
                }
                else
                {                    
                    rowFASManage.Visible = false;
                    rowRecover.Visible = false;
                    rowVendorManage.Visible = false;
                }
            }
            catch(Exception ex)
            {
                HandleError(ex);
            }
            finally
            {
                if(conn != null)
                    conn.Close();
            }            
        }

        protected void btnLogon_Click(object sender, EventArgs e)
        {
            this.Login();
        }

        protected void Login()
        {
            bool passwordMatch = false;
            bool forceAuthentication = false;
            if (!IsAuthenticated)
            {
                SqlConnection conn = base.ConnectToConfigString("imetpsconnection");
                IMETPOClasses.User working = new IMETPOClasses.User();
                try
                {
                    working.LoadByUsername(conn, txtUsername.Text);
                    string dbPasswordHash = working.password.ToString();
                    int saltSize = 5;
                    string fakeSaltString = imetspage.CreateSalt(saltSize);
                    string salt = dbPasswordHash.Substring(dbPasswordHash.Length - fakeSaltString.Length);
                    passwordMatch = imetspage.CreatePasswordHash(this.txtPassword.Text, salt).Equals(dbPasswordHash);
                    if (passwordMatch)
                    {
                        FormsAuthenticationTicket tkt = new FormsAuthenticationTicket(1, working.username, DateTime.Now, DateTime.Now.AddYears(1), this.chkPersistCookie.Checked, "");
                        string cookiestr = FormsAuthentication.Encrypt(tkt);
                        HttpCookie ck = new HttpCookie(FormsAuthentication.FormsCookieName, cookiestr);
                        if (this.chkPersistCookie.Checked)
                        {
                            ck.Expires = tkt.Expiration;
                        }
                        ck.Path = FormsAuthentication.FormsCookiePath;
                        base.Response.Cookies.Add(ck);
                        this.login.Visible = false;
                        this.menu.Visible = true;
                        forceAuthentication = true;
                        SetSessionValue("currentuser", working);
                    }
                    else
                    {
                        base.ShowAlert("Login incorrect.  Please try again, or create a new user account.");
                    }

                }
                catch (Exception ex)
                {
                    HandleError(ex);
                }
                finally
                {
                    conn.Close();
                }
            }
            for (int i = 0; i < Request.Params.Count; i++)
            {
                if (Request.Params.GetKey(i) == "RETURNURL")
                {
                    string url = Request.Params[i];
                    Response.Redirect(url, false);
                    return;
                }
            }
            PopulateMenu(false, forceAuthentication);
        }

        protected void btnLogout_Click(object sender, EventArgs e)
        {
            try
            {
                Logout();
                PopulateMenu(true, false);
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }
    }
}

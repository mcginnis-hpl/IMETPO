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
using System.Reflection;

namespace IMETPO
{
    /// <summary>
    /// The landing page for IMETPS, which is the main menu and the login screen. There's a lot going on on this page.
    /// </summary>
    public partial class _Default : imetspage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (hiddenDoLogin.Value != null)
            {
                if (hiddenDoLogin.Value == "1")
                {
                    Login();
                    return;
                }
            }
            this.versionnumber.InnerHtml = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            PopulateHeader(titlespan);
            if (CurrentUser != null && !string.IsNullOrEmpty(CurrentUser.firstname))
            {
                name_greeting.InnerHtml = "Welcome, " + CurrentUser.firstname + ".  Please select from the following menu";
            }
            else
            {
                name_greeting.InnerHtml = "Please select from the following menu";
            }
            hiddenDoLogin.Value = string.Empty;            
            greeting.InnerHtml = GetApplicationSetting("applicationGreeting");
            // The below is an attempt to fix a bug that seemed to be keeping the updated status of the system from showing up.
            if (!IsPostBack)
            {
                Response.Buffer = true;
                Response.CacheControl = "no-cache";
                Response.AddHeader("Pragma", "no-cache");
                Response.Expires = -1441;                
            }
            PopulateMenu(false, false, CurrentUser);
        }

        protected void PopulateMenu(bool forceNotAuthenticated, bool forceAuthenticated, User theUser)
        {
            // If the user is not logged in, only show them the login screen.
            if ((!IsAuthenticated && !forceAuthenticated) || forceNotAuthenticated)
            {
                login.Visible = true;
                menu.Visible = false;
                // headerTitle.Visible = false;
                return;
            }
            else
            {
                login.Visible = false;
                menu.Visible = true;
                // headerTitle.Visible = true;
            }
            // From here down, the menu is populated based on the user's roles, and the current state of the system
            SqlConnection conn = ConnectToConfigString("imetpsconnection");
            try {                
                int pendingcount = 0;
                int rejectedcount = 0;
                int approvedcount = 0;
                int purchasedcount = 0;

                // This query get the count of all requests for which the user is the requestor and groups them by state
                string query = "SELECT COUNT(*) AS c, state FROM v_requests_with_executor WHERE userid='" + theUser.userid.ToString() + "' AND permission=0 GROUP BY state";
                SqlCommand cmd = new SqlCommand()
                {
                    CommandType = CommandType.Text,
                    CommandText = query,
                    Connection = conn
                };
                string colortext = string.Empty;
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    PurchaseRequest.RequestState state = (PurchaseRequest.RequestState)int.Parse(reader["state"].ToString());
                    if (state == PurchaseRequest.RequestState.pending)
                        pendingcount = int.Parse(reader["c"].ToString());
                    else if (state == PurchaseRequest.RequestState.rejected)
                        rejectedcount = int.Parse(reader["c"].ToString());
                    else if (state == PurchaseRequest.RequestState.approved)
                        approvedcount = int.Parse(reader["c"].ToString());
                    else if (state == PurchaseRequest.RequestState.purchased)
                        purchasedcount = int.Parse(reader["c"].ToString());
                }
                reader.Close();
                // Update the text alerts with the numbers that were just loaded.
                if (pendingcount > 0 || approvedcount > 0)
                {
                    if (pendingcount > 0)
                    {
                        colortext += "<br/><font size='-1'><font color='red'>" + pendingcount.ToString() + "</font> requests awaiting approval</font>";
                    }
                    if (approvedcount > 0)
                    {
                        colortext += "<br/><font size='-1'><font color='red'>" + approvedcount.ToString() + "</font> requests awaiting purchase</font>";
                    }
                    pendingCount.InnerHtml = colortext;
                }
                colortext = string.Empty;
                if (pendingcount > 0 || rejectedcount > 0 || approvedcount > 0)
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
                    if (approvedcount > 0)
                    {
                        colortext += "<br/><font size='-1'><font color='red'>" + approvedcount.ToString() + "</font> requests awaiting purchase</font>";
                    }
                    modifyCount.InnerHtml = colortext;
                }
                else
                {
                    rowModify.Visible = false;
                }
                colortext = string.Empty;
                if (purchasedcount > 0)
                {
                    rowUpdate.Visible = true;
                    colortext += "<br/><font size='-1'><font color='red'>" + purchasedcount.ToString() + "</font> requests awaiting receipt</font>";
                    updateCount.InnerHtml = colortext;
                }
                else
                {
                    rowUpdate.Visible = false;
                }
                // Special purchaser functionlity
                if (theUser.UserPermissions.Contains(IMETPOClasses.User.Permission.purchaser))
                {
                    approvedcount = 0;
                    purchasedcount = 0;
                    // Do the same thing as above, for purchased requests
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
                    }
                    reader.Close();
                    // Update the text indicators with the values loaded above.
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
                        colortext = "<br/><font size='-1'><font color='red'>" + purchasedcount.ToString() + "</font> awaiting receipt</font>";
                        updateCount.InnerHtml = colortext;
                    }
                    else
                    {
                        linkUpdate.Visible = false;
                        updateCount.InnerHtml = "No requests awaiting receipt";
                    }
                    rowPurchase.Visible = true;
                }
                else
                {                    
                    rowPurchase.Visible = false;
                }

                // The number of pending requests broken out by FASNumber for approval.
                colortext = string.Empty;
                query = "SELECT COUNT(*) AS c, fasnumber, description FROM v_requests_with_executor WHERE userid='" + theUser.userid.ToString() + "' AND permission=8 AND state=0 GROUP BY fasnumber, description";
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
                    rowReview.Visible = true;
                }
                else
                {
                    linkReview.Visible = false;
                    rowReview.Visible = false;
                    // reviewCount.InnerHtml = "No Purchase Requests pending your approval";
                }

                // Special inventory functionality
                if (theUser.UserPermissions.Contains(IMETPOClasses.User.Permission.inventory))
                {
                    int inventorycount = 0;
                    // Get a count of all line items that are flagged to be inventoried.
                    query = "SELECT COUNT(*) AS c FROM v_inventory_items WHERE inventoryimet=1 OR inventorymd=1";
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
                    // Update the indicator text with those numbers.
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
                    rowInventory.Visible = true;
                }
                else
                {
                    rowInventory.Visible = false;
                }
                // Global Approver command hide/unhide.
                if (theUser.UserPermissions.Contains(IMETPOClasses.User.Permission.globalapprover))
                {
                    rowSupervise.Visible = true;
                }
                else
                {
                    rowSupervise.Visible = false;
                }
                // Vendor management row hide/unhide
                if (theUser.UserPermissions.Contains(IMETPOClasses.User.Permission.admin) || theUser.UserPermissions.Contains(IMETPOClasses.User.Permission.purchaser))
                {
                    rowVendorManage.Visible = true;
                }
                else
                {
                    rowVendorManage.Visible = false;
                }
                // Admin row hide/unhide
                if (theUser.UserPermissions.Contains(IMETPOClasses.User.Permission.admin))
                {                    
                    rowFASManage.Visible = true;
                    rowRecover.Visible = true;
                    rowAdministrate.Visible = true;
                }
                else
                {                    
                    rowFASManage.Visible = false;
                    rowRecover.Visible = false;
                    rowAdministrate.Visible = false;
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

        // This function attempts to log the current user in with the entered username and password.
        protected void Login()
        {
            bool passwordMatch = false;
            bool forceAuthentication = false;
            IMETPOClasses.User working = null;

            if (!IsAuthenticated)
            {
                SqlConnection conn = base.ConnectToConfigString("imetpsconnection");
                working = new IMETPOClasses.User();
                try
                {
                    // Load the user object.
                    working.LoadByUsername(conn, txtUsername.Text);
                    if (string.IsNullOrEmpty(working.username) || working.userid == Guid.Empty)
                    {
                        ShowAlert("No user with that username could be found.  Please check the username entered, or use the link above to request an account.");
                        return;
                    }
                    string dbPasswordHash = working.password.ToString();
                    int saltSize = 5;
                    string fakeSaltString = imetspage.CreateSalt(saltSize);
                    string salt = dbPasswordHash.Substring(dbPasswordHash.Length - fakeSaltString.Length);
                    // Create a salted/hashed version of the password and compare them.
                    passwordMatch = imetspage.CreatePasswordHash(this.txtPassword.Text, salt).Equals(dbPasswordHash);
                    if (passwordMatch)
                    {
                        // Create an authentication cookie for this user.
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
                    HandleError(ex, "User: " + txtUsername.Text);
                }
                finally
                {
                    if(conn != null)
                        conn.Close();
                }
            }
            // Bounce the user back from whence they came.
            for (int i = 0; i < Request.Params.Count; i++)
            {
                if (Request.Params.GetKey(i) == "RETURNURL")
                {
                    string url = Request.Params[i];
                    Response.Redirect(url, false);
                    return;
                }
            }
            if(working == null)
                PopulateMenu(false, forceAuthentication, CurrentUser);
            else
                PopulateMenu(false, forceAuthentication, working);
        }

        protected void btnLogout_Click(object sender, EventArgs e)
        {
            try
            {
                Logout();
                PopulateMenu(true, false, CurrentUser);
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }
    }
}

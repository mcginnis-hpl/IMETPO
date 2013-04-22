using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using IMETPOClasses;
using System.Data;
using System.Data.SqlClient;

namespace IMETPO
{
    /// <summary>
    /// This class just lists all of the vendors, with a click-through to actually edit each vendor.
    /// </summary>
    public partial class ListVendors : imetspage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsAuthenticated)
            {
                string url = "Default.aspx?RETURNURL=" + Request.Url.ToString();
                Response.Redirect(url, false);
                return;
            }
            try
            {
                PopulateHeader(titlespan);
                if (!IsPostBack)
                {
                    SqlConnection conn = ConnectToConfigString("imetpsconnection");
                    PopulateData(conn);
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }
        
        /// <summary>
        /// Populate the list.  This is dead simple.
        /// </summary>
        /// <param name="conn">An open connection to the IMETPS database.</param>
        protected void PopulateData(SqlConnection conn)
        {
            // Check permissions; show nothing if the user does not have Admin or Purchaser permissions.
            if (!CurrentUser.UserPermissions.Contains(IMETPOClasses.User.Permission.admin) && !CurrentUser.UserPermissions.Contains(IMETPOClasses.User.Permission.purchaser))
            {
                content.Visible = false;
                return;
            }
            try
            {
                // Load a list of all vendors.
                List<Vendor> vendors = Vendor.LoadAllVendors(conn);
                foreach (Vendor v in vendors)
                {
                    // For each vendor, add a row to the list of vendors
                    TableRow tr = new TableRow();
                    TableCell td = new TableCell();
                    // Relevant bit: the link to actually edit the vendor.
                    td.Text = "<a href='ModifyVendor.aspx?VENDORID=" + v.vendorid.ToString() + "'>" + v.vendorname + "</a>";
                    tr.Cells.Add(td);

                    td = new TableCell();
                    td.Text = v.description;
                    tr.Cells.Add(td);

                    td = new TableCell();
                    if (!string.IsNullOrEmpty(v.url))
                    {
                        td.Text = "<a href='" + v.url + "' target='_blank'>Link</a>";
                    }
                    else
                    {
                        td.Text = string.Empty;
                    }
                    tr.Cells.Add(td);

                    tblVendors.Rows.Add(tr);
                }
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }
    }
}
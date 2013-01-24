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
    public partial class ListVendors : imetspage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsAuthenticated)
            {
                string url = "Default.aspx?RETURNURL=" + Request.Url.ToString();
                Response.Redirect(url);
                return;
            }
            try
            {
                PopulateHeader(appTitle, appSubtitle);
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

        protected void PopulateData(SqlConnection conn)
        {
            if (!UserIsAdministrator)
            {
                content.Visible = false;
                return;
            }
            try
            {
                List<Vendor> vendors = Vendor.LoadAllVendors(conn);
                foreach (Vendor v in vendors)
                {
                    TableRow tr = new TableRow();
                    TableCell td = new TableCell();
                    td.Text = "<a href='ModifyVendor.aspx?VENDORID=" + v.vendorid.ToString() + "'>" + v.vendorname + "</a>";
                    tr.Cells.Add(td);

                    td = new TableCell();
                    td.Text = v.description;
                    tr.Cells.Add(td);

                    td = new TableCell();
                    if (!string.IsNullOrEmpty(v.url))
                    {
                        td.Text = "<a href='" + v.url + "'>Link</a>";
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
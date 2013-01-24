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
    public partial class ModifyVendor : imetspage
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
                    RemoveSessionValue("WorkingVendor");
                    Guid vendorid = Guid.Empty;
                    if (!UserIsAdministrator)
                    {
                        tblVendorInfo.Visible = false;
                        return;
                    }
                    for (int i = 0; i < Request.Params.Count; i++)
                    {
                        if (Request.Params.GetKey(i).ToUpper() == "VENDORID")
                        {
                            vendorid = new Guid(Request.Params[i]);
                            break;
                        }
                    }
                    if (vendorid == Guid.Empty)
                    {
                        Vendor v = new Vendor();
                        SetSessionValue("WorkingVendor", v);
                    }
                    else
                    {
                        Vendor v = new Vendor();
                        SqlConnection conn = ConnectToConfigString("imetpsconnection");
                        v.Load(conn, vendorid);
                        PopulateData(v);
                        SetSessionValue("WorkingVendor", v);
                        conn.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        protected void PopulateData(Vendor v)
        {
            txtAddress1.Text = v.address1;
            txtAddress2.Text = v.address2;
            txtCity.Text = v.city;
            txtDescription.Text = v.description;
            txtFax.Text = v.fax;
            txtFEIN.Text = v.fein;
            txtName.Text = v.vendorname;
            txtPhone.Text = v.phone;
            txtPostalCode.Text = v.zip;
            txtState.Text = v.st;
            txtURL.Text = v.url;
            if (v.vendorid == Guid.Empty)
                lblVendorID.Text = string.Empty;
            else
                lblVendorID.Text = v.vendorid.ToString();
        }

        protected void btnSaveVendor_Click(object sender, EventArgs e)
        {
            try
            {
                Vendor v = (Vendor)GetSessionValue("WorkingVendor");
                if (string.IsNullOrEmpty(txtName.Text))
                {
                    ShowAlert("The vendor must have a name.");
                    return;
                }
                v.vendorname = txtName.Text;
                v.address1 = txtAddress1.Text;
                v.address2 = txtAddress2.Text;
                v.city = txtCity.Text;
                v.description = txtDescription.Text;
                v.fax = txtFax.Text;
                v.fein = txtFEIN.Text;
                v.phone = txtPhone.Text;
                v.st = txtState.Text;
                v.url = txtURL.Text;
                SqlConnection conn = ConnectToConfigString("imetpsconnection");
                v.Save(conn);
                conn.Close();
                ShowAlert("Vendor saved.");
                SetSessionValue("WorkingVendor", v);
                PopulateData(v);
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            try
            {
                Vendor v = new Vendor();
                SetSessionValue("WorkingVendor", v);
                PopulateData(v);
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                Vendor v = (Vendor)GetSessionValue("WorkingVendor");
                v.state = Vendor.VendorState.Deleted;
                SqlConnection conn = ConnectToConfigString("imetpsconnection");
                v.Save(conn);
                conn.Close();
                v = new Vendor();
                SetSessionValue("WorkingVendor", v);
                PopulateData(v);
                ShowAlert("Vendor deleted.");
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }
    }
}
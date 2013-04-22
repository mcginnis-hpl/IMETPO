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
    /// This class encapsulates the page used to edit the details of a vendor.  It is pretty straghtforward, as its just a bunch of text fields that dump back to the underlying object.
    /// </summary>
    public partial class ModifyVendor : imetspage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // If the user is not authenticated, redirect to the main page.
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
                    RemoveSessionValue("WorkingVendor");
                    Guid vendorid = Guid.Empty;
                    // Hide everything if the user does not have permission to view vendors.
                    if (!CurrentUser.UserPermissions.Contains(IMETPOClasses.User.Permission.admin) && !CurrentUser.UserPermissions.Contains(IMETPOClasses.User.Permission.purchaser))
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

        // Place the details of the vendor in the text fields on this page.
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
            txtcontact_email.Text = v.contact_email;
            txtcontact_name.Text = v.contact_name;
            txtcontact_phone.Text = v.contact_phone;
            txtcustomer_account_number.Text = v.customer_account_number;

            if (v.vendorid == Guid.Empty)
                lblVendorID.Text = string.Empty;
            else
                lblVendorID.Text = v.vendorid.ToString();
        }

        // Clear out the current vendor, create a new vendor, and repopulate the fields.
        protected void btnNewVendor_Click(object sender, EventArgs e)
        {
            PopulateHeader(titlespan);
            RemoveSessionValue("WorkingVendor");
            if (!CurrentUser.UserPermissions.Contains(IMETPOClasses.User.Permission.admin) && !CurrentUser.UserPermissions.Contains(IMETPOClasses.User.Permission.purchaser))
            {
                tblVendorInfo.Visible = false;
                return;
            }
            Vendor v = new Vendor();
            SetSessionValue("WorkingVendor", v);
            PopulateData(v);
        }

        // Save the current vendor.
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
                v.zip = txtPostalCode.Text;
                v.contact_name = txtcontact_name.Text;
                v.contact_email = txtcontact_email.Text;
                v.contact_phone = txtcontact_phone.Text;
                v.customer_account_number = txtcustomer_account_number.Text;
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

        // Delete a vendor.
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
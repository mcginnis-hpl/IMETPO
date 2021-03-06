﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;
using IMETPOClasses;

namespace IMETPO
{
    /// <summary>
    /// Modify a user's details.  This page is used to create users, assign permissions, change passwords and so on.
    /// </summary>
    public partial class ModUser : imetspage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                // If not authenticated, bounce the user back to login.
                if (!IsAuthenticated)
                {
                    string url = "Default.aspx?RETURNURL=" + Request.Url.ToString();
                    Response.Redirect(url, false);
                    return;
                }
                else
                {
                    PopulateHeader(titlespan);
                    userInfo.Visible = true;
                    permissions.Visible = true;
                    if (!IsPostBack)
                    {
                        RemoveSessionValue("WorkingUser");
                        if (IsSelf())
                        {
                            SetSessionValue("WorkingUser", CurrentUser);
                            PopulateData(CurrentUser, IsSelf());
                        }
                        else
                        {
                            PopulateAdminData();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        /// <summary>
        /// Return true if the user got here via the "Change Password" link, false otherwise.
        /// </summary>
        /// <returns></returns>
        protected bool IsSelf()
        {
            bool isSelf = false;
            for (int i = 0; i < Request.Params.Count; i++)
            {
                if (Request.Params.GetKey(i) == "SELF")
                {
                    if (Request.Params["SELF"] == "1")
                    {
                        isSelf = true;
                    }
                }
            }
            return isSelf;
        }

        // Place the user data in the text fields
        protected void PopulateData(User inUser, bool isSelf)
        {
            if (inUser == null)
            {
                txtEmail.Text = string.Empty;
                txtUsername.Text = string.Empty;
                txtPassword.Text = string.Empty;
                txtPasswordConfirm.Text = string.Empty;
                txtFirstName.Text = string.Empty;
                txtLastName.Text = string.Empty;
            }
            else
            {
                txtEmail.Text = inUser.email;
                txtUsername.Text = inUser.username;
                int saltSize = 5;
                string fakeSaltString = CreateSalt(saltSize);
                string not_salt = inUser.password.Substring(0, inUser.password.Length - fakeSaltString.Length);
                txtPassword.Text = not_salt;
                txtPasswordConfirm.Text = not_salt;
                if (inUser.UserPermissions.Contains(IMETPOClasses.User.Permission.noemail))
                {
                    chkDoNotEmail.Checked = true;
                }
                else
                {
                    chkDoNotEmail.Checked = false;
                }
                txtFirstName.Text = inUser.firstname;
                txtLastName.Text = inUser.lastname;
            }
            // If this is self-editing, hide the permissions and other admin information.
            permissions.Visible = !isSelf;
            adminInfo.Visible = !isSelf;
            rowSelectUser.Visible = !isSelf;
            rowParentUser.Visible = !isSelf;
        }

        // Populate the administrator-level data.
        protected void PopulateAdminData()
        {
            if (!UserIsAdministrator)
            {
                userInfo.Visible = false;
                permissions.Visible = false;
                adminInfo.Visible = false;
                return;
            }
            User working = (User)GetSessionValue("WorkingUser");
            PopulateData(working, false);
            // Get a list of all users, and put that list in the "Users" dropdown and the "parent user" dropdown.
            SqlConnection conn = ConnectToConfigString("imetpsconnection");
            string query = "SELECT username, userid FROM users ORDER BY username";
            SqlCommand cmd = new SqlCommand()
            {
                Connection = conn,
                CommandType = CommandType.Text,
                CommandText = query
            };
            comboUsers.Items.Clear();
            comboUsers.Items.Add(new ListItem("New User", "NEW"));
            comboParentUser.Items.Clear();
            comboParentUser.Items.Add(new ListItem(string.Empty, string.Empty));

            SqlDataReader reader = cmd.ExecuteReader();
            try
            {
                while (reader.Read())
                {
                    comboUsers.Items.Add(new ListItem(reader[0].ToString(), reader[0].ToString()));
                    if (working != null && working.username == reader[0].ToString())
                    {
                        comboUsers.SelectedIndex = comboUsers.Items.Count - 1;
                    }
                    if (working == null || (working != null && working.username != reader[0].ToString()))
                    {
                        comboParentUser.Items.Add(new ListItem(reader[0].ToString(), reader[1].ToString()));
                        if (working != null && working.parentuser != null)
                        {
                            if (reader[1].ToString() == working.parentuser.userid.ToString())
                            {
                                comboParentUser.SelectedIndex = comboParentUser.Items.Count - 1;
                            }
                        }
                    }
                }
                reader.Close();
            }
            catch (Exception)
            {
            }
            // Populate the permissions lists with the current user's data.
            listAvailablePermissions.Items.Clear();
            listUserPermissions.Items.Clear();

            if (working != null)
            {
                if (working.UserPermissions.Contains(IMETPOClasses.User.Permission.admin))
                {
                    listUserPermissions.Items.Add(new ListItem("Administrator", ((int)IMETPOClasses.User.Permission.admin).ToString()));
                }
                else
                {
                    listAvailablePermissions.Items.Add(new ListItem("Administrator", ((int)IMETPOClasses.User.Permission.admin).ToString()));
                }
                if (working.UserPermissions.Contains(IMETPOClasses.User.Permission.inventory))
                {
                    listUserPermissions.Items.Add(new ListItem("Inventory", ((int)IMETPOClasses.User.Permission.inventory).ToString()));
                }
                else
                {
                    listAvailablePermissions.Items.Add(new ListItem("Inventory", ((int)IMETPOClasses.User.Permission.inventory).ToString()));
                }
                if (working.UserPermissions.Contains(IMETPOClasses.User.Permission.purchaser))
                {
                    listUserPermissions.Items.Add(new ListItem("Purchaser", ((int)IMETPOClasses.User.Permission.purchaser).ToString()));
                }
                else
                {
                    listAvailablePermissions.Items.Add(new ListItem("Purchaser", ((int)IMETPOClasses.User.Permission.purchaser).ToString()));
                }
                if (working.UserPermissions.Contains(IMETPOClasses.User.Permission.globalapprover))
                {
                    listUserPermissions.Items.Add(new ListItem("Bypass Approver", ((int)IMETPOClasses.User.Permission.globalapprover).ToString()));
                }
                else
                {
                    listAvailablePermissions.Items.Add(new ListItem("Bypass Approver", ((int)IMETPOClasses.User.Permission.globalapprover).ToString()));
                }
                if (working.UserPermissions.Contains(IMETPOClasses.User.Permission.globalrequestor))
                {
                    listUserPermissions.Items.Add(new ListItem("Bypass Requestor", ((int)IMETPOClasses.User.Permission.globalrequestor).ToString()));
                }
                else
                {
                    listAvailablePermissions.Items.Add(new ListItem("Bypass Requestor", ((int)IMETPOClasses.User.Permission.globalrequestor).ToString()));
                }
            }
            else
            {
                permissions.Visible = false;
            }
            // Get the system values, and place them in the system values fields.
            cmd = new SqlCommand()
            {
                Connection = conn,
                CommandType = CommandType.StoredProcedure,
                CommandText = "sp_getsystemsetting"
            };
            cmd.Parameters.Add(new SqlParameter("@intag", "accountbypasslimit"));
            reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                txtAccountBypassLimit.Text = reader["value"].ToString();
            }
            reader.Close();

            cmd = new SqlCommand()
            {
                Connection = conn,
                CommandType = CommandType.StoredProcedure,
                CommandText = "sp_getsystemsetting"
            };
            cmd.Parameters.Add(new SqlParameter("@intag", "approver_nagcutoff"));
            reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                txtApproverNagFrequency.Text = reader["value"].ToString();
            }
            reader.Close();

            cmd = new SqlCommand()
            {
                Connection = conn,
                CommandType = CommandType.StoredProcedure,
                CommandText = "sp_getsystemsetting"
            };
            cmd.Parameters.Add(new SqlParameter("@intag", "purchaser_nagcutoff"));
            reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                txtPurchaserNagFrequency.Text = reader["value"].ToString();
            }
            reader.Close();

            cmd = new SqlCommand()
            {
                Connection = conn,
                CommandType = CommandType.StoredProcedure,
                CommandText = "sp_getsystemsetting"
            };
            cmd.Parameters.Add(new SqlParameter("@intag", "receiver_nagcutoff"));
            reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                txtReceiverNagFrequency.Text = reader["value"].ToString();
            }
            reader.Close();
            conn.Close();
        }

        // Add a permission to a user's account.  These post back, which is a bit inefficient, 
        protected void btnAddPermission_Click(object sender, EventArgs e)
        {
            try
            {
                if (listAvailablePermissions.SelectedIndex < 0)
                    return;
                User working = (User)GetSessionValue("WorkingUser");
                if (working == null)
                {
                    ShowAlert("No user is currently loaded.");
                    return;
                }
                // Add the new permission to the current user object.
                IMETPOClasses.User.Permission new_perm = (IMETPOClasses.User.Permission)int.Parse(listAvailablePermissions.SelectedValue);
                if (!working.UserPermissions.Contains(new_perm))
                    working.UserPermissions.Add(new_perm);
                SqlConnection conn = ConnectToConfigString("imetpsconnection");
                // Automatically save all permission changes.
                working.Save(conn);
                conn.Close();
                PopulateAdminData();
                SetSessionValue("WorkingUser", working);
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        // Remove a permission from the current user's list of permissions.
        protected void btnRemovePermission_Click(object sender, EventArgs e)
        {
            try
            {
                if (listUserPermissions.SelectedIndex < 0)
                    return;
                IMETPOClasses.User working = (IMETPOClasses.User)GetSessionValue("WorkingUser");
                if (working == null)
                {
                    ShowAlert("No user is currently loaded.");
                    return;
                }
                IMETPOClasses.User.Permission new_perm = (IMETPOClasses.User.Permission)int.Parse(listUserPermissions.SelectedValue);
                if (working.UserPermissions.Contains(new_perm))
                    working.UserPermissions.Remove(new_perm);
                SqlConnection conn = ConnectToConfigString("imetpsconnection");
                working.Save(conn);
                conn.Close();
                PopulateAdminData();
                SetSessionValue("WorkingUser", working);
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        // Save a user's details.
        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtEmail.Text))
                {
                    ShowAlert("An email address is required for this user.");
                    return;
                }
                if (IsSelf())
                {
                    if (string.IsNullOrEmpty(txtPassword.Text))
                    {
                        ShowAlert("The user must have a password.");
                        return;
                    }
                }
                if (string.IsNullOrEmpty(txtUsername.Text))
                {
                    ShowAlert("The user must have a username");
                    return;
                }
                if (txtPassword.Text != txtPasswordConfirm.Text)
                {
                    ShowAlert("The passwords do not match.  Please try again.");
                    return;
                }
                IMETPOClasses.User working = (IMETPOClasses.User)GetSessionValue("WorkingUser");
                if (working == null)
                {
                    working = new User();
                }
                if (string.IsNullOrEmpty(working.password) && string.IsNullOrEmpty(txtPassword.Text))
                {
                    ShowAlert("This user must have a password.");
                    return;
                }
                /*if (!string.IsNullOrEmpty(working.password) && string.IsNullOrEmpty(txtPassword.Text))
                {
                    ShowAlert("You have not entered a password; the user's password is unchanged.");
                }*/
                if (string.IsNullOrEmpty(txtFirstName.Text) || string.IsNullOrEmpty(txtLastName.Text))
                {
                    ShowAlert("The user must have a first and last name.");
                    return;
                }
                working.username = txtUsername.Text;
                working.email = txtEmail.Text;
                working.firstname = txtFirstName.Text;
                working.lastname = txtLastName.Text;

                // Save the user's salted and hashed password.
                int saltSize = 5;
                string salt = CreateSalt(saltSize);
                if (!string.IsNullOrEmpty(txtPassword.Text))
                {
                    string passwordHash = CreatePasswordHash(this.txtPassword.Text, salt);
                    working.password = passwordHash;
                }
                SqlConnection conn = ConnectToConfigString("imetpsconnection");
                if (!string.IsNullOrEmpty(comboParentUser.SelectedValue))
                {
                    working.parentuser = new User();
                    working.parentuser.Load(conn, new Guid(comboParentUser.SelectedValue));
                    bool circle = false;
                    User current_user = working.parentuser;
                    int count = 0;
                    while (current_user != null)
                    {
                        if (current_user.userid == working.userid)
                        {
                            circle = true;
                            break;
                        }
                        current_user = current_user.parentuser;
                        count += 1;
                        if (count > 100)
                        {
                            circle = true;
                            break;
                        }
                    }
                    if (circle)
                    {
                        working.parentuser = null;
                        ShowAlert("You have attempted to create a circular user relationship.  Please select a different parent for this user.");
                        return;
                    }
                }
                else
                {
                    working.parentuser = null;
                }
                if (chkDoNotEmail.Checked)
                {
                    if (!working.UserPermissions.Contains(IMETPOClasses.User.Permission.noemail))
                        working.UserPermissions.Add(IMETPOClasses.User.Permission.noemail);
                }
                else
                {
                    if (working.UserPermissions.Contains(IMETPOClasses.User.Permission.noemail))
                        working.UserPermissions.Remove(IMETPOClasses.User.Permission.noemail);
                }
                working.Save(conn);
                conn.Close();
                SetSessionValue("WorkingUser", working);
                if (IsSelf())
                {
                    PopulateData(working, true);
                }
                else
                {
                    PopulateAdminData();
                }
                ShowAlert("User record updated.");
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        // When the user selected a user from the user dropdown, populate that user's data to the page.
        protected void comboUsers_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (comboUsers.SelectedValue == "NEW")
                {
                    RemoveSessionValue("WorkingUser");
                }
                else
                {
                    SqlConnection conn = ConnectToConfigString("imetpsconnection");
                    IMETPOClasses.User working = new IMETPOClasses.User();
                    working.LoadByUsername(conn, comboUsers.SelectedValue);
                    SetSessionValue("WorkingUser", working);
                    conn.Close();
                }
                PopulateAdminData();
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        // Save changes to the system values.
        protected void btnUpdateSystemValues_Click(object sender, EventArgs e)
        {
            try
            {
                SqlConnection conn = ConnectToConfigString("imetpsconnection");
                SqlCommand cmd = null;
                cmd = new SqlCommand()
                {
                    Connection = conn,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "sp_savesystemsetting"
                };
                cmd.Parameters.Add(new SqlParameter("@intag", "accountbypasslimit"));
                cmd.Parameters.Add(new SqlParameter("@invalue", txtAccountBypassLimit.Text));
                cmd.ExecuteNonQuery();

                cmd = new SqlCommand()
                {
                    Connection = conn,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "sp_savesystemsetting"
                };
                cmd.Parameters.Add(new SqlParameter("@intag", "approver_nagcutoff"));
                cmd.Parameters.Add(new SqlParameter("@invalue", txtApproverNagFrequency.Text));
                cmd.ExecuteNonQuery();

                cmd = new SqlCommand()
                {
                    Connection = conn,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "sp_savesystemsetting"
                };
                cmd.Parameters.Add(new SqlParameter("@intag", "purchaser_nagcutoff"));
                cmd.Parameters.Add(new SqlParameter("@invalue", txtPurchaserNagFrequency.Text));
                cmd.ExecuteNonQuery();

                cmd = new SqlCommand()
                {
                    Connection = conn,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "sp_savesystemsetting"
                };
                cmd.Parameters.Add(new SqlParameter("@intag", "receiver_nagcutoff"));
                cmd.Parameters.Add(new SqlParameter("@invalue", txtReceiverNagFrequency.Text));
                cmd.ExecuteNonQuery();

                conn.Close();
                PopulateAdminData();
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }
    }
}
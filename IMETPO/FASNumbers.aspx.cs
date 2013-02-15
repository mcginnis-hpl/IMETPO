using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Data;
using System.Data.SqlClient;
using IMETPOClasses;

namespace IMETPO
{
    public partial class FASNumbers : imetspage
    {
        protected override void OnInit(EventArgs e)
        {
            try
            {
                SqlConnection conn = ConnectToConfigString("imetpsconnection");
                PopulateData(conn, !IsPostBack);
                permissions.Visible = false;
                conn.Close();
                base.OnInit(e);
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

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
                PopulateHeader(appTitle, appSubtitle);
                if (!string.IsNullOrEmpty(hiddenUpdateNumber.Value))
                {
                    SqlConnection conn = ConnectToConfigString("imetpsconnection");
                    UpdateFASNumber(conn, hiddenUpdateNumber.Value);
                    PopulateData(conn, true);
                    conn.Close();
                    hiddenUpdateNumber.Value = string.Empty;
                }
                if (!string.IsNullOrEmpty(hiddenPermissions.Value))
                {
                    SqlConnection conn = ConnectToConfigString("imetpsconnection");
                    PopulatePermissions(conn);
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        protected void UpdateFASNumber(SqlConnection conn, string num)
        {
            FASNumber new_f = new FASNumber();
            TextBox txt = (TextBox)Page.FindControl("txtFRSNumber_" + num);
            new_f.Number = txt.Text;
            if (string.IsNullOrEmpty(new_f.Number))
            {
                ShowAlert("You must enter a valid FRS Number to save.");
                return;
            }
            txt = (TextBox)Page.FindControl("txtFRSDesc_" + num);
            new_f.Description = txt.Text;
            DropDownList exec = (DropDownList)Page.FindControl("comboFRSExecutor_" + num);
            if (string.IsNullOrEmpty(exec.SelectedValue))
            {
                ShowAlert("You must enter a valid owner to save.");
                return;
            }
            new_f.Permissions.Add(new FASPermission(new Guid(exec.SelectedValue), IMETPOClasses.User.Permission.owner));
            new_f.Permissions.Add(new FASPermission(new Guid(exec.SelectedValue), IMETPOClasses.User.Permission.requestor));
            new_f.Permissions.Add(new FASPermission(new Guid(exec.SelectedValue), IMETPOClasses.User.Permission.approver));
            CheckBox check = (CheckBox)Page.FindControl("chkFRSDisabled_" + num);
            new_f.Disabled = check.Checked;
            new_f.Save(conn);
            if (num == "new")
            {
                txtFRSNumber_new.Text = string.Empty;
                txtFRSDesc_new.Text = string.Empty;
                comboFRSExecutor_new.SelectedIndex = 0;
                chkFRSDisabled_new.Checked = false;
            }
        }

        protected void PopulatePermissions(SqlConnection conn)
        {
            if (string.IsNullOrEmpty(hiddenPermissions.Value))
            {
                permissions.Visible = false;
                return;
            }

            if (hiddenPermissions.Value.IndexOf("NEW:") >= 0)
            {
                listAvailableRequestors.Items.Clear();
                listRequestors.Items.Clear();
                listApprovers.Items.Clear();
                hiddenPermissions.Value = hiddenPermissions.Value.Substring(4);
            }
            string control_id = "comboFRSExecutor_" + hiddenPermissions.Value;
            DropDownList executor_list = (DropDownList)Page.FindControl(control_id);
            if (executor_list == null)
                return;
            if (string.IsNullOrEmpty(executor_list.SelectedValue))
            {
                ShowAlert("You must select an owner for that account before editing permissions.");
                return;
            }
            permissions.Visible = true;
            if (listAvailableRequestors.Items.Count > 0 || listRequestors.Items.Count > 0 || listApprovers.Items.Count > 0)
                return;

            SqlCommand cmd = null;
            SqlDataReader reader = null;
            Guid ownerid = new Guid(executor_list.SelectedValue);
            hiddenOwnerid.Value = ownerid.ToString();

            User owner = new IMETPOClasses.User();
            owner.Load(conn, ownerid);

            listAvailableRequestors.Items.Add(new ListItem(owner.username, owner.userid.ToString()));

            cmd = new SqlCommand()
            {
                Connection = conn,
                CommandType = CommandType.StoredProcedure,
                CommandText = "sp_LookupRelatedUsers"
            };
            cmd.Parameters.Add(new SqlParameter("@inuserid", ownerid));
            reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                string username = reader["username"].ToString();
                string userid = reader["userid"].ToString();
                listAvailableRequestors.Items.Add(new ListItem(username, userid));
            }
            reader.Close();

            cmd = new SqlCommand()
            {
                Connection = conn,
                CommandType = CommandType.StoredProcedure,
                CommandText = "sp_lookupfasusers"
            };
            cmd.Parameters.Add(new SqlParameter("@infasnumber", hiddenPermissions.Value));
            reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                string username = reader["username"].ToString();
                string userid = reader["userid"].ToString();
                IMETPOClasses.User.Permission perm = (IMETPOClasses.User.Permission)int.Parse(reader["permission"].ToString());
                if (perm == IMETPOClasses.User.Permission.requestor)
                    listRequestors.Items.Add(new ListItem(username, userid));
                if (perm == IMETPOClasses.User.Permission.approver)
                    listApprovers.Items.Add(new ListItem(username, userid));
            }
            reader.Close();
        }

        protected void PopulateData(SqlConnection conn, bool populateData)
        {
            List<string> executors = new List<string>();
            List<string> executorids = new List<string>();
            SqlCommand cmd = new SqlCommand()
            {
                Connection = conn,
                CommandType = CommandType.StoredProcedure,
                CommandText = "sp_LoadUsers"
            };
            comboFRSExecutor_new.Items.Clear();
            comboFRSExecutor_new.Items.Add(new ListItem("", ""));

            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                string username = reader["username"].ToString();
                string userid = reader["userid"].ToString();
                if (!executorids.Contains(userid))
                {
                    executors.Add(username);
                    executorids.Add(userid);
                    comboFRSExecutor_new.Items.Add(new ListItem(username, userid.ToString()));
                }
            }
            reader.Close();

            cmd = new SqlCommand()
            {
                Connection = conn,
                CommandType = CommandType.StoredProcedure,
                CommandText = "sp_loadallfasnumbers"
            };
            int i = 0;
            while (i < tblFRSNumbers.Rows.Count)
            {
                if (tblFRSNumbers.Rows[i].GetType() == typeof(TableHeaderRow) || tblFRSNumbers.Rows[i].ID == "newrow")
                {
                    i += 1;
                }
                else
                {
                    tblFRSNumbers.Rows.RemoveAt(i);
                }
            }
            int row_index = -1;
            for (i = 0; i < tblFRSNumbers.Rows.Count; i++)
            {
                if (tblFRSNumbers.Rows[i].GetType() == typeof(TableHeaderRow))
                {
                    row_index = i + 1;
                }
            }
            reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                string fasnumber = reader["fasnumber"].ToString();

                TableRow tr = new TableRow();
                TableCell tc = new TableCell();
                TextBox txt = new TextBox();
                if (populateData)
                    txt.Text = fasnumber;
                txt.ID = "txtFRSNumber_" + fasnumber;
                tc.Controls.Add(txt);
                tr.Cells.Add(tc);

                tc = new TableCell();
                txt = new TextBox();
                if (populateData)
                    txt.Text = reader["description"].ToString();
                txt.ID = "txtFRSDesc_" + fasnumber;
                tc.Controls.Add(txt);
                tr.Cells.Add(tc);

                tc = new TableCell();
                DropDownList dl = new DropDownList();
                dl.ID = "comboFRSExecutor_" + fasnumber;
                string exid = reader["ownerid"].ToString();
                dl.Items.Add(new ListItem(string.Empty, string.Empty));
                for (int j = 0; j < executorids.Count; j++)
                {
                    dl.Items.Add(new ListItem(executors[j], executorids[j]));
                    if (exid == executorids[j] && populateData)
                    {
                        dl.SelectedIndex = dl.Items.Count - 1;
                    }
                }
                tc.Controls.Add(dl);
                tr.Cells.Add(tc);

                tc = new TableCell();
                CheckBox chk = new CheckBox();
                chk.ID = "chkFRSDisabled_" + fasnumber;
                if (populateData)
                {
                    if (int.Parse(reader["disabled"].ToString()) == 0)
                    {
                        chk.Checked = false;
                    }
                    else
                    {
                        chk.Checked = true;
                    }
                }
                tc.Controls.Add(chk);
                tr.Cells.Add(tc);

                tc = new TableCell();
                string link = "<a href=\"javascript:submitFASNumber('" + fasnumber + "')\">Modify FRS</a>";
                link += "&nbsp;&nbsp;&nbsp;<a href=\"javascript:editPermissions('" + fasnumber + "')\">Edit Permissions</a>";
                tc.Text = link;
                tr.Cells.Add(tc);
                tblFRSNumbers.Rows.AddAt(row_index, tr);
                row_index += 1;
            }
            reader.Close();
        }

        protected void btnAddRequestor_Click(object sender, EventArgs e)
        {
            try
            {
                if (listAvailableRequestors.SelectedIndex < 0)
                    return;
                foreach (ListItem li in listRequestors.Items)
                {
                    if (li.Value == listAvailableRequestors.SelectedValue)
                        return;
                }
                ListItem new_item = new ListItem(listAvailableRequestors.SelectedItem.Text, listAvailableRequestors.SelectedItem.Value);
                listRequestors.Items.Add(new_item);
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        protected void btnRemoveRequestor_Click(object sender, EventArgs e)
        {
            try
            {
                if (listRequestors.SelectedIndex < 0)
                    return;
                if (listRequestors.SelectedValue == hiddenOwnerid.Value)
                {
                    ShowAlert("You can not remove the owner's requestor permissions.");
                    return;
                }
                listRequestors.Items.RemoveAt(listRequestors.SelectedIndex);
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        protected void btnAddApprover_Click(object sender, EventArgs e)
        {
            try
            {
                if (listAvailableRequestors.SelectedIndex < 0)
                    return;
                foreach (ListItem li in listApprovers.Items)
                {
                    if (li.Value == listAvailableRequestors.SelectedValue)
                        return;
                }
                ListItem new_item = new ListItem(listAvailableRequestors.SelectedItem.Text, listAvailableRequestors.SelectedItem.Value);
                listApprovers.Items.Add(new_item);
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        protected void btnRemoveApprover_Click(object sender, EventArgs e)
        {
            try
            {
                if (listApprovers.SelectedIndex < 0)
                    return;
                if (listApprovers.SelectedValue == hiddenOwnerid.Value)
                {
                    ShowAlert("You can not remove the owner's approver permissions.");
                    return;
                }
                listApprovers.Items.RemoveAt(listApprovers.SelectedIndex);
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        protected void btnSavePermissionsChanges_Click(object sender, EventArgs e)
        {
            try
            {
                SqlConnection conn = ConnectToConfigString("imetpsconnection");
                SqlCommand cmd = new SqlCommand()
                {
                    Connection = conn,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "sp_clearfasmap"
                };
                cmd.Parameters.Add(new SqlParameter("@infasnumber", hiddenPermissions.Value));
                cmd.ExecuteNonQuery();
                string query = string.Empty;
                foreach (ListItem li in listRequestors.Items)
                {
                    query += "INSERT INTO users2fas(uid,fasnumber,permission) VALUES('" + li.Value + "', '" + hiddenPermissions.Value + "', " + ((int)IMETPOClasses.User.Permission.requestor).ToString() + ");";
                }
                foreach (ListItem li in listApprovers.Items)
                {
                    query += "INSERT INTO users2fas(uid,fasnumber,permission) VALUES('" + li.Value + "', '" + hiddenPermissions.Value + "', " + ((int)IMETPOClasses.User.Permission.approver).ToString() + ");";
                }
                if (!string.IsNullOrEmpty(query))
                {
                    cmd = new SqlCommand()
                    {
                        Connection = conn,
                        CommandType = CommandType.Text,
                        CommandText = query
                    };
                    cmd.ExecuteNonQuery();
                }
                hiddenPermissions.Value = string.Empty;
                hiddenOwnerid.Value = string.Empty;
                permissions.Visible = false;
                listRequestors.Items.Clear();
                listAvailableRequestors.Items.Clear();
                listApprovers.Items.Clear();
                conn.Close();
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }
    }
}
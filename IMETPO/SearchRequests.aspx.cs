using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using IMETPOClasses;

namespace IMETPO
{
    public partial class SearchRequests : imetspage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsAuthenticated)
            {
                string url = "Default.aspx?RETURNURL=" + Request.Url.ToString();
                Response.Redirect(url);
                return;
            }
            try {
            PopulateHeader(appTitle, appSubtitle);
            if (!IsPostBack)
            {
                Search(true);
            }
            if (!string.IsNullOrEmpty(hiddenUndelete.Value))
            {
                Guid requestid = new Guid(hiddenUndelete.Value);
                hiddenUndelete.Value = string.Empty;
                Undelete(requestid);
            }
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        protected void Undelete(Guid requestid)
        {
            SqlConnection conn = ConnectToConfigString("imetpsconnection");
            try
            {
                PurchaseRequest working = new PurchaseRequest();
                working.Load(conn, requestid);
                working.state = PurchaseRequest.RequestState.pending;
                working.history.Add(new RequestTransaction(RequestTransaction.TransactionType.Modified, CurrentUser.userid, CurrentUser.username, "Undeleted by administrator."));
                working.Save(conn);
            }
            catch
            {
            }
            finally
            {
            conn.Close();
            }
            Search(true);
        }

        protected void Search(bool useParams)
        {
            SqlConnection conn = ConnectToConfigString("imetpsconnection");
            SqlCommand cmd = null;
            SqlDataReader reader = null;
            string query = string.Empty;
            DateTime startdate = DateTime.MinValue;
            DateTime enddate = DateTime.MinValue;
            List<int> status = new List<int>();
            Guid requestorid = Guid.Empty;
            Guid approverid = Guid.Empty;
            bool limitscope = false;
            string linkmode = string.Empty;
            int userrole = -1;
            for (int i = 0; i < Request.Params.Count; i++)
            {
                if (Request.Params.GetKey(i) == "USERONLY")
                {
                    limitscope = true;
                }
                if (Request.Params.GetKey(i) == "USERROLE")
                {
                    userrole = int.Parse(Request.Params[i]);
                }
                if (Request.Params.GetKey(i) == "LINKMODE")
                {
                    linkmode = Request.Params[i];
                }
            }
            if (comboRequestor.Items.Count <= 0)
            {
                comboRequestor.Items.Add(new ListItem("All", "ALL"));
                query = "SELECT username, userid FROM users ORDER BY username";
                cmd = new SqlCommand()
                {
                    Connection = conn,
                    CommandText = query,
                    CommandType = CommandType.Text
                };
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    comboRequestor.Items.Add(new ListItem(reader["username"].ToString(), reader["userid"].ToString()));
                }
                reader.Close();
            }
            if (useParams)
            {
                for (int i = 0; i < Request.Params.Count; i++)
                {
                    if (Request.Params.GetKey(i) == "STATUS")
                    {
                        if (Request.Params[i].IndexOf("|") > 0)
                        {
                            char[] delim = {'|'};
                            string[] tokens = Request.Params[i].Split(delim);
                            foreach (string s in tokens)
                            {
                                try
                                {
                                    status.Add(int.Parse(s));
                                }
                                catch (FormatException)
                                {
                                }
                            }
                        }
                        else
                        {
                            status.Add(int.Parse(Request.Params[i]));
                        }
                        for (int j = 0; j < comboFilterStatus.Items.Count; j++)
                        {
                            if (comboFilterStatus.Items[j].Value == status[0].ToString())
                            {
                                comboFilterStatus.SelectedIndex = j;
                                break;
                            }
                        }
                    }
                    if (Request.Params.GetKey(i) == "STARTDATE")
                    {
                        startdate = DateTime.Parse(Request.Params[i]);
                        txtStartDate.Text = startdate.ToShortDateString();
                    }
                    if (Request.Params.GetKey(i) == "ENDDATE")
                    {
                        enddate = DateTime.Parse(Request.Params[i]);
                        txtEndDate.Text = enddate.ToShortDateString();
                    }
                    if (Request.Params.GetKey(i) == "REQUESTOR")
                    {
                        requestorid = new Guid(Request.Params[i]);
                        for (int j = 0; j < comboRequestor.Items.Count; j++)
                        {
                            if (comboRequestor.Items[j].Value == Request.Params[i])
                            {
                                comboRequestor.SelectedIndex = j;
                                break;
                            }
                        }
                    }
                    if (Request.Params.GetKey(i) == "APPROVER")
                    {
                        approverid = new Guid(Request.Params[i]);
                    }
                }
            }
            else
            {
                if (comboFilterStatus.SelectedIndex > 0)
                {
                    status.Add(int.Parse(comboFilterStatus.SelectedValue));
                }
                if (!string.IsNullOrEmpty(txtStartDate.Text))
                {
                    startdate = DateTime.Parse(txtStartDate.Text);
                }
                if (!string.IsNullOrEmpty(txtEndDate.Text))
                {
                    enddate = DateTime.Parse(txtEndDate.Text);
                }
                if (comboRequestor.SelectedIndex > 0)
                {
                    requestorid = new Guid(comboRequestor.SelectedValue);
                }
            }
            query = "SELECT requestid, fasnumber, requestorname, vendorname, tagnumber, description, status, requestdate FROM v_requests_search";
            bool init = false;
            if (status.Count > 0)
            {
                if (!init)
                {
                    query += " WHERE";
                }
                else
                {
                    query += " AND";
                }
                init = true;
                query += " (status=" + status[0].ToString();
                for (int i = 1; i < status.Count; i++)
                {
                    query += " OR status=" + status[i].ToString();
                }
                query += ")";
            }
            if (startdate != DateTime.MinValue)
            {
                if (!init)
                {
                    query += " WHERE";
                }
                else
                {
                    query += " AND";
                }
                init = true;
                query += " (requestdate >='" + startdate.ToShortDateString() + "')";
            }
            if (enddate != DateTime.MinValue)
            {
                if (!init)
                {
                    query += " WHERE";
                }
                else
                {
                    query += " AND";
                }
                init = true;
                query += " (requestdate <='" + enddate.ToShortDateString() + "')";
            }
            if (requestorid != Guid.Empty)
            {
                if (!init)
                {
                    query += " WHERE";
                }
                else
                {
                    query += " AND";
                }
                init = true;
                query += " (requestorid ='" + requestorid.ToString() + "')";
            }
            if (limitscope)
            {
                if (!init)
                {
                    query += " WHERE";
                }
                else
                {
                    query += " AND";
                }
                init = true;
                query += " (userid='" + CurrentUser.userid.ToString() + "'";
                if (userrole >= 0)
                {
                    query += " AND permission=" + userrole.ToString();
                }
                query += ")";
            }
            query += " GROUP BY requestid, fasnumber, requestorname, vendorname, tagnumber, description, status, requestdate ORDER BY requestdate";
            cmd = new SqlCommand()
            {
                Connection = conn,
                CommandType = CommandType.Text,
                CommandText = query
            };
            string html = "<table class='example table-autosort:0 table-stripeclass:alternate'><thead><tr>";
		    html += "<th class='table-sortable:date'>Request Date</th>";
            html += "<th class='table-sortable:default'>Account #</th>";
            html += "<th class='table-sortable:default'>Requestor</th>";
            html += "<th class='table-sortable:default'>Vendor</th>";
            html += "<th class='table-sortable:default'>Tag #</th>";
            html += "<th class='table-sortable:default'>Status</th>";
            html += "<th class='table-sortable:default'>Description</th>";
            html += "<th></th>";
            html += "</tr></thead><tbody>";
            reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                html += "<tr>";
                html += "<td>" + reader["requestdate"].ToString() + "</td>";
                html += "<td>" + reader["fasnumber"].ToString() + "</td>";
                html += "<td>" + reader["requestorname"].ToString() + "</td>";
                html += "<td>" + reader["vendorname"].ToString() + "</td>";
                html += "<td>" + reader["tagnumber"].ToString() + "</td>";
                PurchaseRequest.RequestState curr_state = (PurchaseRequest.RequestState)int.Parse(reader["status"].ToString());
                html += "<td>" + PurchaseRequest.GetRequestStateString(curr_state) + "</td>";
                html += "<td>" + reader["description"].ToString() + "</td>";
                html += "<td><a href='SubmitRequest.aspx?REQUESTID=" + reader["requestid"].ToString();
                if (!string.IsNullOrEmpty(linkmode))
                {
                    html += "&MODE=" + linkmode;
                }
                html += "'>View</a>";
                if (curr_state == PurchaseRequest.RequestState.deleted && UserIsAdministrator)
                {
                    html += "&nbsp;&nbsp;&nbsp;<a href='javascript:undelete(\"" + reader["requestid"].ToString() + "\")'>Undelete</a>";
                }
                html += "</td>";
                html += "</tr>";
            }
            reader.Close();
            conn.Close();
            html += "</tbody></table>";
            results.InnerHtml = html;
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            try {
            Search(false);
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }
    }
}
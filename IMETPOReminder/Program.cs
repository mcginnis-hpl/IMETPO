using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using IMETPOClasses;
using System.Configuration;

namespace IMETPOReminder
{
    class Program
    {
        public static SqlConnection Connect(string connectionString)
        {
            SqlConnection ret = null;
            try
            {
                ret = new SqlConnection(connectionString);
                ret.Open();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message + " " + ex.StackTrace);
                return null;
            }
            return ret;
        }

        static string GetApplicationSetting(string inKey)
        {
            string ret = ConfigurationManager.AppSettings[inKey];
            return ret;
        }

        public static void SendErrorEmail(Exception ex)
        {
            string[] to = { "smcginnis@umces.edu" };
            string[] cc = null;
            string[] bcc = null;
            string subject = GetApplicationSetting("applicationTitle") + " Error: " + ex.Message;
            string body = "<p>" + ex.Message + "</p><p>" + ex.StackTrace + "</p>";
            SendEmail(to, cc, bcc, subject, body);
        }

        // Send an email from the application one or more users.
        public static void SendEmail(string[] to, string[] cc, string[] bcc, string subject, string body)
        {
            string fromaddress = GetApplicationSetting("emailaddress");
            string fromuser = GetApplicationSetting("emailuser");
            string frompassword = GetApplicationSetting("emailpassword");
            string emailport = GetApplicationSetting("emailport");
            string emailhost = GetApplicationSetting("emailhost");
            string[] debug_to = { "smcginnis@umces.edu" };
            // Utils.SendEmail(to, cc, bcc, subject, body, fromaddress, fromuser, frompassword, emailport, emailhost);
            Utils.SendEmail(debug_to, cc, bcc, subject, body, fromaddress, fromuser, frompassword, emailport, emailhost);
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Starting!");
            string connstring = ConfigurationManager.ConnectionStrings["imetpsconnection"].ConnectionString;
            SqlConnection conn = Connect(connstring);
            if (conn == null)
                return;
            string str_cutoff_span = Utils.GetSystemSetting(conn, "approver_nagcutoff");
            Console.WriteLine("Approver cutoff span: " + str_cutoff_span);
            int cutoff_span = -1;
            try
            {
                SqlCommand cmd = null;
                SqlDataReader reader = null;
                List<Guid> candidates = new List<Guid>();
                Guid candidate = Guid.Empty;

                if (!string.IsNullOrEmpty(str_cutoff_span))
                {
                    cutoff_span = int.Parse(str_cutoff_span);

                    cmd = new SqlCommand()
                    {
                        Connection = conn,
                        CommandType = CommandType.StoredProcedure,
                        CommandText = "sp_getlaterequests_approvers"
                    };
                    cmd.Parameters.Add(new SqlParameter("@incutoff", cutoff_span));

                    reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        if (!reader.IsDBNull(reader.GetOrdinal("requestid")))
                        {
                            candidate = new Guid(reader["requestid"].ToString());
                            if (!candidates.Contains(candidate))
                                candidates.Add(candidate);
                        }
                    }
                    reader.Close();

                    foreach (Guid g in candidates)
                    {
                        Console.WriteLine("Processing: " + g.ToString());
                        PurchaseRequest working = new PurchaseRequest();
                        working.Load(conn, g);
                        List<string> to = new List<string>();
                        foreach (FASPermission f in working.fasnumber.Permissions)
                        {
                            if (f.permission == IMETPOClasses.User.Permission.approver)
                            {
                                IMETPOClasses.User u = new User();
                                u.Load(conn, f.userid);
                                if (!string.IsNullOrEmpty(u.email) && !to.Contains(u.email) && !u.UserPermissions.Contains(IMETPOClasses.User.Permission.noemail))
                                {
                                    to.Add(u.email);
                                }
                            }
                        }
                        List<string> bcc = new List<string>();
                        bcc.Add("smcginnis@umces.edu");
                        // to.Add("smcginnis@hpl.umces.edu");
                        if (to.Count > 0)
                        {
                            string subject = "[" + GetApplicationSetting("applicationTitle") + "]: Reminder - approval needed for purchase order against " + working.fasnumber.Number;
                            string body = "<p>A Purchase Request has been made against FAS number " + working.fasnumber.Number + " (" + working.fasnumber.Description + ") ";
                            body += "using " + GetApplicationSetting("applicationSubtitle") + ". You are being notified because the ";
                            body += "system indicates you are an executor of this FAS. This request will not be ";
                            body += "sent on for purchase until you approve it. You can also choose to reject the request, ";
                            body += "causing it to go back to the requestor for modification. Following is a summary of ";
                            body += "the request:</p>";
                            body += "<ul><li>IPS Number: " + working.tagnumber + "</li>";
                            body += "<li>Description: " + working.description + "</li>";
                            body += "<li>Requestor Notes: " + working.requestornotes + "</li>";
                            body += "<li>Action needed: APPROVAL/REJECTION</li></ul>";
                            body += "<p><a href='http://" + GetApplicationSetting("hostAddress") + "/SubmitRequest.aspx?REQUESTID=" + working.requestid.ToString() + "'>Click here to view this request.</a></p>";
                            body += "Thank you,<br/>" + GetApplicationSetting("applicationTitle") + " System";
                            SendEmail(to.ToArray<string>(), null, bcc.ToArray<string>(), subject, body);
                        }
                    }
                }

                // Purchaser notification
                str_cutoff_span = Utils.GetSystemSetting(conn, "purchaser_nagcutoff");
                Console.WriteLine("Purchaser cutoff span: " + str_cutoff_span);
                if (!string.IsNullOrEmpty(str_cutoff_span))
                {
                    cutoff_span = int.Parse(str_cutoff_span);
                    cmd = new SqlCommand()
                    {
                        Connection = conn,
                        CommandType = CommandType.StoredProcedure,
                        CommandText = "sp_getlaterequests_purchasers"
                    };
                    cmd.Parameters.Add(new SqlParameter("@incutoff", cutoff_span));
                    reader = cmd.ExecuteReader();
                    candidates.Clear();
                    candidate = Guid.Empty;
                    while (reader.Read())
                    {
                        if (!reader.IsDBNull(reader.GetOrdinal("requestid")))
                        {
                            candidate = new Guid(reader["requestid"].ToString());
                            if (!candidates.Contains(candidate))
                                candidates.Add(candidate);
                        }
                    }
                    reader.Close();
                    List<User> purchasers = IMETPOClasses.User.LoadUsersWithPermission(conn, IMETPOClasses.User.Permission.purchaser);

                    foreach (Guid g in candidates)
                    {
                        Console.WriteLine("Processing: " + g.ToString());
                        PurchaseRequest working = new PurchaseRequest();
                        working.Load(conn, g);
                        List<string> to = new List<string>();
                        foreach (User u in purchasers)
                        {
                            if (!string.IsNullOrEmpty(u.email) && !to.Contains(u.email))
                                to.Add(u.email);
                        }
                        List<string> bcc = new List<string>();
                        bcc.Add("smcginnis@umces.edu");
                        // to.Add("smcginnis@hpl.umces.edu");
                        if (to.Count > 0)
                        {
                            string subject = "[" + GetApplicationSetting("applicationTitle") + "]: Reminder - request " + working.tagnumber + " awaiting purchase";
                            string body = "<p>A Purchase Request has been approved and is awaiting purchase. Following is a summary ";
                            body += "of the request:</p>";
                            body += "<ul><li>IPS Number: " + working.tagnumber + "</li>";
                            body += "<li>Description: " + working.description + "</li>";
                            body += "<li>Requestor Notes: " + working.requestornotes + "</li>";
                            body += "<li>Executor Notes: " + working.executornotes + "</li>";
                            body += "<li>Action Required: PURCHASE</li></ul>";
                            body += "<p><a href='http://" + GetApplicationSetting("hostAddress") + "/SubmitRequest.aspx?REQUESTID=" + working.requestid.ToString() + "'>Click here to view this request.</a></p>";
                            body += "Thank you,<br/>" + GetApplicationSetting("applicationTitle") + " System";
                            SendEmail(to.ToArray<string>(), null, bcc.ToArray<string>(), subject, body);
                        }
                    }
                }

                // Receiver notification
                str_cutoff_span = Utils.GetSystemSetting(conn, "receiver_nagcutoff");
                Console.WriteLine("Receiver cutoff span: " + str_cutoff_span);
                if (!string.IsNullOrEmpty(str_cutoff_span))
                {
                    cutoff_span = int.Parse(str_cutoff_span);
                    cmd = new SqlCommand()
                    {
                        Connection = conn,
                        CommandType = CommandType.StoredProcedure,
                        CommandText = "sp_getlaterequests_receivers"
                    };
                    cmd.Parameters.Add(new SqlParameter("@incutoff", cutoff_span));
                    reader = cmd.ExecuteReader();
                    candidates.Clear();
                    candidate = Guid.Empty;
                    while (reader.Read())
                    {
                        if (!reader.IsDBNull(reader.GetOrdinal("requestid")))
                        {
                            candidate = new Guid(reader["requestid"].ToString());
                            if (!candidates.Contains(candidate))
                                candidates.Add(candidate);
                        }
                    }
                    reader.Close();
                    List<User> purchasers = IMETPOClasses.User.LoadUsersWithPermission(conn, IMETPOClasses.User.Permission.purchaser);

                    foreach (Guid g in candidates)
                    {
                        Console.WriteLine("Processing: " + g.ToString());
                        PurchaseRequest working = new PurchaseRequest();
                        working.Load(conn, g);
                        List<string> to = new List<string>();
                        Guid userid = working.GetTransactionUser(RequestTransaction.TransactionType.Opened);
                        User u = new User();
                        u.Load(conn, userid);

                        if (!string.IsNullOrEmpty(u.email))
                        {
                            to.Add(u.email);
                        }
                        List<string> bcc = new List<string>();
                        bcc.Add("smcginnis@umces.edu");
                        // to.Add("smcginnis@hpl.umces.edu");
                        if (to.Count > 0)
                        {
                            string subject = "[" + GetApplicationSetting("applicationTitle") + "]: Reminder - request " + working.tagnumber + " awaiting receipt";
                            string body = "<p>A Purchase Request has been purchased and needs to be marked as received. Following is a summary ";
                            body += "of the request:</p>";
                            body += "<ul><li>IPS Number: " + working.tagnumber + "</li>";
                            body += "<li>Description: " + working.description + "</li>";
                            body += "<li>Requestor Notes: " + working.requestornotes + "</li>";
                            body += "<li>Executor Notes: " + working.executornotes + "</li>";
                            body += "<li>Action Required: RECEIVE</li></ul>";
                            body += "<p><a href='http://" + GetApplicationSetting("hostAddress") + "/SubmitRequest.aspx?REQUESTID=" + working.requestid.ToString() + "'>Click here to view this request.</a></p>";
                            body += "Thank you,<br/>" + GetApplicationSetting("applicationTitle") + " System";
                            SendEmail(to.ToArray<string>(), null, bcc.ToArray<string>(), subject, body);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SendErrorEmail(ex);
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                    conn = null;
                }
            }

        }
    }
}

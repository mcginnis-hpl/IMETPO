using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Net.Mail;
using IMETPOClasses;
using System.Web.UI.HtmlControls;

namespace IMETPO
{
    public class imetspage : Page
    {                       
        public SqlConnection Connect(string connectionString)
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

        public SqlConnection ConnectToConfigString(string key)
        {
            string connstring = ConfigurationManager.ConnectionStrings[key].ConnectionString;
            return this.Connect(connstring);
        }

        public static string CreatePasswordHash(string pwd, string salt)
        {
            return (FormsAuthentication.HashPasswordForStoringInConfigFile(pwd + salt, "SHA1") + salt);
        }

        public static string CreateSalt(int size)
        {
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] buff = new byte[size];
            rng.GetBytes(buff);
            return Convert.ToBase64String(buff);
        }

        public object GetSessionValue(string key)
        {
            try
            {
                return this.Session[this.Session.SessionID + "-" + key];
            }
            catch (Exception)
            {
                return null;
            }
        }
        
        public void Logout()
        {
            this.Session.Clear();
            this.Session.Abandon();
            FormsAuthentication.SignOut();
            base.Response.Cache.SetExpires(DateTime.Now);
            base.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            base.Response.Cache.SetNoStore();
        }

        public void RemoveSessionValue(string key)
        {
            this.Session.Remove(this.Session.SessionID + "-" + key);
        }

        public void SetSessionValue(string key, object inobj)
        {
            this.Session[this.Session.SessionID + "-" + key] = inobj;
        }

        public virtual void ShowAlert(string msg)
        {
            AddStartupCall("alert('" + msg + "');", "MessagePopUp");
            // base.ClientScript.RegisterStartupScript(this.GetType(), "MessagePopUp", "<script type='text/javascript'>alert('" + msg + "')</script>", false);
        }

        public void AddStartupCall(string call, string name)
        {
            base.ClientScript.RegisterStartupScript(base.GetType(), name, "<script type='text/javascript'>" + call + "</script>", false);
        }

        public void RemoveStartupCall(string name)
        {
            base.ClientScript.RegisterStartupScript(base.GetType(), name, string.Empty, true);
        }

        public User CurrentUser
        {
            get
            {
                Guid ret = Guid.Empty;
                if (GetSessionValue("currentuser") != null)
                {
                    return (User)GetSessionValue("currentuser");
                }
                if (string.IsNullOrEmpty(Username))
                    return new IMETPOClasses.User();

                SqlConnection conn = ConnectToConfigString("imetpsconnection");
                User p = new IMETPOClasses.User();
                p.LoadByUsername(conn, Username);
                SetSessionValue("currentuser", p);
                conn.Close();
                return p;
            }
        }

        public string CurrentUserEmail
        {
            get
            {
                if (CurrentUser != null)
                    return CurrentUser.email;
                return string.Empty;
            }
        }

        public bool IsAuthenticated
        {
            get
            {
                return base.Request.IsAuthenticated;
            }
        }

        public bool UserIsAdministrator
        {
            get
            {
                return CurrentUser.UserPermissions.Contains(IMETPOClasses.User.Permission.admin);
            }
        }

        public string Username
        {
            get
            {
                return HttpContext.Current.User.Identity.Name;
            }
        }

        public string GetApplicationSetting(string inkey)
        {
            return ConfigurationManager.AppSettings[inkey];
        }

        public void PopulateHeader(HtmlGenericControl title, HtmlGenericControl subtitle)
        {
            string title_text = GetApplicationSetting("applicationTitle");
            string subtitle_text = GetApplicationSetting("applicationSubtitle");
            if(title != null)
                title.InnerText = title_text;
            if(subtitle != null)
                subtitle.InnerText = subtitle_text;

            Page.Title = title_text + ": " + subtitle_text;
        }

        public void HandleError(Exception ex)
        {
            ShowAlert("An error occurred; the developer has been notified.\n" + ex.Message + "\n" + ex.StackTrace);
            SendErrorNotification(ex.Message, ex.StackTrace);
        }

        public void SendErrorNotification(string msg, string stacktrace)
        {
            try
            {
                string[] to = { "smcginnis@umces.edu" };
                string subject = "IMETPS Error";
                if (CurrentUser != null)
                {
                    subject += " User: " + CurrentUser.username;
                }
                string body = "<p>Message: " + msg + "</p>";
                body += "<p>Stack trace: " + stacktrace + "</p>";
                SendEmail(to, null, null, subject, body);
            }
            catch
            {
            }
        }

        public void SendEmail(string[] to, string[] cc, string[] bcc, string subject, string body)
        {
            string fromaddress = GetApplicationSetting("emailaddress");
            string fromuser = GetApplicationSetting("emailuser");
            string frompassword = GetApplicationSetting("emailpassword");
            MailMessage mail = new MailMessage();
            mail.From = new MailAddress(fromaddress);
            if (to != null)
            {
                foreach (string s in to)
                {
                    mail.To.Add(new MailAddress(s));
                }
            }
            if (cc != null)
            {
                foreach (string s in cc)
                {
                    mail.CC.Add(new MailAddress(s));
                }
            }
            if (bcc != null)
            {
                foreach (string s in bcc)
                {
                    mail.Bcc.Add(new MailAddress(s));
                }
            }
            // mail.To.Add(new MailAddress("sean@seanwmcginnis.com"));
            mail.Subject = subject;
            mail.Body = body;
            mail.IsBodyHtml = true;

            SmtpClient client = new SmtpClient();
            client.Port = int.Parse(GetApplicationSetting("emailport"));
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Host = GetApplicationSetting("emailhost");
            client.Credentials = new System.Net.NetworkCredential(fromuser, frompassword);
            client.EnableSsl = true; // runtime encrypt the SMTP communications using SSL
            client.Send(mail);
        }
    }
}
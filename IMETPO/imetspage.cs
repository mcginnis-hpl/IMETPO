﻿using System;
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
        // Connect to the database indicated by connectionString
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

        // Connect to a database that has its connection string listed in web.config
        public SqlConnection ConnectToConfigString(string key)
        {
            string connstring = ConfigurationManager.ConnectionStrings[key].ConnectionString;
            return this.Connect(connstring);
        }

        // Hash a password
        public static string CreatePasswordHash(string pwd, string salt)
        {
            return (FormsAuthentication.HashPasswordForStoringInConfigFile(pwd + salt, "SHA1") + salt);
        }

        // Generate some password salt
        public static string CreateSalt(int size)
        {
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] buff = new byte[size];
            rng.GetBytes(buff);
            return Convert.ToBase64String(buff);
        }

        // Return a session value.  This was done to prevent multiple concurrent sessions from mixing and matching objects
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

        // Remove a session value
        public void RemoveSessionValue(string key)
        {
            this.Session.Remove(this.Session.SessionID + "-" + key);
        }

        // Set a session value
        public void SetSessionValue(string key, object inobj)
        {
            this.Session[this.Session.SessionID + "-" + key] = inobj;
        }

        // Inserts a javascript popup that shows text msg.
        public virtual void ShowAlert(string msg)
        {
            AddStartupCall("alert('" + msg + "');", "MessagePopUp");
        }

        // Inserts a javascript popup that shows text msg.
        public virtual void ShowAlertWithRedirect(string msg, string destination)
        {
            string jscript = "alert('" + msg + "');window.location='" + destination + "';";
            AddStartupCall(jscript, "MessagePopUp");
        }

        // Add a javascript to the page
        public void AddStartupCall(string call, string name)
        {
            base.ClientScript.RegisterStartupScript(this.GetType(), name, call, true);
        }

        // Remove a javascript fron the page
        public void RemoveStartupCall(string name)
        {
            base.ClientScript.RegisterStartupScript(this.GetType(), name, string.Empty, true);
        }

        // Return a user object for the currently logged in user (loading it if the user does not exist)
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
                if (CurrentUser == null)
                    return false;
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

        // Return a value from web.config
        public string GetApplicationSetting(string inkey)
        {
            return ConfigurationManager.AppSettings[inkey];
        }

        // Dynamically fill out the header values.  This was done to facilitate deploying the application a multiple facilities.
        public void PopulateHeader(HtmlGenericControl title)
        {
            string title_text = GetApplicationSetting("applicationTitle");
            string subtitle_text = GetApplicationSetting("applicationSubtitle");
            string fulltitle = title_text + " (" + subtitle_text + ")";
            if(title != null)
                title.InnerText = fulltitle;

            Page.Title = fulltitle;
        }

        public void HandleError(Exception ex)
        {
            HandleError(ex, string.Empty);
        }

        // Show the user an error, and then send an email to the developer with error details.
        public void HandleError(Exception ex, string optional)
        {
            ShowAlert("An error occurred; the developer has been notified.\n" + ex.Message + "\n" + ex.StackTrace);
            SendErrorNotification(ex.Message + optional, ex.StackTrace);
        }

        // Send an email to the developer, including a stack trace and an exception message.
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

        // Send an email from the application one or more users.
        public void SendEmail(string[] to, string[] cc, string[] bcc, string subject, string body)
        {
            string fromaddress = GetApplicationSetting("emailaddress");
            string fromuser = GetApplicationSetting("emailuser");
            string frompassword = GetApplicationSetting("emailpassword");
            string emailport = GetApplicationSetting("emailport");
            string emailhost = GetApplicationSetting("emailhost");
            Utils.SendEmail(to, cc, bcc, subject, body, fromaddress, fromuser, frompassword, emailport, emailhost);
        }
    }
}
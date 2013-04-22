using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Net.Mail;

namespace IMETPOClasses
{
    public class Utils
    {
        // Return a system setting, which is stored in the system_settings table of the database
        // Returns an empty string if the value does not exist.
        public static string GetSystemSetting(SqlConnection conn, string tag)
        {
            string ret = string.Empty;
            SqlCommand cmd = new SqlCommand()
            {
                Connection = conn,
                CommandType = CommandType.StoredProcedure,
                CommandText = "sp_getsystemsetting"
            };
            cmd.Parameters.Add(new SqlParameter("@intag", tag));
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                if(!reader.IsDBNull(reader.GetOrdinal("value")))
                    ret = reader["value"].ToString();
            }
            reader.Close();
            return ret;
        }

        // Send an email from the application one or more users.
        public static void SendEmail(string[] to, string[] cc, string[] bcc, string subject, string body, string fromaddress, string fromuser, string frompassword, string emailport, string emailhost)
        {
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
            client.Port = int.Parse(emailport);
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Host = emailhost;
            client.Credentials = new System.Net.NetworkCredential(fromuser, frompassword);
            client.EnableSsl = true; // runtime encrypt the SMTP communications using SSL
            client.Send(mail);
        }
    }
}

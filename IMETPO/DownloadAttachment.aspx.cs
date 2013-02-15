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
    public partial class DownloadAttachment : imetspage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Guid attachmentid = Guid.Empty;
            for (int i = 0; i < Request.Params.Count; i++)
            {
                if (Request.Params.GetKey(i).ToUpper() == "ATTACHMENTID")
                {
                    attachmentid = new Guid(Request.Params[i]);
                    break;
                }
            }
            if (attachmentid != Guid.Empty)
            {
                SqlConnection conn = ConnectToConfigString("imetpsconnection");
                AttachedFile af = new AttachedFile();
                af.Load(conn, attachmentid);
                conn.Close();

                if (af.ID != Guid.Empty)
                {
                    byte[] fileBytes = af.GetBytes();
                    System.Web.HttpContext context = System.Web.HttpContext.Current;
                    context.Response.Clear();
                    context.Response.ClearHeaders();
                    context.Response.ClearContent();
                    context.Response.AppendHeader("content-length", fileBytes.Length.ToString());
                    context.Response.ContentType = MimeHelper.GetMimeType(af.Filename);
                    context.Response.AppendHeader("content-disposition", "attachment; filename=" + af.Filename);
                    context.Response.BinaryWrite(fileBytes);

                    // use this instead of response.end to avoid thread aborted exception (known issue):
                    // http://support.microsoft.com/kb/312629/EN-US
                    context.ApplicationInstance.CompleteRequest();
                }
            }
            else
            {
                Response.Write("<h1>File not found.</h1>");
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace IMETPOClasses
{
    public class AttachedFile
    {
        public string Path;
        public string Filename;
        public Guid ID;
        public AttachedFile()
        {
            Path = string.Empty;
            ID = Guid.Empty;
            Filename = string.Empty;
        }

        public void Load(SqlConnection conn, Guid inid)
        {
            SqlCommand cmd = new SqlCommand()
            {
                Connection = conn,
                CommandText = "sp_loadattachedfile",
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add(new SqlParameter("@inuploadedfile_id", inid));
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                if (!reader.IsDBNull(reader.GetOrdinal("uploadedfile_id")))
                {
                    ID = new Guid(reader["uploadedfile_id"].ToString());
                }
                if (!reader.IsDBNull(reader.GetOrdinal("filepath")))
                {
                    Path = reader["filepath"].ToString();
                }
                if (!reader.IsDBNull(reader.GetOrdinal("filename")))
                {
                    Filename = reader["filename"].ToString();
                }
            }
            reader.Close();
        }

        public void Save(SqlConnection conn, Guid requestid)
        {
            if (ID == Guid.Empty)
                ID = Guid.NewGuid();
            SqlCommand cmd = new SqlCommand()
            {
                Connection = conn,
                CommandType = CommandType.StoredProcedure,
                CommandText = "sp_saveattachedfile"
            };
            cmd.Parameters.Add(new SqlParameter("@inid", ID));
            cmd.Parameters.Add(new SqlParameter("@inrequestid", requestid));
            cmd.Parameters.Add(new SqlParameter("@inpath", Path));
            cmd.Parameters.Add(new SqlParameter("@infilename", Filename));
            cmd.ExecuteNonQuery();
        }

        public byte[] GetBytes()
        {
            FileStream fs = null;
            byte[] ret = null;
            try
            {
                fs = File.Open(Path, FileMode.Open);
                ret = new byte[fs.Length];
                fs.Read(ret, 0, (int)fs.Length);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (fs != null)
                {
                    try
                    {
                        fs.Close();
                        fs = null;
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            return ret;
        }
    }
}

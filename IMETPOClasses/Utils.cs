using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace IMETPOClasses
{
    public class Utils
    {
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
    }
}

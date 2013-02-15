using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace IMETPOClasses
{
    public class FASPermission
    {
        // This class holds a mapping between an FRS number and a user id (the same data as the users2fas table in the database)
        public Guid userid;
        public User.Permission permission;

        public FASPermission(Guid inuserid, User.Permission inpermission)
        {
            userid = inuserid;
            permission = inpermission;
        }
    }

    public class FASNumber
    {
        // The FRS Number in question
        public string Number;
        // A description of this number
        public string Description;
        // The disabled flag
        public bool Disabled;
        // A mapping from userids to permissions
        public List<FASPermission> Permissions;

        public Guid OwnerID
        {
            get
            {
                Guid ret = Guid.Empty;
                foreach (FASPermission p in Permissions)
                {
                    if (p.permission == User.Permission.owner)
                    {
                        ret = p.userid;
                        break;
                    }
                }
                return ret;
            }
        }

        public FASNumber()
        {
            Number = string.Empty;
            Description = string.Empty;
            Disabled = false;
            Permissions = new List<FASPermission>();
        }
       
        public void Load(SqlConnection conn, string inNumber)
        {
            SqlCommand cmd = new SqlCommand()
            {
                Connection = conn,
                CommandType = CommandType.StoredProcedure,
                CommandText = "sp_loadfasnumber"
            };
            cmd.Parameters.Add(new SqlParameter("@infasnumber", inNumber));
            SqlDataReader reader = cmd.ExecuteReader();
            List<FASNumber> ret = new List<FASNumber>();
            while (reader.Read())
            {
                Description = reader["description"].ToString();
                if (int.Parse(reader["disabled"].ToString()) != 0)
                {
                    Disabled = true;
                }
                Number = reader["fasnumber"].ToString();
                User.Permission perm = (User.Permission)int.Parse(reader["permission"].ToString());
                Guid userid = new Guid(reader["userid"].ToString());
                Permissions.Add(new FASPermission(userid, perm));
            }
            reader.Close();
        }

        public void Save(SqlConnection conn)
        {
            SqlCommand cmd = new SqlCommand()
            {
                Connection = conn,
                CommandType = CommandType.StoredProcedure,
                CommandText = "sp_savefasnumber"
            };
            cmd.Parameters.Add(new SqlParameter("@infasnumber", Number));
            cmd.Parameters.Add(new SqlParameter("@indescription", Description));
            if (Disabled)
            {
                SqlParameter param = new SqlParameter("@indisabled", SqlDbType.TinyInt);
                param.Value = 1;
                cmd.Parameters.Add(param);
            }
            else
            {
                SqlParameter param = new SqlParameter("@indisabled", SqlDbType.TinyInt);
                param.Value = 0;
                cmd.Parameters.Add(param);
            }
            cmd.ExecuteNonQuery();

            string query = "DELETE FROM users2fas WHERE fasnumber='" + Number + "';";
            foreach (FASPermission perm in Permissions)
            {
                query += "INSERT INTO users2fas (uid, fasnumber, permission) VALUES ('" + perm.userid.ToString() + "', '" + Number + "', " + ((int)perm.permission).ToString() + ");";
            }
            cmd = new SqlCommand()
            {
                Connection = conn,
                CommandType = CommandType.Text,
                CommandText = query
            };
            cmd.ExecuteNonQuery();
        }
    }

    // Holds data about a user.
    public class User
    {
        // Permissions map to an integer, which is stored in the database
        public enum Permission
        {
            requestor = 0,
            purchaser = 2,
            owner = 4,
            approver = 8,
            admin = 16,
            inventory = 32,
            globalapprover = 64,
            noemail = 128,
            globalrequestor = 256
        }
        // A list of permissions that he user possesses
        public List<Permission> UserPermissions;

        public string username;
        public Guid userid;
        public string email;
        public string password;
        public User parentuser;

        public User()
        {
            userid = Guid.Empty;
            username = string.Empty;
            email = string.Empty;
            password = string.Empty;
            UserPermissions = new List<Permission>();
            parentuser = null;
        }

        // Return a list of all users that possess permission inPermission
        public static List<User> LoadUsersWithPermission(SqlConnection conn, User.Permission inPermission)
        {
            SqlCommand cmd = new SqlCommand()
            {
                Connection = conn,
                CommandType = CommandType.StoredProcedure,
                CommandText = "sp_LookupUsersWithPermission"
            };
            cmd.Parameters.Add(new SqlParameter("@inpermission", (int)inPermission));
            SqlDataReader reader = cmd.ExecuteReader();
            List<User> ret = new List<User>();
            while (reader.Read())
            {
                User u = new User();
                u.username = reader["username"].ToString();
                u.userid = new Guid(reader["userid"].ToString());
                u.email = reader["email"].ToString();
                ret.Add(u);
            }
            reader.Close();
            return ret;
        }

        // Return a list of all users that possess permission inPermission
        public static List<User> LoadAllUsers(SqlConnection conn)
        {
            SqlCommand cmd = new SqlCommand()
            {
                Connection = conn,
                CommandType = CommandType.StoredProcedure,
                CommandText = "sp_LoadAllUsers"
            };
            SqlDataReader reader = cmd.ExecuteReader();
            List<User> ret = new List<User>();
            while (reader.Read())
            {
                User u = new User();
                u.username = reader["username"].ToString();
                u.userid = new Guid(reader["userid"].ToString());
                u.email = reader["email"].ToString();
                ret.Add(u);
            }
            reader.Close();
            return ret;
        }

        // Load all of the FRS numbers for which this user has permission inpermission
        public List<FASNumber> LoadFASNumbers(SqlConnection conn, User.Permission inpermission)
        {
            SqlCommand cmd = null;
            if (inpermission == Permission.globalrequestor)
            {
                cmd = new SqlCommand()
                {
                    Connection = conn,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "sp_lookupallfasnumbers"
                };
            }
            else
            {
                cmd = new SqlCommand()
                {
                    Connection = conn,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "sp_lookupfasnumbers"
                };
                cmd.Parameters.Add(new SqlParameter("@inuserid", userid));
            }
            
            SqlDataReader reader = cmd.ExecuteReader();
            List<FASNumber> ret = new List<FASNumber>();
            while (reader.Read())
            {
                FASNumber f = new FASNumber();
                f.Description = reader["description"].ToString();
                if (int.Parse(reader["disabled"].ToString()) != 0)
                {
                    f.Disabled = true;
                }
                f.Number = reader["fasnumber"].ToString();
                bool found = false;
                User.Permission perm = (User.Permission)int.Parse(reader["permission"].ToString());
                Guid perm_userid = new Guid(reader["userid"].ToString());
                foreach (FASNumber curr in ret)
                {
                    if (curr.Number == f.Number)
                    {
                        curr.Permissions.Add(new FASPermission(perm_userid, perm));
                        found = true;
                        break;
                    }
                }                               
                if (found)
                {
                    continue;
                }
                f.Permissions.Add(new FASPermission(perm_userid, perm));
                if (perm == inpermission)
                {
                    ret.Add(f);
                }
            }
            reader.Close();
            return ret;
        }

        public void Save(SqlConnection conn)
        {
            if (userid == Guid.Empty)
            {
                userid = Guid.NewGuid();
            }
            SqlCommand cmd = new SqlCommand()
            {
                Connection = conn,
                CommandType = CommandType.StoredProcedure,
                CommandText = "sp_saveuser"
            };
            cmd.Parameters.Add(new SqlParameter("@inusername", username));
            cmd.Parameters.Add(new SqlParameter("@inpassword", password));
            cmd.Parameters.Add(new SqlParameter("@inemail", email));
            cmd.Parameters.Add(new SqlParameter("@inuserid", userid));
            if (parentuser != null)
            {
                cmd.Parameters.Add(new SqlParameter("@inparentuserid", parentuser.userid));
            }
            cmd.ExecuteNonQuery();

            string query = "DELETE FROM user_permissions WHERE userid='" + userid.ToString() + "';";
            foreach (Permission p in UserPermissions)
            {
                query += "INSERT INTO user_permissions(userid,permission) VALUES('" + userid.ToString() + "', " + ((int)p).ToString() + ");";
            }
            cmd = new SqlCommand()
            {
                Connection = conn,
                CommandType = CommandType.Text,
                CommandText = query
            };
            cmd.ExecuteNonQuery();
        }

        public void Load(SqlConnection conn, Guid inid)
        {
            SqlCommand cmd = new SqlCommand()
            {
                Connection = conn,
                CommandText = "sp_loaduser",
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add(new SqlParameter("@inuserid", inid));
            SqlDataReader reader = cmd.ExecuteReader();
            Guid temp_puid = Guid.Empty;
            while (reader.Read())
            {
                if (!reader.IsDBNull(reader.GetOrdinal("username")))
                {
                    username = reader["username"].ToString();
                }
                if (!reader.IsDBNull(reader.GetOrdinal("password")))
                {
                    password = reader["password"].ToString();
                }
                if (!reader.IsDBNull(reader.GetOrdinal("email")))
                {
                    email = reader["email"].ToString();
                }
                if (!reader.IsDBNull(reader.GetOrdinal("userid")))
                {
                    userid = new Guid(reader["userid"].ToString());
                }
                if (!reader.IsDBNull(reader.GetOrdinal("parentuserid")))
                {
                    temp_puid = new Guid(reader["parentuserid"].ToString());
                }
            }
            reader.Close();
            if (userid != Guid.Empty)
            {
                cmd = new SqlCommand()
                {
                    Connection = conn,
                    CommandText = "sp_lookuppermissions",
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.Add(new SqlParameter("@inuserid", userid));
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    if (!reader.IsDBNull(reader.GetOrdinal("permission")))
                    {
                        UserPermissions.Add((Permission)int.Parse(reader["permission"].ToString()));
                    }
                }
                reader.Close();                
            }
            if (temp_puid != Guid.Empty)
            {
                parentuser = new User();
                parentuser.Load(conn, temp_puid);
            }
        }

        public void LoadByUsername(SqlConnection conn, string inusername)
        {
            SqlCommand cmd = new SqlCommand()
            {
                Connection = conn,
                CommandText = "sp_LookupUser",
                CommandType = CommandType.StoredProcedure
            };
            Guid temp_puid = Guid.Empty;
            cmd.Parameters.Add(new SqlParameter("@inusername", inusername));
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                if (!reader.IsDBNull(reader.GetOrdinal("username")))
                {
                    username = reader["username"].ToString();
                }
                if (!reader.IsDBNull(reader.GetOrdinal("password")))
                {
                    password = reader["password"].ToString();
                }
                if (!reader.IsDBNull(reader.GetOrdinal("email")))
                {
                    email = reader["email"].ToString();
                }
                if (!reader.IsDBNull(reader.GetOrdinal("userid")))
                {
                    userid = new Guid(reader["userid"].ToString());
                }
                if (!reader.IsDBNull(reader.GetOrdinal("parentuserid")))
                {
                    temp_puid = new Guid(reader["parentuserid"].ToString());
                }
            }
            reader.Close();
            if (userid != Guid.Empty)
            {
                cmd = new SqlCommand()
                {
                    Connection = conn,
                    CommandText = "sp_lookuppermissions",
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.Add(new SqlParameter("@inuserid", userid));
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    if (!reader.IsDBNull(reader.GetOrdinal("permission")))
                    {
                        UserPermissions.Add((Permission)int.Parse(reader["permission"].ToString()));
                    }
                }
                reader.Close();                
            }
            if (temp_puid != Guid.Empty)
            {
                parentuser = new User();
                parentuser.Load(conn, temp_puid);
            }
        }

        // Map permission value to string, for display purposes.
        public static string GetPermissionString(Permission inPerm)
        {
            switch (inPerm)
            {
                case Permission.admin:
                    return "Administrator";
                case Permission.owner:
                    return "Owner";
                case Permission.inventory:
                    return "Inventory";
                case Permission.purchaser:
                    return "Purchaser";
                case Permission.requestor:
                    return "Requestor";
                case Permission.approver:
                    return "Approver";
                case Permission.globalapprover:
                    return "Bypass Approver";
                case Permission.globalrequestor:
                    return "Bypass Requestor";
                case Permission.noemail:
                    return "No Email";
            }
            return string.Empty;
        }
    }
}

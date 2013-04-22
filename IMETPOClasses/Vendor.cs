using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace IMETPOClasses
{
    /// <summary>
    /// A class encapsulating all of the data about a Vendor.
    /// </summary>
    public class Vendor
    {
        // The state of the vendor; currently, there are only two.
        public enum VendorState
        {
            Active = 0,
            Deleted = 2
        };
        // A unique identifier for this vendor.
        public Guid vendorid;
        // All of the various contact fields for the vendor.
        public string vendorname;
        public string url;
        public string description;
        public string address1;
        public string address2;
        public string city;
        public string st;
        public string zip;
        public string fein;
        public string phone;
        public string fax;
        public string contact_name;
        public string contact_phone;
        public string contact_email;
        public string customer_account_number;

        public VendorState state;

        public Vendor()
        {
            vendorid = Guid.Empty;
            vendorname = string.Empty;
            url = string.Empty;
            description = string.Empty;
            address1 = string.Empty;
            address2 = string.Empty;
            city = string.Empty;
            st = string.Empty;
            zip = string.Empty;
            fein = string.Empty;
            phone = string.Empty;
            fax = string.Empty;
            contact_name = string.Empty;
            contact_email = string.Empty;
            contact_phone = string.Empty;
            customer_account_number = string.Empty;
            state = VendorState.Active;
        }

        /// <summary>
        /// Save the information about the vendor to the database.
        /// </summary>
        /// <param name="conn">An open connection to the IMETPS database.</param>
        public void Save(SqlConnection conn)
        {
            if (vendorid == Guid.Empty)
            {
                vendorid = Guid.NewGuid();
            }
            SqlCommand cmd = new SqlCommand()
            {
                Connection = conn,
                CommandType = CommandType.StoredProcedure,
                CommandText = "sp_savevendor"
            };
            cmd.Parameters.Add(new SqlParameter("@invendorid", vendorid));
            cmd.Parameters.Add(new SqlParameter("@invendorname", vendorname));
            cmd.Parameters.Add(new SqlParameter("@inurl", url));
            cmd.Parameters.Add(new SqlParameter("@indescription", description));
            cmd.Parameters.Add(new SqlParameter("@inaddress1", address1));
            cmd.Parameters.Add(new SqlParameter("@inaddress2", address2));
            cmd.Parameters.Add(new SqlParameter("@incity", city));
            cmd.Parameters.Add(new SqlParameter("@inst", st));
            cmd.Parameters.Add(new SqlParameter("@inzip", zip));
            cmd.Parameters.Add(new SqlParameter("@infein", fein));
            cmd.Parameters.Add(new SqlParameter("@inphone", phone));
            cmd.Parameters.Add(new SqlParameter("@infax", fax));
            cmd.Parameters.Add(new SqlParameter("@instate", (int)state));
            cmd.Parameters.Add(new SqlParameter("@incontact_name", contact_name));
            cmd.Parameters.Add(new SqlParameter("@incontact_phone", contact_phone));
            cmd.Parameters.Add(new SqlParameter("@incontact_email", contact_email));
            cmd.Parameters.Add(new SqlParameter("@incustomer_account_number", customer_account_number));
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Load a vendor from the database.
        /// </summary>
        /// <param name="conn">An open connection to the IMETPS database.</param>
        /// <param name="inid">The ID of the vendor to load.</param>
        public void Load(SqlConnection conn, Guid inid)
        {
            SqlCommand cmd = new SqlCommand()
            {
                Connection = conn,
                CommandText = "sp_loadvendor",
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add(new SqlParameter("@invendorid", inid));
            SqlDataReader reader = cmd.ExecuteReader();
            Guid newuserid = Guid.Empty;
            Guid newvendorid = Guid.Empty;

            while (reader.Read())
            {
                if (!reader.IsDBNull(reader.GetOrdinal("id")))
                {
                    vendorid = new Guid(reader["id"].ToString());
                }
                if (!reader.IsDBNull(reader.GetOrdinal("vendorname")))
                {
                    vendorname = reader["vendorname"].ToString();
                }
                if (!reader.IsDBNull(reader.GetOrdinal("url")))
                {
                    url = reader["url"].ToString();
                }
                if (!reader.IsDBNull(reader.GetOrdinal("description")))
                {
                    description = reader["description"].ToString();
                }
                if (!reader.IsDBNull(reader.GetOrdinal("address1")))
                {
                    address1 = reader["address1"].ToString();
                }
                if (!reader.IsDBNull(reader.GetOrdinal("address2")))
                {
                    address2 = reader["address2"].ToString();
                }
                if (!reader.IsDBNull(reader.GetOrdinal("city")))
                {
                    city = reader["city"].ToString();
                }
                if (!reader.IsDBNull(reader.GetOrdinal("st")))
                {
                    st = reader["st"].ToString();
                }
                if (!reader.IsDBNull(reader.GetOrdinal("zip")))
                {
                    zip = reader["zip"].ToString();
                }
                if (!reader.IsDBNull(reader.GetOrdinal("fein")))
                {
                    fein = reader["fein"].ToString();
                }
                if (!reader.IsDBNull(reader.GetOrdinal("phone")))
                {
                    phone = reader["phone"].ToString();
                }
                if (!reader.IsDBNull(reader.GetOrdinal("fax")))
                {
                    fax = reader["fax"].ToString();
                }
                if (!reader.IsDBNull(reader.GetOrdinal("state")))
                {
                    state = (VendorState)int.Parse(reader["state"].ToString());
                }
                if (!reader.IsDBNull(reader.GetOrdinal("contact_name")))
                {
                    contact_name = reader["contact_name"].ToString();
                }
                if (!reader.IsDBNull(reader.GetOrdinal("contact_phone")))
                {
                    contact_phone = reader["contact_phone"].ToString();
                }
                if (!reader.IsDBNull(reader.GetOrdinal("contact_email")))
                {
                    contact_email = reader["contact_email"].ToString();
                }
                if (!reader.IsDBNull(reader.GetOrdinal("customer_account_number")))
                {
                    customer_account_number = reader["customer_account_number"].ToString();
                }
            }
            reader.Close();
        }

        /// <summary>
        /// Load all of the vendors that are in the system.
        /// </summary>
        /// <param name="conn">An open connection to the IMETPS database.</param>
        /// <returns>A list of all vendors currently in the system</returns>
        public static List<Vendor> LoadAllVendors(SqlConnection conn)
        {
            SqlCommand cmd = new SqlCommand()
            {
                Connection = conn,
                CommandText = "sp_loadallvendors",
                CommandType = CommandType.StoredProcedure
            };
            SqlDataReader reader = cmd.ExecuteReader();
            Guid newuserid = Guid.Empty;
            Guid newvendorid = Guid.Empty;
            List<Vendor> ret = new List<Vendor>();
            while (reader.Read())
            {
                Vendor v = new Vendor();
                if (!reader.IsDBNull(reader.GetOrdinal("id")))
                {
                    v.vendorid = new Guid(reader["id"].ToString());
                }
                if (!reader.IsDBNull(reader.GetOrdinal("vendorname")))
                {
                    v.vendorname = reader["vendorname"].ToString();
                }
                if (!reader.IsDBNull(reader.GetOrdinal("url")))
                {
                    v.url = reader["url"].ToString();
                }
                if (!reader.IsDBNull(reader.GetOrdinal("description")))
                {
                    v.description = reader["description"].ToString();
                }
                if (!reader.IsDBNull(reader.GetOrdinal("address1")))
                {
                    v.address1 = reader["address1"].ToString();
                }
                if (!reader.IsDBNull(reader.GetOrdinal("address2")))
                {
                    v.address2 = reader["address2"].ToString();
                }
                if (!reader.IsDBNull(reader.GetOrdinal("city")))
                {
                    v.city = reader["city"].ToString();
                }
                if (!reader.IsDBNull(reader.GetOrdinal("st")))
                {
                    v.st = reader["st"].ToString();
                }
                if (!reader.IsDBNull(reader.GetOrdinal("zip")))
                {
                    v.zip = reader["zip"].ToString();
                }
                if (!reader.IsDBNull(reader.GetOrdinal("fein")))
                {
                    v.fein = reader["fein"].ToString();
                }
                if (!reader.IsDBNull(reader.GetOrdinal("phone")))
                {
                    v.phone = reader["phone"].ToString();
                }
                if (!reader.IsDBNull(reader.GetOrdinal("fax")))
                {
                    v.fax = reader["fax"].ToString();
                }
                if (!reader.IsDBNull(reader.GetOrdinal("state")))
                {
                    v.state = (VendorState)int.Parse(reader["state"].ToString());
                }
                if (!reader.IsDBNull(reader.GetOrdinal("contact_name")))
                {
                    v.contact_name = reader["contact_name"].ToString();
                }
                if (!reader.IsDBNull(reader.GetOrdinal("contact_phone")))
                {
                    v.contact_phone = reader["contact_phone"].ToString();
                }
                if (!reader.IsDBNull(reader.GetOrdinal("contact_email")))
                {
                    v.contact_email = reader["contact_email"].ToString();
                }
                if (!reader.IsDBNull(reader.GetOrdinal("customer_account_number")))
                {
                    v.customer_account_number = reader["customer_account_number"].ToString();
                }
                ret.Add(v);
            }
            reader.Close();
            return ret;
        }
    }
}

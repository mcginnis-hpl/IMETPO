﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace IMETPOClasses
{
    public class RequestTransaction
    {
        // The transaction type maps to an integer, which is what is stored in the database.
        public enum TransactionType
        {
            Created = 0,
            Opened = 1,
            Modified = 2,
            Approved = 4,
            Rejected = 8,
            Purchased = 16,
            Closed = 32,
            Deleted = 64,
            Received = 128
        }

        // A function to map the state to a string, for display purposes.
        public static string TransactionTypeString(TransactionType inType)
        {
            switch (inType)
            {
                case TransactionType.Approved:
                    return "Approved";
                case TransactionType.Closed:
                    return "Closed";
                case TransactionType.Created:
                    return "Created";
                case TransactionType.Deleted:
                    return "Deleted";
                case TransactionType.Modified:
                    return "Modified";
                case TransactionType.Opened:
                    return "Opened";
                case TransactionType.Purchased:
                    return "Purchased";
                case TransactionType.Rejected:
                    return "Rejected";
                case TransactionType.Received:
                    return "Received";
            }
            return string.Empty;
        }

        public TransactionType transaction;
        // The userid of the person performing the transaction
        public Guid userid;
        // The username of the above person (so we can load these without having to load the whole user entity)
        public string username;
        // The full name of the above person (so we can load these without having to load the whole user entity)
        public string userfullname;
        // A timestamp, in server time
        public DateTime timestamp;
        public string comments;
        // This flag is false until the transaction is saved (when the request is saved); then it is flagged as true.
        public bool isLogged;

        public RequestTransaction()
        {
            transaction = TransactionType.Created;
            userid = Guid.Empty;
            timestamp = DateTime.MinValue;
            comments = string.Empty;
            isLogged = true;
            username = string.Empty;
            userfullname = string.Empty;
        }

        public RequestTransaction(TransactionType inType, Guid inuserid, string inusername, string incomments, string inuserfullname)
        {
            transaction = inType;
            userid = inuserid;
            comments = incomments;
            timestamp = DateTime.Now;
            isLogged = false;
            username = inusername;
            userfullname = inuserfullname;
        }

        /// <summary>
        /// Save this transaction to the database
        /// </summary>
        /// <param name="conn">An open connection to the IMETPS database</param>
        /// <param name="requestid">The GUID of this transaction.</param>
        public void Save(SqlConnection conn, Guid requestid)
        {
            if (isLogged)
                return;
            SqlCommand cmd = new SqlCommand()
            {
                Connection = conn,
                CommandType = CommandType.StoredProcedure,
                CommandText = "sp_savetransaction"
            };
            cmd.Parameters.Add(new SqlParameter("@inrequestid", requestid));
            cmd.Parameters.Add(new SqlParameter("@intransaction_type", (int)transaction));
            cmd.Parameters.Add(new SqlParameter("@incomments", comments));
            cmd.Parameters.Add(new SqlParameter("@intransaction_time", timestamp));
            cmd.Parameters.Add(new SqlParameter("@inuserid", userid));
            cmd.ExecuteNonQuery();
            isLogged = true;
        }
    }
}

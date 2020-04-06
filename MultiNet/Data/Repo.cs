using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiNet
{
    public class Repo
    {
        public static DataTable GetTable(string belgeNo)
        {
            using (var conn = new OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=MultiNet.accdb;"))
            {
                var cmd = new OleDbCommand();
                cmd.Connection = conn;
                cmd.CommandText = "Select * from Faturalar";
                if (!string.IsNullOrEmpty(belgeNo))
                {
                    cmd.CommandText += " where Id=@Id";
                    cmd.Parameters.AddWithValue("@Id", belgeNo);
                }

                conn.Open();
                var dr = cmd.ExecuteReader();
                DataTable dt = new DataTable();
                dt.Load(dr);

                return dt;
            }
        }
    }
}

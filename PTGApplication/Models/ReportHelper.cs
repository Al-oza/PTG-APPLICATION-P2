using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using System.Web;

namespace PTGApplication.Models
{
    public static class ReportHelper
    {
        public static SqlDataReader Query(string query)
        {
            using (var connection = new SqlConnection(Properties.Database.DefaultConnectionString
                    .Replace("[Source]", Environment.MachineName).Replace("[Catalog]",
                    Properties.Database.DatabaseName)))
            using (var cmd = new SqlCommand(query, connection))
            {
                connection.Open();
                return cmd.ExecuteReader();
            }
        }
    }
}
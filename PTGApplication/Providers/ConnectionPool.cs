using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;

namespace PTGApplication.Providers
{
    public class ConnectionPool : IDisposable
    {
        private readonly String _connectionString;
        private ConnectionPool()
        {
            var configuration = WebConfigurationManager.OpenWebConfiguration("~");
            var section = (ConnectionStringsSection)configuration.GetSection("connectionStrings");

            _connectionString = section.ConnectionStrings["DefaultConnection"].ToString();
        }
        private void ConnectToDatabase()
        {
            DatabaseConnection = new SqlConnection(_connectionString);
            DatabaseConnection.Open();
        }
        public static ConnectionPool GetConnectionPool()
        {
            var result = new ConnectionPool();
            result.ConnectToDatabase();
            return result;
        }
        public void Dispose()
        {
            DatabaseConnection.Close();
            DatabaseConnection.Dispose();
        }
        public static DataSet Query(String sqlcommand, params string[] tables)
        {
            using (var connection = GetConnectionPool())
            using (var adapter = new SqlDataAdapter())
            using (var command = new SqlCommand(sqlcommand,
                connection.DatabaseConnection))
            {
                command.CommandType = System.Data.CommandType.Text;
                foreach (var table in tables)
                {
                    adapter.TableMappings.Add(table, table);
                }
                adapter.SelectCommand = command;

                var dataSet = new DataSet("Report");
                adapter.Fill(dataSet);

                return dataSet;
            }
        }

        public static void ExecuteProcedure(String spname, int id)
        {
            using (var connection = GetConnectionPool())
            using (var cmd = new SqlCommand())
            {
                cmd.Connection = connection.DatabaseConnection;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = spname;
                cmd.Parameters.AddWithValue("@Id", id);

                cmd.ExecuteScalar();
            }
        }

        public String ConnectionString { get { return _connectionString; } }
        public SqlConnection DatabaseConnection { get; private set; }




    }
}
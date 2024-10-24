using Microsoft.Data.SqlClient;
using System.Data;

namespace Api
{
    public class Repository
    {
        private string connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=CRMDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False";

        /// <summary>
        /// executes SQL update, incert or delete.
        /// </summary>
        /// <param name="sql">the sql qurrey to execute</param>
        public void Execute(string sql)
        {

            // opens connection to db
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();

            //execute the qurrey
            SqlCommand command = new SqlCommand(sql, connection);
            command.ExecuteNonQuery();

            // close connection to db

            connection.Close();
        }
        public DataTable GetDataTable(string sql, SqlParameter[] parameters = null)
        {
            DataTable dataTable = new DataTable();

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    dataTable.Load(reader);
                }
            }

            return dataTable;
        }
    }
}

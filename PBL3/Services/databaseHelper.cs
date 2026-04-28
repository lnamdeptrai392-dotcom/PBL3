using Microsoft.Data.SqlClient;

namespace PBL3a.services
{
    public class DatabaseHelper
    {
        //thay the sau server=...; thay thanh server cua ban
        private string connectionString =
        "Server=.\\SQLEXPRESS;Database=PBL3aDB;Trusted_Connection=True;TrustServerCertificate=True;";

        public SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }
    }
}
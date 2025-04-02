using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeLibrary
{
    class DatabaseConnection
    {
        private readonly string _connectionString = @"Server=localhost\SQLEXPRESS;Database=library;Trusted_Connection=True;Encrypt=False";
        public SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}

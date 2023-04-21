using Epson.Core.Domain.Base;
using LinqToDB.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System.Data;

namespace Epson.Data
{
    public class EpsonSQLConnectionFactory : IDbConnectionFactory
    {
        private readonly IConfiguration _configuration;

        public EpsonSQLConnectionFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IDbConnection CreateDataConnection()
        {
            var connectionString = _configuration.GetConnectionString("EpsonDbConnection");

            return new MySqlConnection(connectionString);
        }

        //public DataConnection CreateDataConnection()
        //{
        //    var connectionString = _configuration.GetConnectionString("EpsonDbConnection");

        //    return new DataConnection("SqlServer", connectionString);
        //}

    }
}
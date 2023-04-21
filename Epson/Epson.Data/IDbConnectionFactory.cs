using LinqToDB.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epson.Data
{
    public interface IDbConnectionFactory
    {
        public IDbConnection CreateDataConnection();
    }
}

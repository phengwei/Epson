using Dapper;
using Epson.Core.Domain.Base;
using Epson.Core.Domain.Products;
using LinqToDB;
using LinqToDB.DataProvider;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epson.Data
{
    public class EntityRepository<T> : IRepository<T>
    {
        private readonly EpsonSQLConnectionFactory _dataConnectionProvider;

        public EntityRepository(EpsonSQLConnectionFactory dataConnectionProvider)
        {
            _dataConnectionProvider = dataConnectionProvider;
        }

        public IEnumerable<T> GetAll()
        {
            var tt = typeof(T).Name;
            using IDbConnection db = _dataConnectionProvider.CreateDataConnection();
            return db.Query<T>($"SELECT * FROM {typeof(T).Name}");
        }

        public T GetById(int id)
        {
            using IDbConnection db = _dataConnectionProvider.CreateDataConnection();
            return db.QueryFirstOrDefault<T>($"SELECT * FROM {typeof(T).Name} WHERE Id = @Id", new { Id = id });
        }

        public int Add(T entity)
        {
            using IDbConnection db = _dataConnectionProvider.CreateDataConnection();
            return db.Execute($"INSERT INTO {typeof(T).Name} VALUES (@Name, @Price)", entity);
        }

        public int Update(T entity)
        {
            using IDbConnection db = _dataConnectionProvider.CreateDataConnection();
            return db.Execute($"UPDATE {typeof(T).Name} SET Name = @Name, Price = @Price WHERE Id = @Id", entity);
        }

        public int Delete(int id)
        {
            using IDbConnection db = _dataConnectionProvider.CreateDataConnection();
            return db.Execute($"DELETE FROM {typeof(T).Name} WHERE Id = @Id", new { Id = id });
        }
    }


    //public class ProductRepository : EntityRepository<Product>
    //{
    //    public ProductRepository(Func<IDbConnection> dataConnectionProvider) : base(dataConnectionProvider) { }
    //}
}

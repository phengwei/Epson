using Dapper;
using Epson.Core.Domain.Base;
using Epson.Core.Domain.Products;
using LinqToDB;
using LinqToDB.DataProvider;
using Serilog;
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
        private readonly ILogger _logger;

        public EntityRepository(EpsonSQLConnectionFactory dataConnectionProvider,
            ILogger logger)
        {
            _dataConnectionProvider = dataConnectionProvider;
            _logger = logger;
        }

        public IQueryable<T> Table => GetAll().AsQueryable();

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

            var props = typeof(T).GetProperties()
                .Where(p =>
                    !typeof(IEnumerable<object>).IsAssignableFrom(p.PropertyType) && 
                    (!p.PropertyType.IsClass || p.PropertyType == typeof(string)))  
                .ToArray();

            var query = $"INSERT INTO {typeof(T).Name} ({string.Join(",", props.Select(p => p.Name))}) VALUES ({string.Join(",", props.Select(p => "@" + p.Name))}); SELECT LAST_INSERT_ID();";

            var parameters = new DynamicParameters();
            foreach (var prop in props)
            {
                parameters.Add("@" + prop.Name, prop.GetValue(entity));
            }

            return db.ExecuteScalar<int>(query, parameters);
        }


        public int Update(T entity)
        {
            using IDbConnection db = _dataConnectionProvider.CreateDataConnection();

            var properties = entity.GetType().GetProperties()
                                    .Where(p => p.PropertyType.IsValueType || p.PropertyType == typeof(string))
                                    .Where(p => !p.Name.Equals("id", StringComparison.OrdinalIgnoreCase)) 
                                    .ToArray();

            var query = new StringBuilder($"UPDATE {typeof(T).Name} SET ");

            for (int i = 0; i < properties.Length; i++)
            {
                var property = properties[i];
                query.Append($"{property.Name} = @{property.Name}");

                if (i < properties.Length - 1)
                    query.Append(", ");
            }

            query.Append(" WHERE id = @id");

            _logger.Information("Executing query {querystring}", query.ToString());
            return db.Execute(query.ToString(), entity);
        }


        public int Delete(int id)
        {
            using IDbConnection db = _dataConnectionProvider.CreateDataConnection();
            return db.Execute($"DELETE FROM {typeof(T).Name} WHERE Id = @Id", new { Id = id });
        }

        public int BulkInsert(IEnumerable<T> entities)
        {
            using IDbConnection db = _dataConnectionProvider.CreateDataConnection();
            var tableName = typeof(T).Name;
            var props = typeof(T).GetProperties();
            var columns = string.Join(", ", props.Select(p => p.Name));
            var values = string.Join(", ", props.Select(p => $"@{p.Name}"));
            var query = $"INSERT INTO {tableName} ({columns}) VALUES ({values})";
            return db.Execute(query, entities);
        }
    }


    //public class ProductRepository : EntityRepository<Product>
    //{
    //    public ProductRepository(Func<IDbConnection> dataConnectionProvider) : base(dataConnectionProvider) { }
    //}
}

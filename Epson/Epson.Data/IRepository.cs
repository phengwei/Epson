using Epson.Core.Domain.Base;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static LinqToDB.Reflection.Methods.LinqToDB.Insert;

namespace Epson.Data
{
    public interface IRepository<T>
    {
        IQueryable<T> Table { get; }
        public IEnumerable<T> GetAll();
        Task<IEnumerable<T>> GetAllAsync();
        public T GetById(int id);
        public int Add(T entity, IDbConnection connection = null, IDbTransaction transaction = null);
        public int Update(T entity, IDbConnection connection = null, IDbTransaction transaction = null);
        public int Delete(int id, IDbConnection connection = null, IDbTransaction transaction = null);
    }
}

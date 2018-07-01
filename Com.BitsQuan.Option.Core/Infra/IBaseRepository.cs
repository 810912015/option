using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Com.BitsQuan.Option.Core
{
    public interface IBaseRepository<T>
     where T : class
    {
        bool Add(T entity);
        bool AddRanged(IEnumerable<T> entities);
        bool Delete(T entity);
        bool DeleteRange(IEnumerable<T> entities);
        bool Delete(Func<T, bool> whereLambda);
        T Find(Func<T, bool> whereLambda);
        IQueryable<T> Load(Func<T, bool> whereLambda);
        IQueryable<T> LoadPage<S>(int pageIndex, 
            int pageSize, 
            out int total,
             Expression<Func<T, bool>> whereLambda,
            bool isAsc, 
            Expression<Func<T, S>> orderByLambda);
        bool Update(T entity);
    }
}

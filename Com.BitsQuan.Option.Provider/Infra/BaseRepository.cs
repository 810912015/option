using Com.BitsQuan.Option.Core;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions; 

namespace Com.BitsQuan.Option.Provider
{ 
    /// <summary>
    /// 实现对数据库的操作(增删改查)的基类
    /// </summary>
    /// <typeparam name="T">定义泛型，约束其是一个类</typeparam>
    public class BaseRepository<T> : IBaseRepository<T>,IDisposable where T : class
    {
        public DbContext dbContext = null;
        public void SetCtx(DbContext db) { this.dbContext = db; }
        public virtual List<T> All { get { return dbContext.Set<T>().ToList<T>(); } }
        // 实现对数据库的添加功能,添加实现EF框架的引用
        public virtual bool Add(T entity)
        {
            dbContext.Entry<T>(entity).State = EntityState.Added;
            //下面的写法统一
            return dbContext.SaveChanges() > 0;
        }
        public virtual bool AddRanged(IEnumerable<T> entities)
        {
            dbContext.Set<T>().AddRange(entities);
            return dbContext.SaveChanges() > 0;
        }
         
        //实现对数据库的修改功能
        public virtual bool Update(T entity)
        {
            dbContext.Set<T>().Attach(entity);
            dbContext.Entry<T>(entity).State = EntityState.Modified;
            return dbContext.SaveChanges() > 0;
        }
        public virtual bool UpdateWithChanged(T entity,List <string> propertyToChanged)
        {
            dbContext.Set<T>().Attach(entity);

            DbEntityEntry<T> entry = dbContext.Entry(entity);
             
            foreach (var v in propertyToChanged)
                entry .Property (v).IsModified =true ;
            return dbContext.SaveChanges() > 0;
        }
        public virtual bool UpdateWithUnchanged(T entity, List<string> propertyNotChanged)
        {

            dbContext.Set<T>().Attach(entity);
            DbEntityEntry<T> entry = dbContext.Entry(entity);
            entry.State = EntityState.Modified;
            foreach (var v in propertyNotChanged)
            {
                if (string.IsNullOrEmpty(v)) continue;
                entry.Property(v).IsModified = false;
            }
                
            return dbContext.SaveChanges() > 0;

        }
        //实现对数据库的删除功能
        public virtual bool Delete(T entity)
        {
            dbContext.Set<T>().Attach(entity);
            dbContext.Entry<T>(entity).State = EntityState.Deleted;
            return dbContext.SaveChanges() > 0;
        }
        public virtual bool DeleteRange(IEnumerable<T> entities)
        {
            dbContext.Set<T>().RemoveRange(entities);
            return dbContext.SaveChanges() > 0;
        }
        public virtual bool Delete(Func<T, bool> whereLambda)
        {
            T obj = dbContext.Set<T>().FirstOrDefault(whereLambda);
            return Delete(obj);
        }

        public virtual T Find(Func<T, bool> whereLambda)
        {
            return dbContext.Set<T>().FirstOrDefault(whereLambda);
        }

        //实现对数据库的查询  --简单查询
        public virtual IQueryable<T> Load(Func<T, bool> whereLambda)
        {
            return dbContext.Set<T>().Where<T>(whereLambda).AsQueryable();
        }

        /// <summary>
        /// 实现对数据的分页查询
        /// </summary>
        /// <typeparam name="S">按照某个类进行排序</typeparam>
        /// <param name="pageIndex">当前第几页</param>
        /// <param name="pageSize">一页显示多少条数据</param>
        /// <param name="total">总条数</param>
        /// <param name="whereLambda">取得排序的条件</param>
        /// <param name="isAsc">如何排序，根据倒叙还是升序</param>
        /// <param name="orderByLambda">根据那个字段进行排序</param>
        /// <returns></returns>
        public virtual IQueryable<T> LoadPage<S>(int pageIndex, int pageSize, out  int total, Expression<Func<T, bool>> whereLambda,
            bool isAsc, Expression<Func<T, S>> orderByLambda)
        {
            var temp = dbContext.Set<T>().Where<T>(whereLambda);
            total = temp.Count(); //得到总的条数
            //排序,获取当前页的数据
            if (isAsc)
            {
                temp =temp
                    .OrderBy<T, S>(orderByLambda)
                     .Skip<T>(pageSize * (pageIndex - 1)) //越过多少条
                     .Take<T>(pageSize).AsQueryable(); //取出多少条
            }
            else
            {
                temp =temp
                    .OrderByDescending<T, S>(orderByLambda)
                    .Skip<T>(pageSize * (pageIndex - 1)) //越过多少条
                    .Take<T>(pageSize).AsQueryable(); //取出多少条
            }
            return temp;
        }


        public void Dispose()
        {
            this.dbContext.Dispose();
        }
    }
}

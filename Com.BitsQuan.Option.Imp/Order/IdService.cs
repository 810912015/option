using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Provider;
using System.Linq;
using System.Threading;

namespace Com.BitsQuan.Option.Imp
{
    /// <summary>
    /// 对象Id服务:初始化时从数据库中读取类型的最大id
    /// 单例
    /// </summary>
    /// <typeparam name="T">要为之产生id的类型</typeparam>
    public class IdService<T> :SingletonWithInit<IdService<T>>, IInitialbe where T : class,IEntityWithId
    {
        int CurrentOrderId;
        public int NewId()
        {
            if (CurrentOrderId == int.MaxValue - 1)
            {
                Interlocked.Exchange(ref CurrentOrderId, 0);
            }
            Interlocked.Increment(ref CurrentOrderId);
            return CurrentOrderId;

        }
        public void SetCurrentOrderId(int id)
        {
            Interlocked.Exchange(ref CurrentOrderId, id);
        }
        public void Init() 
        {
            if (CurrentOrderId > 0) return;
            if (typeof(T) == typeof(Order))
            {
                //因为order和temporders中存储的是相同的东西,所以应该是两个表中较大的id值
                int fmax;
                using (var db = new OptionDbCtx())
                {
                    string sql = "select MAX(id) from TempOrders";
                    int? r1=0;
                    var r3 = db.Database.SqlQuery<int?>(sql);
                    if(r3!=null)
                        r1=r3.FirstOrDefault();
                    int r2 = 0;
                    var c = db.Set<T>();
                    var t = c.Count();
                    if (t > 0)
                    {
                        var max = db.Set<T>().Max(a => a.Id);
                        r2 = max;
                    }

                    fmax = r1 > r2 ? (r1??0) : r2;
                }
                SetCurrentOrderId(fmax + 1);
            }
            else
            {
                using (var db = new OptionDbCtx())
                {
                    var c = db.Set<T>();
                    var t = c.Count();
                    if (t > 0)
                    {
                        var max = db.Set<T>().Max(a => a.Id);
                        SetCurrentOrderId(max);
                    }
                    else SetCurrentOrderId(0);
                }
            }
        }
    }
     
}

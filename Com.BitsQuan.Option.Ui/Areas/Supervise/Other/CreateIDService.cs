using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Ui.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace Com.BitsQuan.Option.Ui.Areas.Supervise.Other
{

    /// <summary>
    /// 对象Id服务:初始化时从数据库中读取类型的最大id
    /// 单例
    /// </summary>
    /// <typeparam name="T">要为之产生id的类型</typeparam>
    public class BackIdService<T> : SingletonWithInit<BackIdService<T>>, IInitialbe where T : class,IEntityWithId
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
        protected void SetCurrentOrderId(int id)
        {
            Interlocked.Exchange(ref CurrentOrderId, id);
        }
        public void Init()
        {
            if (CurrentOrderId > 0) return;
            using (var db = new ApplicationDbContext())
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
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Com.BitsQuan.Option.Core
{
    /// <summary>
    /// 用于保存过去时间段内的数值
    ///     如果时间段内没有值保留时间段内的最后一个值
    /// </summary>
    public abstract class InPastTimeSpan<T>
    {
        Queue<Tuple<DateTime, T>> q = new Queue<Tuple<DateTime, T>>();
        object loc = new object();
        protected List<Tuple<DateTime, T>> queue
        {
            get
            {
                List<Tuple<DateTime, T>> r;
                lock (loc)
                    r = q.ToList();
                return r;
            }
        }
        protected double TimeIntervalInMinutes;
        public virtual void Put(T d)
        {
            if (q.Count == 0)
            {
                lock(loc)
                q.Enqueue(Tuple.Create(DateTime.Now, d));
            }
            else
            {
                while (q.Count > 0)
                {
                    var r = DateTime.Now.Subtract(q.Peek().Item1).TotalMinutes > TimeIntervalInMinutes;
                    if (r)
                    {
                        lock (loc)
                        q.Dequeue();
                        continue;
                    }
                    else
                        break;
                }
                lock (loc)
                q.Enqueue(Tuple.Create(DateTime.Now, d));
            }

        }


    }
}

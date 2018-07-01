using Com.BitsQuan.Option.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace Com.BitsQuan.Option.Match
{
    /// <summary>
    /// 委托生命周期管理器
    ///     超出指定时间不成交的委托踢出队列
    /// </summary>
    public class OrderLifeManager<T> where T : IOrder
    {
        List<T> l1;
        List<T> l2;
        object ll1;
        object ll2;
        public static double SpanInHours = Com.BitsQuan.Miscellaneous.AppSettings.Read<double>("orderExpireTimeInHours", 4);

        Timer t;
        string Tag;
        static TextLog log = new TextLog("orderlife.txt");
        public OrderLifeManager(List<T> l1, List<T> l2, object l1Lock, object l2Lock)
        {
            if (l1 == null || l2 == null || l1Lock == null || l2Lock == null) throw new ArgumentException("参数不能为空");
            if (typeof(T)==typeof( Order)) {
                Tag = "Option";
            }
            else
            {
                Tag = "Spot";
            }
            this.l1 = l1; this.l2 = l2;
            this.ll1 = l1Lock; this.ll2 = l2Lock; 
            t = new Timer();
            t.Interval = SpanInHours / 2 * 60 * 60 * 1000;
            t.Elapsed += t_Elapsed;
            t_Elapsed(null, null);
        }
        bool IsExpired(T o)
        {
            if (o == null) return false;
            var delta = DateTime.Now.Subtract(o.OrderTime).TotalHours;
            return delta >= SpanInHours;
        }
        public event Action<List<T>> OnExpired;
        void Check(List<T> l, object loc)
        {
            if (l.Count == 0) return;
            var tl1 = l.ToList();
            if (tl1.Count == 0) return;
            List<T> tl = new List<T>();
            StringBuilder sb = new StringBuilder();
           
            for (int i = 0; i < tl1.Count; i++)
            {
                var v = tl1[i];
                if (!IsExpired(v)) continue;
                tl.Add(v);
                
                sb.AppendFormat("-编号{0}-方向{1}-数量{2}-时间{3}-价格{4}-合约{5}",
                    v.Id, v.Direction, v.OrderCount, v.OrderTime, v.Price, v.Sign);
            }
            if (sb.Length > 0)
            {
                sb.Insert(0,Tag);
                sb.Insert(0, "因为超期抛弃委托");
                log.Info(sb.ToString());
            }
            if (tl.Count > 0)
            {
                lock (loc)
                {
                    foreach (var v in tl)
                    {
                        l.Remove(v);
                    }
                }
                
                if (OnExpired != null)
                {
                    OnExpired(tl);
                }
            }
        }
        void t_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                t.Stop();
                Check(l1, ll1);
                Check(l2, ll2);
            }
            catch (Exception ex)
            {
                Singleton<TextLog>.Instance.Error(ex);
            }
            finally
            {
                t.Start();
            }
        }
    }
}

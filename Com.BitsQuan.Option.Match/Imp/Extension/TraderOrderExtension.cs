using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Match;
using System;
using System.Linq;
namespace Com.BitsQuan.Option.Imp
{
    public class Limit100 : InPastTimeSpan<int>
    {
        public Limit100()
        {
            this.TimeIntervalInMinutes = 1;
        }
        public int CountInPast
        {
            get
            {
                if (queue.Count == 0) return 0;
                var r= queue.Where(a => a.Item1.AddMinutes(this.TimeIntervalInMinutes) >= DateTime.Now).Select(a => a.Item2).Sum();
                return r;
            }
        }
    }
    public static class TraderOrderExtension
    {
        static MyProperty<Trader, Limit100> l100 = new MyProperty<Trader, Limit100>();
        /// <summary>
        /// 每分钟下单量不能超过限制份数
        /// </summary>
        /// <param name="t"></param>
        /// <param name="o"></param>
        /// <returns></returns>
        public static bool CheckLimitPerMin(this Trader t, Order o)
        {
            try
            {
                if (o.IsBySystem) return true;
                var l = l100.Get(t);
                if (l == null)
                {
                    l = new Limit100();
                    l100.Set(t, l);
                }
                var b = (l.CountInPast + o.ReportCount) < OrderPreHandler.CountPerMinuteLimit;
                if (b)
                    l.Put(o.ReportCount);
                return b;
            }
            catch (Exception e)
            {
                Singleton<TextLog>.Instance.Error(e, "CheckLimitPerMin");
                return false;
            }
        }
        /// <summary>
        /// 每个合约每个用户最多未成交单限制
        /// </summary>
        /// <param name="t"></param>
        /// <param name="o"></param>
        /// <returns></returns>
        public static bool CheckLimitPerContract(this Trader t, Order o)
        {
            try
            {
                if (o.IsBySystem) return true;
                if (t.Orders() == null || t.Orders().Count == 0) return true;
                var c = t.Orders().GetCountByContract(o.Contract);//.Where(a => a != null && a.Contract != null && a.Contract == o.Contract);
              
                if (c == 0) return true;
                return c < OrderPreHandler.CountPerContractLimit;
            }
            catch (Exception ex)
            {
                Singleton<TextLog>.Instance.Error(ex, "CheckLimitPerContract");
                return false;
            }
        }
    }

    public static class TraderORderExtension2
    {
        /// <summary>
        /// 是否有同方向的开平不同的单子
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static bool HasSameDirNotSameOrderType(this  Order o)
        {
            try
            {
                if (o.Trader.Orders() == null) return false;
                if (o.Trader.Orders().Count == 0) return false;
              
                var c = o.Trader.Orders().GetSameSameDirNotSameOrderTypeCount(o);
                return c > 0;
            }
            catch (Exception e)
            {
                Singleton<TextLog>.Instance.Error(e, "HasSameDirNotSameOrderType");
                //true表示不能下单
                return true;
            }
        }
    }
}

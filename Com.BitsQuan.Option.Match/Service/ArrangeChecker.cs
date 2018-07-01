using Com.BitsQuan.Option.Core;
using System;
using System.Linq;
using System.Timers;

namespace Com.BitsQuan.Option.Match
{
    /// <summary>
    /// 撮合定时检查器
    ///     解决队列中应撮合而未撮合委托的撮合问题
    /// </summary>
    public class ArrangeChecker : IDisposable
    {
        IMatcherDataContainer container;
        Action<Order> Handle;
        Timer t;
        TextLog log;
        public ArrangeChecker(IMatcherDataContainer container, Action<Order> match, TextLog log)
        {
            this.container = container;
            this.Handle = match;
            this.log = log;
            t = new Timer();
            t.Interval = 1000 * 5;
            t.Elapsed += t_Elapsed;
        }
        public void Start()
        {
            if (t != null)
                t.Start();
        }
        void Check()
        {
            if (container == null || Handle == null || container.Orders == null || container.Orders.Count == 0) return;
            var list = container.Orders.Values.ToList();
            foreach (var v in list)
            {
                if (v == null || v.SellQueue == null || v.SellQueue.Count == 0 || v.BuyQueue == null || v.BuyQueue.Count == 0) continue;
                var s = v.Sell1Price;
                var b = v.Buy1Price;
                if (s > b) continue;
                var s1 = v.SellQueue.OrderBy(a => a.Price).FirstOrDefault();
                if (s1 == null) continue;
                log.Info(string.Format("出现买1价大于卖1价-{0}-卖1价{1}-买1价{2}", s1.Contract.Code, s, b));
                Handle(s1);
            }
        }

        void t_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                t.Stop();
                Check();
            }
            catch (Exception ex)
            {
                log.Error(ex, "arrangeChecker");
            }
            finally
            {
                if (t != null) t.Start();
            }
        }

        public void Dispose()
        {
            if (t != null)
            {
                t.Dispose();
                t = null;
            }
        }
    }
}

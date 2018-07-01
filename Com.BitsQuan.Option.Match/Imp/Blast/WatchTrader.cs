using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Match.Imp;
using System;
using System.Timers;

namespace Com.BitsQuan.Option.Match
{ 
    /// <summary>
    /// 爆仓一旦开始就持续进行直到保证率满足要求
    /// </summary>
    public class WatchTrader : IDisposable
    {
        Trader t;
        Timer timer;
        Action<Trader> blasterAction;
        Action<Trader> clearAction;
        public WatchTrader(Trader t, Action<Trader> bact, Action<Trader> cact)
        {
            this.t = t;
            this.blasterAction = bact;
            clearAction = cact;
            timer = new Timer();
            timer.Interval = 5 * 1000;
            timer.Elapsed += timer_Elapsed;
            timer.Start();
        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                timer.Stop();
                var ratio = t.GetMaintainRatio(t.GetMarket());//获得保证率
                if (ratio < SysPrm.Instance.MonitorParams.BlastMaintainRatio)
                {
                    blasterAction(t);
                }
                else
                {
                    clearAction(t);
                    t.RepayToSystem();
                    t.SetMonitoring(false);
                    Dispose();
                }
            }
            catch (Exception ex)
            {
                Singleton<TextLog>.Instance.Error(ex, "watcher,将解除监视");
                t.SetMonitoring(false);
                this.Dispose();
            }
            finally
            {
                if (timer != null)
                    timer.Start();
            }
        }

        public event Action<Trader> OnStop;

        public void Dispose()
        {
            if (timer != null)
            {
                timer.Dispose();
                timer = null;
                if (OnStop != null) OnStop(this.t);
                
            }
        }
    }
}

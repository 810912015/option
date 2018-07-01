using Com.BitsQuan.Option.Core;
using System;
using System.Timers;

namespace Com.BitsQuan.Option.Match.Imp.Share
{
    /// <summary>
    /// 每天指定时刻执行的定时器
    /// </summary>
    public abstract class SpotTimer
    {
        TimeSpan when;
        Timer t;
        protected bool isFirst { get; private set; }
        public abstract void Execute();

        public SpotTimer(TimeSpan when)
        {
            isFirst = true;
            this.when = when;
            t = new Timer();
            var dtn=DateTime.Now;
            var dt = dtn.Date.Add(when);
            var tms = dt.Subtract(dtn).TotalMilliseconds;
            if (tms < 0)
            {
                dt=dtn.Date.AddDays(1).Add(when);
                tms = dt.Subtract(dtn).TotalMilliseconds;
            }
            t.Interval = tms;
            t.Elapsed += t_Elapsed;
            t.Start();
        }
        /// <summary>
        /// 只应该在测试时调用
        /// </summary>
        /// <param name="val"></param>
        public void SetIsFirst(bool val)
        {
            this.isFirst = val;
        }
        void t_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                t.Stop();
                Execute();
                if (isFirst)
                {
                    isFirst = false;
                    t.Interval = 1000 * 60 * 60 * 24;
                }
                
            }
            catch (Exception ex)
            {
                Singleton<TextLog>.Instance.Error(ex, "spot timer");
            }
            finally
            {
                t.Start();
            }
        }

    }
}

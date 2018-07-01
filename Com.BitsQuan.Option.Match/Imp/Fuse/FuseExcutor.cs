using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Imp;
using Com.BitsQuan.Option.Match.Imp;
using System;
using System.Timers;

namespace Com.BitsQuan.Option.Match
{
    /// <summary>
    /// 熔断执行器
    ///     功能:熔断结束后如果边界值没有变化,则上边界=原上边界*1.5,下边界=原下边界/2
    /// </summary>
    public class FuseExcutor : IDisposable
    {
        decimal startMax;
        decimal startMin;

        Func<decimal> GetMax;
        Func<decimal> GetMin;

        Action<decimal> SetMax;
        Action<decimal> SetMin;

       
        public static decimal RaiseRatio = 1.5m;
        public static decimal FallRatio = 1.5m;
        Timer t;

        Action<DateTime?> SetStartTimeAction;
        Action setBoundaryWhenOver;
        public bool isExecuting { get; private set; }

        public FuseExcutor(

            Func<decimal> getMax,
            Func<decimal> getMin,
            Action<decimal> setMax,
            Action<decimal> setMin, 
            Action<DateTime?> setStartTime,
             Action setBoundaryWhenOver
            )
        {
            this.GetMax = getMax; this.GetMin = getMin;
            this.SetMax = setMax; this.SetMin = setMin; 
            this.SetStartTimeAction = setStartTime;
            this.setBoundaryWhenOver = setBoundaryWhenOver;
            isExecuting = false;
            HasDealsWhenFusing = false;
        }
        FuseType? ftype;
        public void Start(decimal startMax,
            decimal startMin,FuseType ft)
        {
            if (isExecuting) return;
            HasDealsWhenFusing = false;
            isExecuting = true;
            this.ftype = ft;
            this.startMax = startMax; this.startMin = startMin;
            if (t == null)
            {
                t = new Timer();
                t.Interval =ContractFuse.FuseSpanInMin * 60 * 1000;
                t.Elapsed += t_Elapsed;
                SetStartTimeAction(DateTime.Now);
                t.Start();
            }

        }
        public event Action<decimal, decimal> OnFuseOver;
        public bool HasDealsWhenFusing { get; set; }
        void t_Elapsed(object sender, ElapsedEventArgs e)
        {
            t.Stop();


            if (!HasDealsWhenFusing)
            {
                if (ftype != null)
                {
                    if (ftype == FuseType.上涨熔断)
                    {
                        SetMax(startMax * RaiseRatio);
                    }
                    else
                    {
                        SetMin(startMin / FallRatio);
                    }
                }

            }
            else
            {
                setBoundaryWhenOver();
            }
            HasDealsWhenFusing = false;
           
            isExecuting = false;
            if (OnFuseOver != null)
            {
                var tmam = GetMax();
                var tmin = GetMin();
                foreach (var v in OnFuseOver.GetInvocationList())
                {
                    try
                    {
                        ((Action<decimal, decimal>)v).BeginInvoke(tmam, tmin,null,null);
                    }
                    catch (Exception ex)
                    {
                        Singleton<TextLog>.Instance.Error(ex, "fuse excutor");
                    }
                }
            }
            Dispose();
        }


        public void Dispose()
        {
            if (ftype != null) ftype = null;
            if (t != null)
            {
                t.Dispose();
                t = null;
            }
        }
    }
}

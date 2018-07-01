using System;
using System.Timers;

namespace Com.BitsQuan.Option.Core
{
    public class BtcPrice
    {
        static decimal current = 1000;
        public static decimal Current { get { return current; } }
        public static event Action<decimal> OnBtcPriceChanged;
        static Timer t;
        static bool isAutoUpdateBtcPrice = true;
        public static bool IsAutoUpdateBtcPrice
        {
            get { return isAutoUpdateBtcPrice; }
            set
            {
                if (isAutoUpdateBtcPrice == value) return;
                isAutoUpdateBtcPrice = value;
                Singleton<TextLog>.Instance.Info(string.Format("手动设置是否允许比特币价格自动更新,设置后状态:{0}", isAutoUpdateBtcPrice));
            }
        }
        public static void Init()
        {
            t = new Timer(10000);
            t.Elapsed += t_Elapsed;
            t_Elapsed(null, null);
        }
        static void t_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (!IsAutoUpdateBtcPrice) return;
                t.Stop();
                var our = BtcPriceGenerator.Our().Result;
                if (our != -1)
                {
                    current = our;
                    ExtremIn5.Put(current);
                    RaiseChanged(current);
                }
            }
            finally
            {
                t.Start();
            }
        }
        static void RaiseChanged(decimal c)
        {
            if (OnBtcPriceChanged == null) return;
            foreach (var v in OnBtcPriceChanged.GetInvocationList())
            {
                try
                {
                    ((Action<decimal>)v).BeginInvoke(c, null, null);
                }
                catch (Exception e)
                {
                    Singleton<TextLog>.Instance.Error(e, "BtcPriceChanged");
                }
            }
        }
        public static readonly ExtremIn5 ExtremIn5 = new ExtremIn5();
        public static void SetPrice(decimal c)
        {
            RaiseChanged(c);
            current = c;
            ExtremIn5.Put(current);
        }

    }
}

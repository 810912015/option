using Com.BitsQuan.Option.Core;

namespace Com.BitsQuan.Option.Match.Imp
{
    public static class TraderBailExtension
    {
        static Market market;
        /// <summary>
        /// 设置用户的市场信息
        ///     实际当前所有用户的市场信息都是同一个,所以只需设置一个即可
        /// </summary>
        /// <param name="t"></param>
        /// <param name="m"></param>
        public static void SetMarket(this Trader t, Market m)
        {
            if (market == null) market = m;
        }
        public static Market GetMarket(this Trader t)
        {
            return market;
        }
        /// <summary>
        /// 订阅保证金改变事件
        ///     当保证金金额改变时重新计算保证率
        /// </summary>
        /// <param name="t"></param>
        public static void InitBailEvent(this Trader t)
        {
            if (t.Id < 1) return;
            t.OnBailChanged += t_OnBailChanged;
        }
        static MyProperty<Trader, decimal> curMaintainRatio = new MyProperty<Trader, decimal>();
        static void t_OnBailChanged(Trader arg1, decimal arg2)
        {
            if (arg1.Id < 1) return;
            var old = curMaintainRatio.Get(arg1);
            var ratio = arg1.GetMaintainRatio(market);
            if (old != ratio)
            {
                curMaintainRatio.Set(arg1, ratio);
                arg1.RaiseRatioChanged(ratio);
            }
        }
        /// <summary>
        /// 下单后引发保证金改变事件
        /// </summary>
        /// <param name="arg1"></param>
        public static void RaiseRatioChangedAfterOrder(this Trader arg1)
        {
            t_OnBailChanged(arg1, 0);
        }
    }
}

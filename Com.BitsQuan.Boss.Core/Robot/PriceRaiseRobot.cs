using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Imp;
using System;

namespace Com.BitsQuan.Boss.Core
{
    /// <summary>
    /// 价格上限机器人:低于指定价格
    /// </summary>
    public class PriceRaiseRobot : SinglePriceRobot
    {
        public PriceRaiseRobot(int traderId,
           Func<int, int, TradeDirectType, OrderType, OrderPolicy, int, decimal, string, OrderResult> orderItFunc,
           decimal threshold)
            : base(traderId, orderItFunc, threshold) { }
        protected override bool ShouldEat(Order o)
        {
            var rp = ReasonablePrice(o.Contract);
            var r = o.Price > rp;
            return r;
        }

        decimal ReasonablePrice(Contract c)
        {
            var bp = BtcPrice.Current;
            var ep = c.ExcutePrice;
            decimal rp = 0;
            if (c.OptionType == OptionType.认购期权)
                rp = bp - ep;
            else rp = ep - bp;
            if (rp < 0) rp = 0;
            var r1 = rp + 50;
            var r2 = rp * 1.5m ;
            var r = Math.Max(r1, r2);
            return r;
        }
        
    }
}

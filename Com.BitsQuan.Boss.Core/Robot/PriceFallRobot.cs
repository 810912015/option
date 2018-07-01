using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Imp;
using System;

namespace Com.BitsQuan.Boss.Core
{
    /// <summary>
    /// 价格下限机器人:高于指定价格
    /// </summary>
    public class PriceFallRobot : SinglePriceRobot
    {
        public PriceFallRobot(int traderId,
           Func<int, int, TradeDirectType, OrderType, OrderPolicy, int, decimal, string, OrderResult> orderItFunc,
           decimal threshold)
            : base(traderId, orderItFunc, threshold) { }
        protected override bool ShouldEat(Order o)
        {
            return false;
            return o.Price < threshold;
        }
    }
}

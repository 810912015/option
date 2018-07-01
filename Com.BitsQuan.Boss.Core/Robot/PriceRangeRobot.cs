using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Imp;
using System;

namespace Com.BitsQuan.Boss.Core
{
    public class PriceRangeRobot : RobotBase
    {
        public decimal Min { get; private set; }
        public decimal Max { get; private set; }


        public PriceRangeRobot(int traderId,
            Func<int, int, TradeDirectType, OrderType, OrderPolicy, int, decimal, string, OrderResult> orderItFunc,
            decimal min,decimal max)
            : base(traderId, orderItFunc) {
                this.Max = max; this.Min = min;
        }
        protected override bool ShouldEat(Order o)
        {
            return o.Price > Min && o.Price < Max;
        }
    }
}

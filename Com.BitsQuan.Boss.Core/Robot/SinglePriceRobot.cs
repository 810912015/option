using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Imp;
using System;

namespace Com.BitsQuan.Boss.Core
{
    /// <summary>
    /// 价格边界机器人
    ///     当价格超出边界时给出吃单
    /// </summary>
    public abstract class SinglePriceRobot : RobotBase
    {

        protected decimal threshold;
        public decimal PriceThreshold { get { return threshold; } }
        protected SinglePriceRobot(int traderId,
            Func<int, int, TradeDirectType, OrderType, OrderPolicy, int, decimal, string, OrderResult> orderItFunc,
            decimal threshold)
            : base(traderId, orderItFunc)
        {
            this.threshold = threshold;
        }


    }
}

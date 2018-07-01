using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Imp;
using System;
using System.Threading;

namespace Com.BitsQuan.Boss.Core
{
    /// <summary>
    /// 机器人
    /// </summary>
    public abstract class RobotBase
    {
        public int TraderId { get; private set; }


        int UserOpId;
        protected int NewId()
        {
            if (UserOpId == int.MaxValue - 1)
            {
                Interlocked.Exchange(ref UserOpId, 0);
            }
            Interlocked.Increment(ref UserOpId);
            return UserOpId;

        }

        protected static readonly TextLog log = new TextLog("simplerobot.txt");
        protected abstract bool ShouldEat(Order o);
        protected RobotBase(int traderId,
            Func<int, int, TradeDirectType, OrderType, OrderPolicy, int, decimal, string, OrderResult> orderItFunc)
        {
            this.TraderId = traderId;
            this.OrderItFunc = orderItFunc;
            UserOpId = 0;
        }

        public virtual void EatOrder(Order o)
        {
            if (o.Direction == TradeDirectType.卖) return;
            if (o.Trader.Id == this.TraderId) return;
            if (o.State != OrderState.等待中 && o.State != OrderState.部分成交) return;
            if (!ShouldEat(o)) return;
            var oid = NewId();
            var r = OrderItFunc(this.TraderId,
                           o.Contract.Id,
                           TradeDirectType.卖 ,
                           OrderType.开仓,
                           OrderPolicy.市价FOK,
                           o.Count,
                           o.Price,
                           oid.ToString()
                           );
            var ls = string.Format("用户:{0},序号:{1},对手单:{2},本单:{3}", TraderId, oid, o.ToShortString(), r.ToString());

            log.Info(ls);
        }
        /// <summary>
        /// int who, int contract, TradeDirectType dir, OrderType orderType, OrderPolicy policy, int count, decimal price,string userOpId = ""
        /// </summary>
        protected Func<int, int, TradeDirectType, OrderType, OrderPolicy, int, decimal, string, OrderResult> OrderItFunc;
    }
}

using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Core.Spot;
using Com.BitsQuan.Option.Imp;
using System;
using System.Linq;

namespace Com.BitsQuan.Option.Match.Spot
{
    public class SpotOrderCreator
    {
        SpotModel sm;
        object loc = new object();
        public SpotOrderCreator(SpotModel sorder)
        {
            this.sm = sorder;
        }
        public SpotOrderResult Create(int trader, int coinId, Core.TradeDirectType dir,OrderPolicy policy,
            decimal userCount, decimal price)
        {
            var count = Math.Round(userCount, 2);
            if (count <= 0) return new SpotOrderResult
            {
                ResultCode = 1,
                Desc = "金额不能小于等于0",
                Spot = null
            };
            if (!policy.IsPriceValid(price))
                return new SpotOrderResult
                {
                    ResultCode = 1,
                    Desc = "非市价策略必须有价格",
                    Spot = null
                };
            var t = sm.Model.Traders.Where(a => a.Id == trader).FirstOrDefault();
            if (t == null) return new SpotOrderResult { Spot = null, Desc = "无此用户", ResultCode = 2 };
            var c = sm.Model.Coins.Where(a => a.Id == coinId).FirstOrDefault();
            if (c == null) return new SpotOrderResult { Spot = null, Desc = "无此虚拟币", ResultCode = 3 };
            var ap = Math.Round(price, 2);
            lock (loc)
            {
                var so = new SpotOrder
                {
                    Id = IdService<SpotOrder>.Instance.NewId(),
                    Coin = c,
                    CoinId = c.Id,
                    Count = count,
                    Direction = dir,
                    OrderTime = DateTime.Now,
                    Price = ap,
                    State = OrderState.等待中,
                    Trader = t,
                    TraderId = t.Id,
                    ReportCount = count,
                    OrderPolicy = policy
                };
                return new SpotOrderResult { ResultCode = 0, Desc = "创建委托成功", Spot = so };
            }
        }
    }
}

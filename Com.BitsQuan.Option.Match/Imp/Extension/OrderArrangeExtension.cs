using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Imp;
using System;
using System.Collections.Generic;
using System.Linq;
using Com.BitsQuan.Option.Match.Imp;

namespace Com.BitsQuan.Option.Match
{

    public static class OrderArrangeExtension
    {
        /// <summary>
        /// 是否可撮合
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static bool IsArrangable(this Order o)
        {
            return o.State == OrderState.等待中 || o.State == OrderState.部分成交;
        }

        static BooleanProperty<Order> arranging = new BooleanProperty<Order>();

        public static bool IsArranging(this Order o)
        {
            return arranging.Get(o);
        }
        public static void BeginArrange(this Order o)
        {
            arranging.Set(o, true);
        }
        public static void EndArrange(this Order o)
        {
            arranging.Set(o, false);
        }

        /// <summary>
        /// 订单策略是否是市价
        ///     如果是市价,则价格应当为0
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static bool IsMarketPrice(this Order o)
        {
            return o.OrderPolicy == OrderPolicy.市价FOK || o.OrderPolicy == OrderPolicy.市价IOC || o.OrderPolicy == OrderPolicy.市价剩余转限价;
        }

        public static decimal GetSellOpenCountToFreeze(this Order o)
        {
            if (o.Direction != TradeDirectType.买 || o.OrderType != OrderType.开仓) return 0m;

            sellOpenFrozen.Set(o.Id, o.Price);

            return o.Price * o.Count;
        }
        /// <summary>
        /// key:order,value:price to frozen
        /// </summary>
        static MyProperty<int, decimal> sellOpenFrozen = new MyProperty<int, decimal>();
        public static decimal GetSellOpenCountFrozen(this Order o)
        {
            var p = sellOpenFrozen.Get(o.Id);
            if (p == 0) return 0;
            var r = p * o.DoneCount;
            if (o.State == OrderState.已撤销)
            {
                r = p *(o.ReportCount -o.TotalDoneCount);
            }
            return r;
        }
        public static void ClearSellOpenCount(this Order o)
        {
            sellOpenFrozen.Clear(o.Id);
        }

        public static void Unfreeze(this Order o)
        {
            if (o == null || o.Sign == null) return;
            lock (o.Sign)
            {
                if (o.IsNeedBailExceptMaintainBail())
                {
                    var c = o.GetSellOpenCountFrozen();
                    if (c > 0)
                    {
                        TraderService.OperateAccount(o.Trader, c, AccountChangeType.保证金解冻, "system", o);
                    }
                    if (o.State == OrderState.已撤销 || o.State == OrderState.已成交 || o.State == OrderState.已行权)
                        o.ClearSellOpenCount();
                }
            }
        }
    }

}

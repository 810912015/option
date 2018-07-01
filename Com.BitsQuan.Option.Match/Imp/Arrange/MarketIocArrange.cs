using Com.BitsQuan.Option.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Com.BitsQuan.Option.Match
{
    /// <summary>
    /// 市价IOC：按对手方最优报价最大限度成交，不成交部分系统自动撤单。
    /// 市价IOC = 2,
    /// </summary>
    public class MarketIocArrange : Arrange
    {
        public MarketIocArrange(IMatcherDataContainer dc, Action<Order, Order, decimal> saveDeal, Action<Order, bool> matched, Action<Order, int> pmatched, Func<Order, bool> redo, Action<Order, int, bool> pmatchedTrue)
        {
            this.Container = dc; this.SaveDeal = saveDeal; this.Matched = matched; this.PartialMatched = pmatched; this.Redo = redo; this.PartialMatchedTrue = pmatchedTrue;
        }
        public override void Match(Order o)
        {
            var ll = Container.FindOpposite(o).FirstOrDefault();//.FindByDirAndPos(o);
            if (ll == null)
            {
                Redo(o);
                return;
            }
            var l = Container.FindOpposite(o).Where(d=>d.Price==ll.Price);
            var price= HandleCount(l, o);
            if (o.State != OrderState.已成交)
            {
                Redo(o);
            }
        }

        protected override void HandleFuse(Order main, Order slave, decimal boundaryPrice)
        {
            Redo(main);
        }
    }
}

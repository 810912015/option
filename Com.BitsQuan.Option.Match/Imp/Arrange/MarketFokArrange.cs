using Com.BitsQuan.Option.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using Com.BitsQuan.Option.Imp;

namespace Com.BitsQuan.Option.Match
{

    /// <summary>
    /// 市价FOK：市价申报后立即全部成交，否则系统自动撤单。
    /// 市价FOK = 5
    /// </summary>
    public class MarketFokArrange : Arrange
    {
        public MarketFokArrange(IMatcherDataContainer dc, Action<Order, Order, decimal> saveDeal, Action<Order, bool> matched, Action<Order, int> pmatched, Func<Order, bool> redo, Action<Order, int, bool> pmatchedTrue)
        {
            this.Container = dc; this.SaveDeal = saveDeal; this.Matched = matched; this.PartialMatched = pmatched; this.Redo = redo;this.PartialMatchedTrue = pmatchedTrue;
        }
        public override void Match(Order o)
        {
            
            var l = Container.FindByDirAndPos(o);
            if (l == null || l.Count == 0)
            {
                Redo(o);
                return;
            }
            var lc = l.Select(a => a.Count).Sum();
            if (lc < o.Count)
            {
                Redo(o);
                return;
            }
            if (o.Direction == TradeDirectType.买)
            {
                l = l.OrderBy(a => a.Price).ToList();
            }
            else
            {
                l = l.OrderByDescending(a => a.Price).ToList();
            }
            HandleCount(l, o); 
        }

        protected override void HandleFuse(Order main, Order slave, decimal boundaryPrice)
        {
            Redo(main);
        }
    }
}

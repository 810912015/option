using Com.BitsQuan.Option.Core;
using System;
using System.Linq;

namespace Com.BitsQuan.Option.Match
{

    /// <summary>
    /// 限价FOK：手工设定价格，申报后立即全部成交，否则系统自动撤单。
    /// 限价FOK = 4,
    /// </summary>
    public class PriceFokArrange : Arrange
    {
        public PriceFokArrange(IMatcherDataContainer dc, Action<Order, Order, decimal> saveDeal, Action<Order, bool> matched, Action<Order, int> pmatched, Func<Order, bool> redo, Action<Order, int, bool> pmatchedTrue)
        {
            this.Container = dc; this.SaveDeal = saveDeal; this.Matched = matched; this.PartialMatched = pmatched; this.Redo = redo; this.PartialMatchedTrue = pmatchedTrue;
        }
        public override void Match(Order o)
        {
            var r = Container.FindOppositeGFD(o);
            if (r.Count == 0)
            {
                Redo(o);
                return;
            }
            var rc = r.Select(a => a.Count).Sum();
            if (rc < o.Count)
            {
                Redo(o);
                return;
            }
            if (o.Direction == TradeDirectType.买)
            {
                r = r.OrderBy(a => a.Price).ToList();
            }
            else
            {
                r = r.OrderByDescending(a => a.Price).ToList();
            }
            HandleCount(r, o); 
        }

        protected override void HandleFuse(Order main, Order slave, decimal boundaryPrice)
        {
            Redo(main);
        }
    }
}

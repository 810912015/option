using Com.BitsQuan.Option.Core;
using System;

namespace Com.BitsQuan.Option.Match
{

    /// <summary>
    /// 限价GFD:手工设定价格，未成交部分一直等待直至撤单。
    /// 限价申报 = 1
    /// 最初的撮合程序:价格匹配,平仓,时间优先
    /// </summary>
    public class PriceArrange : Arrange
    {
        public PriceArrange(IMatcherDataContainer dc, Action<Order, Order, decimal> saveDeal, Action<Order, bool> matched, Action<Order, int> pmatched, Func<Order, bool> redo, Action<Order, int, bool> pmatchedTrue)
        {
            this.Container = dc; this.SaveDeal = saveDeal; this.Matched = matched; this.PartialMatched = pmatched; this.Redo = redo; this.PartialMatchedTrue = pmatchedTrue;
        }
        public override void Match(Order o)
        {
            try
            {
                var r = Container.FindOppositeGFD(o);
                HandleCount(r, o);
            }
            catch (Exception e)
            {
                Singleton<TextLog>.Instance.Error(e, "撮合异常");
            }
        }


        protected override void HandleFuse(Order main, Order slave, decimal boundaryPrice)
        {
            main.Price = boundaryPrice;
        }
    }
}

using Com.BitsQuan.Option.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using Com.BitsQuan.Option.Match.Imp;

namespace Com.BitsQuan.Option.Match
{
    public static class OrderListExtension
    {
        public static List<Order> SortForDir(this List<Order> l,TradeDirectType forDir)
        {
            if (l == null)return new List<Order>();
            if(l.Count() < 2) return l;
            if (forDir == TradeDirectType.买)
            {
                return l.OrderBy(a => a.Price).ToList();
            }
            else
            {
                return l.OrderByDescending(a => a.Price).ToList();
            }
        }
    }

    /// <summary>
    /// 市价剩余转限价：按对手方最优价格成交，未成交部分转限价GFD。
    /// 市价剩余转限价申报，指按市场可执行的最优价格买卖期权合约，
    /// 未成交部分按本方申报最新成交价格转为普通限价申报；
    /// 如该申报无成交的，按本方最优报价转为限价申报；如无本方申报的，该申报撤销。
    ///市价剩余转限价 = 3,
    /// </summary>
    public class MIocThenPriceArrange : Arrange
    {
        public MIocThenPriceArrange(IMatcherDataContainer dc, Action<Order, Order, decimal> saveDeal, Action<Order, bool> matched, Action<Order, int> pmatched, Func<Order, bool> redo, Action<Order, int, bool> pmatchedTrue)
        {
            this.Container = dc; this.SaveDeal = saveDeal; this.Matched = matched; this.PartialMatched = pmatched; this.Redo = redo;this.PartialMatchedTrue = pmatchedTrue;
        }
         decimal GetPrice(Order o)
        {
            var our = this.Container.GetByDir(o.Contract.Code, o.Direction);
            if (our != null&&our.Count>0)
            {
                var p = our.First().Price;
                return p;
            }
            else
            {
                var their= this.Container.GetByDir(o.Contract.Code, o.Direction == TradeDirectType.买 ? TradeDirectType.卖 : TradeDirectType.买);
                if (their != null && their.Count > 0)
                {
                    return their.First().Price;
                }
                else return -1;
            }
        }
        public override void Match(Order o)
        {
            var l = Container.FindByDirAndPos(o);
            if (l.Count == 0)
            {
                var p = GetPrice(o);
                if (p <=0)
                {
                    Redo(o); 
                }
                else
                {
                    o.Price = p;
                    o.OrderPolicy = OrderPolicy.限价申报;
                }
                return;
            }

            List<Order> ll = l.SortForDir(o.Direction);
            ll = ll.Where(a => a.Price == ll.FirstOrDefault().Price).ToList();
            var price = HandleCount(ll, o);
            if (o.State == OrderState.部分成交)
            {
                o.Price = o.DonePrice;
                o.OrderPolicy = OrderPolicy.限价申报;
            }
            else
            if (o.State != OrderState.已成交)
            {
                var p = GetPrice(o);
                if (p > 0)
                {
                    o.Price = price;
                    o.OrderPolicy = OrderPolicy.限价申报;
                }
                else
                {
                    Redo(o);
                }

            }
        }

       

        protected override void HandleFuse(Order main, Order slave, decimal boundaryPrice)
        {
            main.Price = boundaryPrice;
        }        
    }
}

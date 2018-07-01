using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Core.Spot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Com.BitsQuan.Option.Match.Spot
{
    /// <summary>
    /// 市价剩余转限价：按对手方最优价格成交，未成交部分转限价GFD。
    /// 
    ///市价剩余转限价 = 3,
    /// </summary>
    public class SpotMIocThenPrice : SpotArrange
    {
        public SpotMIocThenPrice(Func<SpotOrder,bool, List<SpotOrder>> FindPossibleMatch,
            SpotOrderContainer Container,
            SpotModel model,
            Action<SpotOrder, SpotOrder> MakeDeal, Func<SpotOrder, bool> Redo)
            : base(FindPossibleMatch, Container, model, MakeDeal,Redo) { }
        public override void Match(SpotOrder o)
        {
            var l = FindPossibleMatch(o,true);
            if (l.Count() == 0)
            {
                if (o.Direction == TradeDirectType.买)
                {
                    o.Direction = TradeDirectType.卖;
                    l = FindPossibleMatch(o, true);
                    if (l.Count() > 0)
                    {
                        var s = l.FirstOrDefault();
                        o.Price = s.Price;
                        o.Direction = TradeDirectType.买;
                        o.OrderPolicy = OrderPolicy.限价申报;

                        List<SpotOrder> d = new List<SpotOrder>();

                        HandleCount(o,d);
                    }
                }
                else
                {
                    o.Direction = TradeDirectType.买;
                    l = FindPossibleMatch(o, true);
                    if (l.Count() > 0)
                    {
                        var s = l.OrderBy(f => f.Price).FirstOrDefault();
                        o.Price = s.Price;
                        o.Direction = TradeDirectType.卖;
                        o.OrderPolicy = OrderPolicy.限价申报;

                        List<SpotOrder> d = new List<SpotOrder>();
                        HandleCount(o, d);
                    }
                }
                if (l.Count() == 0)
                {
                    Redo(o);
                }
                return;
            }
           List<SpotOrder> m = new List<SpotOrder>();
            m.Add(l[0]);//只取得所有对手单的第一个对手单对象（市场最优价）
            HandleCount(o, m);
            if (o.State == OrderState.部分成交)
            {
                //价格为最后成交价
                o.Price = o.DonePrice;

                o.OrderPolicy = OrderPolicy.限价申报;

            }
            else if (o.State == OrderState.等待中)
            {
                //价格为本方1价;本方没有报价,则撤销
                var v = Container.Get(o.Coin);
                var oo = o.Direction == TradeDirectType.买 ? v.BuyOrders : v.SellOrders;

                if (oo.Count == 1 && oo[0].Price == 0) {//如果本身只有一条（说明就是当前下单条），并且这条的下单金额为0
                    Redo(o);
                    return;
                }

                if (oo.Count == 0)
                {
                    Redo(o);
                }
                else
                {
                    o.Price = oo[0].Price;
                    o.OrderPolicy = OrderPolicy.限价申报;
                }
            }
        }
    }
}

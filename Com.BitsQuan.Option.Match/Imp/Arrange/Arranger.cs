using Com.BitsQuan.Option.Core;
using System;

namespace Com.BitsQuan.Option.Match
{
    public class Arranger
    {
        PriceArrange pa;
        MarketIocArrange mioc;
        MIocThenPriceArrange mpa;
        PriceFokArrange pfok;
        MarketFokArrange mfok;
        public Arranger(IMatcherDataContainer dc, Action<Order, Order,decimal> saveDeal, Action<Order,bool> matched, Action<Order, int> pmatched, Func<Order,bool> redo,Market m,Action<Order, int,bool> pmatchedTrue)
        {
            pa = new PriceArrange(dc, saveDeal, matched, pmatched, redo,pmatchedTrue);
            mioc = new MarketIocArrange(dc, saveDeal, matched, pmatched, redo, pmatchedTrue);
            mpa = new MIocThenPriceArrange(dc, saveDeal, matched, pmatched, redo, pmatchedTrue);
            pfok = new PriceFokArrange(dc, saveDeal, matched, pmatched, redo, pmatchedTrue);
            mfok = new MarketFokArrange(dc, saveDeal, matched, pmatched, redo, pmatchedTrue);
            pa.Market = mioc.Market = mpa.Market = pfok.Market = mfok.Market = m;
        }
        public void Match(Order o)
        {
            switch (o.OrderPolicy)
            {
                case OrderPolicy.限价申报:
                    pa.Match(o);
                    break;
                case OrderPolicy.限价FOK:
                    pfok.Match(o);
                    break;
                case OrderPolicy.市价剩余转限价:
                    mpa.Match(o);
                    break;
                case OrderPolicy.市价IOC:
                    mioc.Match(o);
                    break;
                case OrderPolicy.市价FOK:
                    mfok.Match(o);
                    break;
                default:
                    break;
            }
        }
    }
}

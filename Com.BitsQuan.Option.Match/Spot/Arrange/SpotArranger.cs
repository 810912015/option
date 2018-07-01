using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Core.Spot;
using System;
using System.Collections.Generic;

namespace Com.BitsQuan.Option.Match.Spot
{
    public class SpotArranger
    {
        public static readonly TextLog Log = new TextLog("spotArranger.txt");
        SpotPrice price;
        SpotPriceFok priceOk;
        SpotMarketFok marketFok;
        SpotMarketIoc marketIoc;
        SpotMIocThenPrice mtp;

        public SpotArranger(Func<SpotOrder,bool, List<SpotOrder>> FindPossibleMatch,
            SpotOrderContainer Container,
            SpotModel model,
            Action<SpotOrder, SpotOrder> MakeDeal, Func<SpotOrder, bool> Redo)
        {
            price = new SpotPrice(FindPossibleMatch, Container, model, MakeDeal,Redo);
            priceOk = new SpotPriceFok(FindPossibleMatch, Container, model, MakeDeal,Redo);
            marketFok = new SpotMarketFok(FindPossibleMatch, Container, model, MakeDeal,Redo);
            marketIoc = new SpotMarketIoc(FindPossibleMatch, Container, model, MakeDeal,Redo);
            mtp = new SpotMIocThenPrice(FindPossibleMatch, Container, model, MakeDeal,Redo);
        }

        public void Match(SpotOrder so)
        {
            if (so == null)
            {
                Log.Info("委托对象为空,不能撮合");
                return;
            }

            switch (so.OrderPolicy)
            {
                case OrderPolicy.限价申报:
                    price.Match(so);
                    break;
                case OrderPolicy.限价FOK:
                    priceOk.Match(so);
                    break;
                case OrderPolicy.市价剩余转限价:
                    mtp.Match(so);
                    break;
                case OrderPolicy.市价FOK:
                    marketFok.Match(so);
                    break;
                case OrderPolicy.市价IOC:
                    marketIoc.Match(so);
                    break;
                default:
                    Log.Info("交易策略有问题,不能撮合:{0}", so.ToString());
                    break;
            }
        }
    }
}

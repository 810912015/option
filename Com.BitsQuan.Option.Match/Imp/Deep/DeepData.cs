using Com.BitsQuan.Option.Core;
using System;
using System.Collections.Generic;

namespace Com.BitsQuan.Option.Match
{
    /// <summary>
    /// 市场深度数据
    /// </summary>
    public class DeepData
    {
        public string Sign { get; private set; }
        DeepLeftBuy buyLeft;
        DeepRightSell sellRight;
        public DeepData(string c)
            : this()
        {
            this.Sign = c;

        }
        public DeepData()
        {
            buyLeft = new DeepLeftBuy();
            sellRight = new DeepRightSell();
        }
        public void AddOrder(IOrder o)
        {
            if (o.Direction == TradeDirectType.买)
            {
                buyLeft.AddOrder(o);
            }

            else
            {
                sellRight.AddOrder(o);
            }
        }

        public void RemoveOrder(IOrder o)
        {
            if (o.Direction == TradeDirectType.买)
                buyLeft.RemoveOrder(o);
            else sellRight.RemoveOrder(o);
        }

        public void HandlePartial(IOrder o, int count)
        {
            if (o.Direction == TradeDirectType.买)
                buyLeft.HandlePartial(o, count);
            else sellRight.HandlePartial(o, count);
        }

        public List<List<List<decimal>>> List
        {
            get
            {
                //var r = new List<List<List<decimal>>> { buyLeft.List, sellRight.List };
                //return r;


                //deep图中间开(买卖各一半)
                //基本思路:取买卖到中间值的距离较小者作为距离
                var bmax = buyLeft.MaxPrice;
                var bmin = buyLeft.MinPrice;

                var smax = sellRight.MaxPrice;
                var smin = sellRight.MinPrice;

                var mid = (bmax + smin) / 2;

                var bd = mid - bmin;
                var sd = smax - mid;

                var delta = bd < sd ? bd : sd;

                var bboundary = mid - delta;
                var sboundary = mid + delta;

                var bl = buyLeft.GetList(bboundary);
                var sl = sellRight.GetList(sboundary);

                var r = new List<List<List<decimal>>> { bl, sl };
                return r;
            }
        }



    }
}

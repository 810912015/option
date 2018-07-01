using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Core.Spot;
using System;
using System.Collections.Generic;

namespace Com.BitsQuan.Option.Match.Spot
{
    public class SpotOrderContainer
    {
        Action<SpotOrder> OnFinish;
        public SpotOrderContainer(Action<SpotOrder> OnFinish)
        {
            this.OnFinish = OnFinish;
        }
        Dictionary<Coin, CoinOrderContainer> dic = new Dictionary<Coin, CoinOrderContainer>();
        public IEnumerable<CoinOrderContainer> Orders
        {
            get { return dic.Values; }
        }
        static object loc = new object();
        public void Add(SpotOrder so)
        {
            if (!dic.ContainsKey(so.Coin))
            {
                lock (loc)
                {
                    if (!dic.ContainsKey(so.Coin))
                    {
                        dic.Add(so.Coin, new CoinOrderContainer(so.Coin,OnFinish));
                    }
                }

            }
            dic[so.Coin].Add(so);
        }
        public bool Remove(SpotOrder so)
        {
            if (!dic.ContainsKey(so.Coin))
            {
                lock (loc)
                {
                    if (!dic.ContainsKey(so.Coin))
                    {
                        dic.Add(so.Coin, new CoinOrderContainer(so.Coin,OnFinish));
                    }
                }

            }
            return dic[so.Coin].Remove(so);
        }
        public CoinOrderContainer Get(Coin c)
        {
            if (!dic.ContainsKey(c))
            {
                lock (loc)
                {
                    if (!dic.ContainsKey(c))
                    {
                        dic.Add(c, new CoinOrderContainer(c,OnFinish));
                    }
                }

            }
            return dic[c];
        }
    }
}

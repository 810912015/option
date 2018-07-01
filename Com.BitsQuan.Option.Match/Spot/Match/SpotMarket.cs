using Com.BitsQuan.Option.Core;
using System.Collections.Generic;

namespace Com.BitsQuan.Option.Match.Spot
{ 
    public class SpotMarket
    {
        Dictionary<Coin, SpotMarketItem> dic;
        object loc;
        public SpotMarket()
        {
            dic = new Dictionary<Coin, SpotMarketItem>();
            loc = new object();
        }
        public SpotMarketItem Get(Coin c)
        {
            if (!dic.ContainsKey(c))
                lock (loc)
                {
                    if (!dic.ContainsKey(c))
                    dic.Add(c, new SpotMarketItem(c));
                }
            return dic[c];
        }
    }
}

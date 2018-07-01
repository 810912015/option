using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Core.Spot;
using System.Collections.Generic;

namespace Com.BitsQuan.Option.Match.Spot
{
    public class HotSpotDeals
    {
        public List<SpotDeal> Items { get; private set; }
        int max;
        object loc;
        public HotSpotDeals(int max=100)
        {
            this.max = max;
            Items = new List<SpotDeal>();
            loc = new object();
        }
        public void Put(SpotDeal sd)
        {
            var dc=Items.Count;
            if (dc > max)
            {
                int c = dc - max;
                lock (loc)
                {
                        Items.RemoveRange(0, c);
                }
            }
            Items.Add(sd);
        }
    }
    public class SpotMarketItem
    {
        public Coin Coin { get; private set; }

        public decimal NewestDealPrice { get;private set; }

        ExtremIn5 e24 = new ExtremIn5(24 * 60);
        public decimal Max24 { get { return e24.MaxIn5Min; } }
        public decimal Min24 { get { return e24.MinIn5Min; } }
        Past24Hour c24 = new Past24Hour();
        public decimal TotalCount { get { return c24.Total; } }
        public HotSpotDeals Deals { get; private set; }
        public SpotMarketItem(Coin c)
        {
            this.Coin = c;
            Deals = new HotSpotDeals();
        }

        public void HandleDeal(SpotDeal sd)
        {
            this.NewestDealPrice = sd.Price; 
            Deals.Put(sd);
            e24.Put(sd.Price);
            c24.Put(sd.Count);
        }
    }
}

using Com.BitsQuan.Option.Core;

namespace Com.BitsQuan.Option.Match.Spot
{ 
    public static class CoinPriceExtension
    {
        static MyProperty<Coin, decimal?> cp = new MyProperty<Coin, decimal?>();
        public static decimal GetCurPrice(this Coin c)
        {
            return cp.Get(c)??0;
        }
        public static void SetCurPrice(this Coin c, decimal? price)
        {
            cp.Set(c, price);
        }
    }
}

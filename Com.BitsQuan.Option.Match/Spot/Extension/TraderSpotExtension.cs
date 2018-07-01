using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Core.Spot;
using System.Collections.Generic;

namespace Com.BitsQuan.Option.Match.Spot
{
    public static class TraderSpotExtension
    {
        static MyProperty<Trader, List<SpotOrder>> sos = new MyProperty<Trader, List<SpotOrder>>();
        public static List<SpotOrder> GetSpotOrders(this Trader t)
        {
            var sl = sos.Get(t);
            if (sl == null)
            {
                sl = new List<SpotOrder>();
                sos.Set(t, sl);
            }
           // return sos.Get(t);
            return sl;
        }
        public static void AddSpotOrder(this Trader t, SpotOrder so)
        {
            var sl = t.GetSpotOrders();
            if (sl == null)
            {
                sl = new List<SpotOrder>();
                sos.Set(t, sl);
            }
            sl.Add(so);
        }
        public static bool RemoveSpotOrder(this Trader t, SpotOrder so)
        {
            var sl = t.GetSpotOrders();
            if (sl == null) return false;

            return sl.Remove(so);
        }

    }
}

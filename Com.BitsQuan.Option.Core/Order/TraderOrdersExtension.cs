using System;
using System.Collections.Generic;

namespace Com.BitsQuan.Option.Core
{
    public static class TraderOrdersExtension
    {
        static Dictionary<Trader, OrderBundle> tos = new Dictionary<Trader, OrderBundle>();
        public static OrderBundle Orders(this Trader t)
        {
            if (t == null) return new OrderBundle();
            if (!tos.ContainsKey(t))
            {
                lock (t)
                {
                    if (!tos.ContainsKey(t))
                    {
                        tos.Add(t, new OrderBundle());
                    }
                }
            }
            return tos[t];
        }
        public static int GetCloseOrderCount(this Trader t, Contract c, TradeDirectType dir)
        {
            try
            {
                return t.Orders().GetCloseCount(c, dir);
            }
            catch (Exception e)
            {
                Singleton<TextLog>.Instance.Error(e);
                return 0;
            }
        }
    }
}

using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Core.Spot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Com.BitsQuan.Option.Match.Spot
{
    public class CoinOrderContainer
    {
        Coin coin;
        List<SpotOrder> sellList;
        List<SpotOrder> buyList;
        object sl;
        object bl;

        OrderLifeManager<SpotOrder> olm;
        Action<SpotOrder> OnFinish;
        public CoinOrderContainer(Coin c, Action<SpotOrder> OnFinish)
        {
            this.OnFinish = OnFinish;
            coin = c;
            sellList = new List<SpotOrder>();
            buyList = new List<SpotOrder>();
            sl = new object();
            bl = new object();

            olm = new OrderLifeManager<SpotOrder>(sellList, buyList, sl, bl);
            olm.OnExpired += olm_OnExpired;
        }

        void olm_OnExpired(List<SpotOrder> obj)
        {
            foreach (var v in obj)
            {
                v.State = OrderState.已撤销;
                v.Trader.RemoveSpotOrder(v);
                v.UnFreeze();
                OnFinish(v);
            }
                
        }
        public static event Action<SpotOrder, int> OnOrderAddRemove;
        static void Raise(SpotOrder o, int op)
        {
            if(OnOrderAddRemove!=null)
                foreach (var v in OnOrderAddRemove.GetInvocationList())
                {
                    try
                    {
                        ((Action<SpotOrder, int>)v).BeginInvoke(o, op, null, null);
                    }
                    catch (Exception e)
                    {
                        Singleton<TextLog>.Instance.Error(e, "spot add remove");
                    }
                }
        }

        public void Add(SpotOrder so)
        {
            if (so.Coin != coin) return;
            if (so.Direction == TradeDirectType.卖)
            {
                lock (sl)
                    sellList.Add(so);
            }
            else
            {
                lock (bl)
                    buyList.Add(so);
            }
            Raise(so, 1);
        }
        public bool Remove(SpotOrder so)
        {
            if (so.Coin != coin) return false;
            bool r = false;
            if (so.Direction == TradeDirectType.卖)
            {
                lock (sl)
                    r = sellList.Remove(so);
            }
            else
            {
                lock (bl)
                    r = buyList.Remove(so);
            }
            if (r) Raise(so, 2);
            return r;
        }
        List<SpotOrder> Exp(Func<List<SpotOrder>> f)
        {
            try
            {
                return f();
            }
            catch (Exception e)
            {
                Singleton<TextLog>.Instance.Error(e, "coinordercontainer");
                return new List<SpotOrder>();
            }
        }
        public List<SpotOrder> FindOpposite(SpotOrder so,bool ignorePrice)
        {
            if (so.Direction == TradeDirectType.卖)
            {
                return Exp(() =>
                {
                    if (buyList.Count == 0) return new List<SpotOrder>();

                    List<SpotOrder> rl;
                    lock (bl)
                    {
                        rl = buyList.ToList();
                    }                    
                    if (rl == null || rl.Count == 0) return new List<SpotOrder>();
                    var r = rl.Where(a => (a.State == OrderState.部分成交 || a.State == OrderState.等待中));
                    if (!ignorePrice)
                        r = r.Where(a => a.Price >= so.Price);
                    if (r == null) return new List<SpotOrder>();
                    r = r.OrderByDescending(a => a.Price)
                     .ThenBy(a => a.OrderTime);
                    if (r == null) return new List<SpotOrder>();

                    return r.ToList();
                });

            }
            else
            {
                return Exp(() =>
                {
                    if (sellList.Count == 0) return new List<SpotOrder>();
                    List<SpotOrder> rl;
                    lock (sl)
                    {
                        rl = sellList.ToList();
                    }  
                    if (rl == null || rl.Count == 0) return new List<SpotOrder>();
                    var r = rl.Where(a => (a.State == OrderState.等待中 || a.State == OrderState.部分成交) );
                    if (!ignorePrice)
                        r = r.Where(a => a.Price <= so.Price);
                    if (r == null) return new List<SpotOrder>();
                    r = r.OrderBy(a => a.Price)
                     .ThenBy(a => a.OrderTime);
                    if (r == null) return new List<SpotOrder>();
                    return r.ToList();
                });

            }
        }
        public List<SpotOrder> SellOrders
        {
            get
            {
                return Exp(() =>
                {

                    if (sellList.Count == 0) return new List<SpotOrder>();
                    List<SpotOrder> rl;
                    lock (sl)
                    {
                        rl = sellList.ToList();
                    } 
                    if (rl == null||rl.Count==0) return new List<SpotOrder>();
                    var r = rl.Where(a => a.State == OrderState.等待中 || a.State == OrderState.部分成交);
                    if (r == null) return new List<SpotOrder>();
                    r = r.OrderBy(a => a.Price)
                    .ThenBy(a => a.OrderTime);
                    if (r == null) return new List<SpotOrder>();
                    return r.ToList();
                });

            }
        }
        public List<SpotOrder> BuyOrders
        {
            get
            {
                return Exp(() =>
                {
                    if (buyList.Count == 0) return new List<SpotOrder>();
                    List<SpotOrder> rl;
                    lock (bl)
                    {
                        rl = buyList.ToList();
                    }    
                    if (rl == null || rl.Count == 0) return new List<SpotOrder>();
                    var r = rl.Where(a => a.State == OrderState.部分成交 || a.State == OrderState.等待中);
                    if (r == null) return new List<SpotOrder>();
                    r = r.OrderByDescending(a => a.Price)
                    .ThenBy(a => a.OrderTime);
                    if (r == null) return new List<SpotOrder>();
                    return r.ToList();
                });

            }
        }
    }
}

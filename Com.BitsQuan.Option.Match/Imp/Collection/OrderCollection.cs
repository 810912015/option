using Com.BitsQuan.Option.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace Com.BitsQuan.Option.Match
{
    #region
    public class OrderPack : Com.BitsQuan.Option.Match.Imp.Collection.IOrderPack
    {
        public List<Order> list { get; private set; }
        public List<Order> Items
        {
            get
            {
                return GetOrders();
            }
        }
        List<Order> GetOrders()
        {
            lock (Sync)
            {
                if (IsSell)
                    return list.OrderBy(a => a.Price).ThenBy(a => a.OrderTime).ToList();
                else
                    return list.OrderByDescending(a => a.Price).ThenBy(a => a.OrderTime).ToList();
            }
        }
        public bool IsSell { get; private set; }
        public object Sync { get; private set; }

        public OrderPack(bool isSell)
        {
            this.IsSell = isSell;
            list = new List<Order>();
            Sync = new object();
        }
        public bool Add(Order o)
        {
            lock (Sync) list.Add( o);
            return true;
        }
        public bool Remove(Order o)
        {
            lock (Sync)
            {
                return list.Remove(o);
            }
        }
        public int Count
        {
            get { return list.Count; }
        }
        public Order FirstOrder
        {
            get {
                var os = GetOrders();
                if (os.Count > 0) return os[0];
                else return null;
            }
        }
        public decimal FirstPrice
        {
            get
            {
                var os = GetOrders();
                if (os.Count > 0) return os[0].Count;
                else return 0;
            }
        }
        public int FirstCount
        {
            get
            {
                var os = GetOrders();
                if (os.Count ==0) return 0;
                var q = os.Where(a => a.Price == os[0].Price).Sum(a => a.Count);
                return q;
            }
        }

    }

    public class OrderCollection2 : IOrderContainer
    {
        OrderPack sellPack;
        OrderPack buyPack;
        OrderLifeManager<Order> lifeMgr;
        Action<Order> OnFinish;
        public OrderCollection2(Action<Order> onFinish)
        {
            this.OnFinish = onFinish;
            sellPack = new OrderPack(true);
            buyPack = new OrderPack(false);
            lifeMgr = new OrderLifeManager<Order>(sellPack.list, buyPack.list, sellPack.Sync, buyPack.Sync);
            lifeMgr.OnExpired += lifeMgr_OnExpired;
        }

        void lifeMgr_OnExpired(List<Order> obj)
        {
            foreach (var v in obj)
            {
                v.State = OrderState.已撤销;
                v.Unfreeze();
                OnFinish(v);
                v.Trader.Orders().Remove(v);
            }
        }

        public List<Order> FindByDirAndPos(TradeDirectType dir, OrderType orderType)
        {
            if (dir == TradeDirectType.买)
                return sellPack.Items;
            else return buyPack.Items;
        }

        public int SellOrderCount
        {
            get { return sellPack.Count; }
        }

        public int BuyOrderCount
        {
            get { return buyPack.Count; }
        }

        public List<Order> SellQueue
        {
            get { return sellPack.Items; }
        }

        public List<Order> BuyQueue
        {
            get { return buyPack.Items; }
        }

        public decimal Buy1Price
        {
            get { return buyPack.FirstPrice; }
        }

        public decimal Sell1Price
        {
            get { return sellPack.FirstPrice; }
        }

        public int Buy1Count
        {
            get { return buyPack.FirstCount; }
        }

        public int Sell1Count
        {
            get { return sellPack.FirstCount; }
        }

        public bool Add(Order o)
        {
            if (o.Direction == TradeDirectType.买)
                return buyPack.Add(o);
            else return  sellPack.Add(o);
        }

        public bool Remove(Order o)
        {
            if (o.Direction == TradeDirectType.买)
                return buyPack.Remove(o);
            else return sellPack.Remove(o);
        }

        public List<Order> FindOpposite(Order o)
        {
            if (o.Direction == TradeDirectType.买)
                return sellPack.Items;
            else return buyPack.Items;
        }

        public List<Order> FindOppositeGFD(Order o)
        {
            return FindOpposite(o);
        }
    }
    #endregion
    /// <summary>
    /// 实现2个队列:其中一个是另一个的交易对象
    /// </summary>
    public class OrderCollection : IOrderContainer
    {
        Action<Order> OnFinish;

        /// <summary>
        /// 队列1:卖开,卖平队列
        /// </summary>
        List<Order> SellList;
        /// <summary>
        /// 队列2:买开,卖平队列
        /// </summary>
        List<Order> BuyList;
        object sellSync;
        object buySync;
        OrderLifeManager<Order> olm;
        public List<Order> SellQueue
        {
            get
            {
                try
                {
                    var r = SellList.ToList();
                    return r.Where(a => a != null).OrderByDescending(a => a.Price).ToList();
                }
                catch (Exception ex)
                {
                    Singleton<TextLog>.Instance.Error(ex, "sellqueure");
                    return new List<Order>();
                }
            }
        }
        
        public List<Order> BuyQueue
        {
            get
            {
                try
                {
                    var r = BuyList.ToList();
                    return r.Where(a => a != null).OrderByDescending(a => a.Price).ToList();
                }
                catch (Exception ex)
                {
                    Singleton<TextLog>.Instance.Error(ex, "buyqueue");
                    return new List<Order>();
                }
            }
        }

        public OrderCollection(Action<Order> OnFinish)
        {
            this.OnFinish = OnFinish;
            BuyList = new List<Order>();
            SellList = new List<Order>();
            buySync = new object();
            sellSync = new object();

            olm = new OrderLifeManager<Order>(BuyList, SellList, buySync, sellSync);
            olm.OnExpired += olm_OnExpired;
        }

        void olm_OnExpired(List<Order> obj)
        {
            foreach (var v in obj)
            {
                v.State = OrderState.已撤销;
                v.Unfreeze();
                OnFinish(v);
              
            }
        }
        public bool Add(Order o)
        {
            if (o.GetQueyeType() == 1)
            { 
                lock(sellSync)
                SellList.Add(o);
            }
            else
            { 
                lock(buySync)
                BuyList.Add(o);
            }
            return true;
        }
        public bool Remove(Order o)
        {
            if (o.GetQueyeType() == 1)
            { 
                lock(sellSync)
                return SellList.Remove(o);
            }

            else
            {
                lock(buySync)
                return BuyList.Remove(o);
            }
        }
        public List<Order> FindOpposite(Order o)
        {
            try
            {
                List<Order> r = null;
                if (o.GetQueyeType() == 1)
                {

                    lock (buySync)
                    {
                        r = BuyList.ToList();
                    }
                }
                else
                {
                    lock (sellSync)
                    {
                        r = SellList.ToList();
                    }

                }
                return r;
            }
            catch (Exception ex)
            {
                Singleton<TextLog>.Instance.Error(ex, "findopposite");
                return null;
            }
        }
        public List<Order> FindOppositeGFD(Order o)
        {
            return FindOpposite(o);
            //if (o.GetQueyeType() == 1) return BuyList.ToList();
            //else return SellList.ToList();
        }
        public List<Order> FindByDirAndPos(TradeDirectType dir, OrderType orderType)
        {
            try
            {
                if (dir == TradeDirectType.卖)
                    return BuyList.OrderByDescending(d => d.Price).ToList();
                else return SellList.ToList();
            }
            catch (Exception ex)
            {
                Singleton<TextLog>.Instance.Error(ex, "findbydirandpos");
                return null;
            }
        }
        
        /// <summary>
        /// 卖队列总单数
        /// </summary>
        public int SellOrderCount
        {
            get { return SellList.Count; }
        }
        /// <summary>
        /// 买队列总单数
        /// </summary>
        public int BuyOrderCount
        {
            get { return BuyList.Count; }
        }
         
        Order Sell1Order
        {
            get
            {
                try
                {
                    if (SellList.Count == 0) return null;
                    var t = SellList.FirstOrDefault();
                    return t;
                }
                catch (Exception ex)
                {
                    Singleton<TextLog>.Instance.Error(ex, "getsell1order");
                    return null;
                }
            }
        }
        Order Buy1Order
        {
            get
            {
                try
                {
                    if (BuyList.Count == 0) return null;
                    var t = BuyList.FirstOrDefault();
                    return t;
                }
                catch (Exception ex)
                {
                    Singleton<TextLog>.Instance.Error(ex, "getbuy1order");
                    return null;
                }
            }

        }
        public decimal Sell1Price
        {
            get
            {
                var s1 = Sell1Order;
                if (s1 == null) return 0;
                return s1.Price;
            }
        }
        public decimal Buy1Price
        {
            get
            {
                var b1 = Buy1Order;
                if (b1 == null) return 0;
                return b1.Price;
            }
        }
        public int Sell1Count
        {
            get
            {
                try
                {
                    var s1 = Sell1Order;
                    if (s1 == null) return 0;
                    var q = SellList.ToList().Where(a => a.Price == s1.Price).ToList();
                    if (q == null || q.Count() == 0) return 0;
                    return q.Select(a => a.Count).Sum();
                }
                catch (Exception ex)
                {
                    Singleton<TextLog>.Instance.Error(ex, "sell1count");
                    return 0;
                }
            }
        }
        public int Buy1Count
        {
            get
            {
                try
                {
                    var b1 = Buy1Order;
                    if (b1 == null) return 0;
                    var q = BuyList.ToList().Where(a => a.Price == b1.Price).ToList();
                    if (q == null || q.Count() == 0) return 0;
                    return q.Select(a => a.Count).Sum();
                }
                catch (Exception ex)
                {
                    Singleton<TextLog>.Instance.Error(ex, "buy1count");
                    return 0;
                }
            }
        }

    }
}

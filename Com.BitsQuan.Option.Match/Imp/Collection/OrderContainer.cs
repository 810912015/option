using Com.BitsQuan.Option.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Com.BitsQuan.Option.Match
{ 
    /// <summary>
    /// 撮合程序使用的委托容器
    /// </summary>
    public class OrderContainer : IMatcherDataContainer
    {
        Action<Order> OnFinish;
        Dictionary<string, IOrderContainer> dic;
        object loc = new object();
        public OrderContainer(Action<Order> OnFinish)
        {
            dic = new Dictionary<string, IOrderContainer>();
            this.OnFinish = OnFinish;
        }

        public Tuple<decimal, int, decimal, int> Get1PriceAndCount(string code)
        {
            decimal Buy1Price=0, Sell1Price=0;
            int Buy1Count=0, Sell1Count=0;
            if (dic.ContainsKey(code))
            {
                Buy1Price = dic[code].Buy1Price;
                Sell1Price = dic[code].Sell1Price;

                Buy1Count = dic[code].Buy1Count;
                Sell1Count = dic[code].Sell1Count;
            }
            return Tuple.Create(Buy1Price, Buy1Count, Sell1Price, Sell1Count);
        }

        public static event Action<Order, int> OnOrderAddRemoved;
        static void Raise(Order o, int op)
        {
            if (OnOrderAddRemoved != null)
            {
                foreach (var v in OnOrderAddRemoved.GetInvocationList())
                {
                    try
                    {
                        ((Action<Order, int>)v).BeginInvoke(o, op, null, null);
                    }
                    catch (Exception e)
                    {
                        Singleton<TextLog>.Instance.Error(e, "on order add remove");
                    }
                }
            }
        }

        public bool Add(Order o)
        {
            lock (loc)
            {
                if (!dic.ContainsKey(o.Contract.Code))
                {
                    dic.Add(o.Contract.Code, new OrderCollection(this.OnFinish));
                }
            }
            Raise(o, 1);
            return dic[o.Contract.Code].Add(o);
        }

        public bool Remove(Order o)
        {
            if (!dic.ContainsKey(o.Contract.Code)) return false;
            bool r = false;
            lock (loc)
            {
                r = dic[o.Contract.Code].Remove(o);
            }
            if (r)
            {
                Raise(o, 2);
            }
            return r;
        }
        List<Order> CheckAndSort(List<Order> l,Order o)
        {
            IEnumerable<Order> q;
            if (o.Direction == TradeDirectType.卖)//卖应该卖给出价最高的:orderbydescending
            {
                q = l.Where(a => a.Price >= o.Price)
                    .OrderByDescending(a => a.Price)//价格优先
                               .ThenBy(a => a.OrderType)//平仓优先
                               .ThenBy(a => a.OrderTime);//时间优先
            }
            else
            {
                q = l. //买应该买出价最低的:orderby
                OrderBy(a => a.Price)//价格优先
                               .ThenBy(a => a.OrderType)//平仓优先
                               .ThenBy(a => a.OrderTime);//时间优先
            }
            

            if (q == null) return new List<Order>();
            return q.ToList();
        }
        List<Order> CheckAndSortGFD(List<Order> l, Order o)
        {
            IEnumerable<Order> q;
            if (o.Direction == TradeDirectType.卖)//卖应该卖给出价最高的:orderbydescending
            {
                q = l.Where(a => a.Price >= o.Price)
                    .OrderByDescending(a => a.Price)//价格优先
                               .ThenBy(a => a.OrderType)//平仓优先
                               .ThenBy(a => a.OrderTime);//时间优先
            }
            else
            {
                q = l.Where(a => a.Price <= o.Price) //买应该买出价最低的:orderby
                .OrderBy(a => a.Price)//价格优先
                               .ThenBy(a => a.OrderType)//平仓优先
                               .ThenBy(a => a.OrderTime);//时间优先
            }


            if (q == null) return new List<Order>();
            return q.ToList();
        }
        public List<Order> FindOpposite(Order o)
        {
            if (!dic.ContainsKey(o.Contract.Code)) return new List<Order>();
            return CheckAndSort(dic[o.Contract.Code].FindOpposite(o), o);
        }
        public List<Order> FindOppositeGFD(Order o)
        {
            if (!dic.ContainsKey(o.Contract.Code)) return new List<Order>();
            return CheckAndSortGFD(dic[o.Contract.Code].FindOpposite(o), o);
        }
        public List<Order> GetByDir(string contractCode, TradeDirectType dir)
        {
            if (!dic.ContainsKey(contractCode)) return new List<Order>();
            if (dir == TradeDirectType.卖)
                return dic[contractCode].SellQueue;
            else return dic[contractCode].BuyQueue;
        }
        public List<Order> FindByDirAndPos(string contractCode,TradeDirectType dir, OrderType orderType)
        {
            if (!dic.ContainsKey(contractCode)) return new List<Order>();
            return dic[contractCode].FindByDirAndPos(dir, orderType);
        }
        public List<Order> FindByDirAndPos(Order o)
        {
            return FindByDirAndPos(o.Contract.Code, o.Direction, o.OrderType);
        }
        /// <summary>
        /// 所有委托字典
        /// </summary>
        public Dictionary<string, IOrderContainer> Orders
        {
            get { return dic; }
        }



        public Order GetOrderById(int orderId)
        {
            Order o = null;
            foreach (var v in dic.Values)
            {
                var q = v.BuyQueue.Where(a => a.Id == orderId).FirstOrDefault();
                if (q != null)
                {
                    o = q; break;
                }
                var q1 = v.SellQueue.Where(a => a.Id == orderId).FirstOrDefault();
                if (q1 != null)
                {
                    o = q; break;
                }

            }
            return o;
        }
    }

   
}

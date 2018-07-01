using System.Collections.Generic;
using System.Linq;

namespace Com.BitsQuan.Option.Core
{
    /// <summary>
    /// 每个用户的orders列表
    /// </summary>
    public class OrderBundle
    {
        List<Order> list { get; set; }
        public int Count
        {
            get { return list.Count; }
        }
        public IEnumerable<Order> Items
        {
            get
            {
                lock (Sync)
                {
                    return list.ToList();
                }
            }
        }

        object Sync { get; set; }

        public OrderBundle()
        {
            list = new List<Order>();
            Sync = new object();
        }
        public int GetCloseCount(Contract c, TradeDirectType dir)
        {
            lock (Sync)
            {
                if (Count == 0) return 0;
                var os = list.Where(a => a != null
                    && (a.State == OrderState.部分成交 || a.State == OrderState.等待中)
                    && a.Contract == c
                    && a.Direction == dir && a.OrderType == OrderType.平仓).Select(a => a.Count).Sum();
                return os;
            }
        }
        public int GetCloseCount(string ccode, TradeDirectType dir)
        {
            lock (Sync)
            {
                if (Count == 0) return 0;
                var os = list.Where(a => a != null
                    && (a.State == OrderState.部分成交 || a.State == OrderState.等待中)
                    && a.Contract.Code == ccode
                    && a.Direction == dir && a.OrderType == OrderType.平仓).Select(a => a.Count).Sum();
                return os;
            }
        }
        public void Add(Order o)
        {
            lock (Sync)
                list.Add(o);
        }
        public int GetCountByContract(Contract c)
        {
            lock (Sync)
            {
                var r = list.Where(a => a != null && a.Contract != null && a.Contract == c);
                if (r == null) return 0;
                return r.Count();
            }
        }

        public int GetSameSameDirNotSameOrderTypeCount(Order o)
        {
            lock (Sync)
            {
                var q = list
                    .Where(a => a != null
                    && a.Contract != null
                    && a.Contract.Id == o.Contract.Id
                    && a.Direction == o.Direction
                    && a.OrderType != o.OrderType
                    && (a.State == OrderState.等待中 || a.State == OrderState.部分成交)
                    );
                if (q == null) return 0;
                var c = q.Count();
                return c;
            }
        }
        public void Remove(Order o)
        {
            lock (Sync)
                list.Remove(o);
        }
        public List<Order> GetByDirAndOrderType(TradeDirectType dir, OrderType type)
        {
            lock (Sync)
            {
                var q = list.Where(a => a.Direction == dir && a.OrderType == type);
                if (q == null) return new List<Order>();
                return q.ToList();
            }
        }
        public List<Order> GetLives()
        {
            lock (Sync)
            {
                var q = list.Where(v => (v.State == OrderState.等待中 || v.State == OrderState.部分成交) && v.Contract.IsNotInUse == false);
                if (q == null) return new List<Order>();
                return q.ToList();
            }
        }
        public List<Order> GetLivesByContractId(int contractId)
        {
            lock (Sync)
            {
                var q = list.Where(a => (a.State == OrderState.等待中 || a.State == OrderState.部分成交) && a.Contract.Id == contractId);
                if (q == null) return new List<Order>();
                return q.ToList();
            }
        }
        public Order GetById(int oid)
        {
            lock (Sync)
            {
                return list.Where(a => a.Id == oid).FirstOrDefault();
            }
        }

    }

  }

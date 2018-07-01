using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Match.Imp;
using Com.BitsQuan.Option.Match.Spot;
using System.Collections.Generic;
using System.Linq;

namespace Com.BitsQuan.Option.Match
{
    public class DeepDataPool
    {
        Dictionary<string, DeepData> dic;
        object loc;
        public DeepDataPool(IEnumerable<Contract> cs)
        {
            dic = new Dictionary<string, DeepData>();
            loc = new object();
            foreach (var v in cs)
            {
                if (!dic.ContainsKey(v.Name))
                {
                    dic.Add(v.Name, new DeepData(v.Name));
                }
            }
        }
        void AddIfNot(IOrder o)
        {
            if (!dic.ContainsKey(o.Sign))
            {
                lock (loc)
                {
                    if (!dic.ContainsKey(o.Sign))
                        dic.Add(o.Sign, new DeepData(o.Sign));
                }

            }

        }
        public void AddOrder(IOrder o)
        {
            AddIfNot(o); dic[o.Sign].AddOrder(o);
        }
        public void RemoveOrder(IOrder o)
        {
            AddIfNot(o); dic[o.Sign].RemoveOrder(o);
        }
        public void HandlePartial(IOrder o, int count)
        {
            AddIfNot(o); dic[o.Sign].HandlePartial(o, count);
        }
        public DeepData Get(string contractName)
        {
            if (contractName == null) return new DeepData();
            if (!dic.ContainsKey(contractName)) return new DeepData();
            return dic[contractName];
        }
    }

    public class DeepDataPool2
    {
        IOptionModel model;
        IMatcherDataContainer container;
        public DeepDataPool2(IOptionModel model,
        IMatcherDataContainer container)
        {
            this.model = model;
            this.container = container;
        }

        public DeepData Get(string contractName)
        {
            var contract = model.Contracts.Where(a => a.Name == contractName).FirstOrDefault();
            if (contract == null) return new DeepData();
            if (!container.Orders.ContainsKey(contract.Code)) return new DeepData();

            var c = container.Orders[contract.Code];
            var r = new DeepData();
            var s = c.SellQueue.ToList();
            var b = c.BuyQueue.ToList();
            foreach (var v in s)
            {
                r.AddOrder(v);
            }
            foreach (var v in b)
            {
                r.AddOrder(v);
            }
            return r;
        }
    }

    public class DeepDataPool3
    {
        
        CoinOrderContainer container;
        public DeepDataPool3(
        CoinOrderContainer container)
        { 
            this.container = container;
        }

        public DeepData Get(string contractName)
        { 
             
            var r = new DeepData();
            var s = container.SellOrders.ToList();
            var b = container.BuyOrders.ToList();
            foreach (var v in s)
            {
                r.AddOrder(v);
            }
            foreach (var v in b)
            {
                r.AddOrder(v);
            }
            return r;
        }
    }
}

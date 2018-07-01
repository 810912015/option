using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Match.Imp;
using System.Collections.Generic;

namespace Com.BitsQuan.Option.Match
{ 
    public class LegacyOrders
    {
        public List<Order> Items { get; private set; }
        object sync;
      
        public LegacyOrders()
        {
            Items = new List<Order>();
            sync = new object();
            
        }
        public void Add(Order o)
        {
            lock (sync)
                Items.Add(o);
        }
        public bool ShouldSave(Order o)
        {
            if (Items.Count == 0) return true;
            if (Items.Contains(o))
            {
                lock (sync)
                    Items.Remove(o);
                return false;
            }
            return true;
        }
    }
}

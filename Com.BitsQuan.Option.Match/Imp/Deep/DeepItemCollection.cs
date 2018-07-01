using Com.BitsQuan.Option.Core;
using System.Collections.Generic;
using System.Linq;

namespace Com.BitsQuan.Option.Match
{

    /// <summary>
    /// 深度数据集合
    ///  插入时:
    ///      插入的累计数量等于之前的数量,之后的数量加上插入数量
    /// 移除和更新时:
    ///      之后的数量加上当前数量
    /// </summary>
    public abstract class DeepItemCollection
    {
        protected SortedDictionary<decimal, DeepItem> dic=new SortedDictionary<decimal,DeepItem> ();
        public List<List<decimal>> List
        {
            get
            {
                return dic.Values.Select(a => a.List).ToList();
            }
        }
        public decimal MaxPrice { get { if (dic.Count == 0) return 0; return dic.Last().Key; } }
        public decimal MinPrice { get { if (dic.Count == 0) return 0; return dic.First().Key; } }
        /// <summary>
        /// 查找需要改变总数的item,不包括自身
        /// </summary>
        /// <returns></returns>
        protected abstract IEnumerable<DeepItem> GetNeedAddItems(decimal price);
        protected abstract DeepItem GetPre(decimal price);
        public abstract List<List<decimal>> GetList(decimal boundary);
        object loc = new object();
        public void AddOrder(IOrder o)
        {
            lock (loc)
            {
                if (!dic.ContainsKey(o.Price))
                {
                    var pre = GetPre(o.Price);
                    decimal total = o.OrderCount;
                    if (pre != null)
                        total += pre.Total;
                    DeepItem di = new DeepItem { Price = o.Price, Count = o.OrderCount, Total = total };
                    dic.Add(di.Price, di);
                }
                var q = GetNeedAddItems(o.Price);
                foreach (var v in q)
                {
                    v.Total += o.OrderCount;
                }
            }
        }
        public void RemoveOrder(IOrder o)
        {
            lock (loc)
            {
                dic.Remove(o.Price);
                var q = GetNeedAddItems(o.Price);
                foreach (var v in q)
                {
                    if(v.Total>=o.OrderCount)
                    v.Total -= o.OrderCount;
                }
            }
        }

        public void HandlePartial(IOrder o, decimal partialCount)
        {
            if (dic.ContainsKey(o.Price))
            {
                dic[o.Price].Count -= partialCount;
                dic[o.Price].Total -= partialCount;
                var q = GetNeedAddItems(o.Price);
                foreach (var v in q)
                    v.Total -= partialCount;
            }
        }
    }
}

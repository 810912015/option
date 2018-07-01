using Com.BitsQuan.Option.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Com.BitsQuan.Option.Match
{
    public class DeepLeftBuy : DeepItemCollection
    {
       

        public override List<List<decimal>> GetList(decimal boundary)
        { 
                try
                {
                    if (dic.Count == 0) return new List<List<decimal>>();
                    return dic.Values.ToList().Where(a=>a.Count>0&&a.Price>=boundary).Select(a => a.List).ToList();
                }
                catch (Exception e)
                {
                    Singleton<TextLog>.Instance.Error(e, "DeepLeftBuy");
                    return new List<List<decimal>>();
                } 
        }
        protected override IEnumerable<DeepItem> GetNeedAddItems(decimal price)
        {
            if (dic == null || dic.Count == 0) return new List<DeepItem>();
            var d = dic.Values.ToList();
            if (d == null) return new List<DeepItem>();
            var q = d.Where(a => a.Price < price);
            if (q == null) return new List<DeepItem>();
            else return q.Reverse().ToList();
        }

        protected override DeepItem GetPre(decimal price)
        {
            if (dic == null || dic.Count == 0) return null;
            var d = dic.Values.ToList();
            if (d == null || d.Count == 0) return null;
            return d.Where(a => a.Price > price).FirstOrDefault();
        }
    }
}

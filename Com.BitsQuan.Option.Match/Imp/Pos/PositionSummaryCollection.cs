using Com.BitsQuan.Option.Core;
using System.Collections.Generic;
using System.Linq;

namespace Com.BitsQuan.Option.Match.Imp
{
    public class PositionSummaryCollection
    {
        Dictionary<string, PositionSummary> dic;
        object calcSync = new object();
        public PositionSummaryCollection() { dic = new Dictionary<string, PositionSummary>(); }

        public void Add(PositionSummaryData d)
        {
            if (!dic.ContainsKey(d.Contract.Code))
            {
                dic.Add(d.Contract.Code, new PositionSummary(d));
            }
        }

        public PositionSummary RemoveByCode(string code)
        {
            lock (calcSync)
            {
                if (code == null || !dic.ContainsKey(code)) return null;
                var d = dic[code];
                dic.Remove(code);
                return d;
            }            
        }

        public List<PositionSummary> GetByType(PositionType pt)
        {
            if (dic == null || dic.Count == 0) return new List<PositionSummary>();
            var k = ((int)pt).ToString();
            var q = dic.Values.Where(a=>a.PositionType==pt.ToString()).ToList();//.Where(a => a.Key.EndsWith(k)).Select(a=>a.Value).ToList();
            return q;
        }
       
        public void Calc(UserPosition up, bool isAdd, decimal curPrice)
        {
            var k = up.Order.Contract.Code;
            PositionSummary ps = null;
            lock (calcSync)
            {
                if (!dic.ContainsKey(k))
                {

                    ps = new PositionSummary(up, curPrice);
                    dic.Add(k, ps);
                }
                else
                {
                    ps = dic[k];
                    dic[k].Update(up, isAdd, curPrice);
                }
            }
        }
        public bool Contains(Contract c)
        {
            if (c == null||dic==null||dic.Count==0) return false;
            return dic.ContainsKey(c.Code);
        }
        public PositionSummary Get(string code, PositionType type)
        {
            if (code == null) return null;
            if (dic.ContainsKey(code))
            {
                return dic[code];
            }
            else return null;
        }
        public List<PositionSummary> All { get { return dic.Values.Where(a => a.Count > 0).ToList(); } }

        public decimal GetTotalMaintain(Market m)
        {
            var d = dic.Values.ToList();
            var q = d.Where(s=>s.Contract.IsNotInUse==false).Select(a => a.GetMaintain(m.GetNewestPrice(a.CName))).Sum();
            return q;
        }
    }
}

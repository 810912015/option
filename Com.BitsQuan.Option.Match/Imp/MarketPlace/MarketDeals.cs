using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Match.Imp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Com.BitsQuan.Option.Match
{
    public class MarketDeals
    {
        Dictionary<string, List<Deal>> dic;
        IOptionModel model;
        public List<Deal> GetDealsByContract(string contractName)
        {
            if (null == contractName) return null;
            if (!dic.ContainsKey(contractName)) return null;
            return dic[contractName];
             
        }
        public MarketDeals(IOptionModel model)
        {
            this.model = model;
            dic = new Dictionary<string, List<Deal>>();
        }
        void Check(List<Deal> l)
        {
            if (l.Count > 100)
            {
                var c = l.Count - 100;
                l.RemoveRange(0, c); 
            }
        }
        Contract GetContractForDeal(Deal d)
        {
            var q = model.Contracts.Where(a => a.Id == d.ContractId&&a.IsDel==false).FirstOrDefault();
            return q;
        }
        public void Handle(Deal d)
        {
            try
            {
                var c = GetContractForDeal(d);

                if (dic.ContainsKey(c.Name))
                {
                    var l = dic[c.Name];
                    Check(l);
                    l.Add(d);
                }
                else
                {
                    dic.Add(c.Name, new List<Deal> { d });
                }
            }
            catch (Exception e)
            {
                Singleton<TextLog>.Instance.Error(e, "market deals handle");
            }
        }
    }

}

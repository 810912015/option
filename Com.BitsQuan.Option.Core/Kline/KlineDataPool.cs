using System;
using System.Collections.Generic;

namespace Com.BitsQuan.Option.Core
{
    /// <summary>
    /// 所有合约的k线图数据缓存
    /// </summary>
    public class KlineDataPool
    {
        Dictionary<string, KlineData> dic;
        Func<int, Contract> GetContractById;
        object loc = new object();
        public KlineDataPool(Func<int, Contract> GetContractById)
        {
            dic = new Dictionary<string, KlineData>();
            this.GetContractById = GetContractById;
        }
        public void Add(Ohlc o)
        {
            var contract = GetContractById(o.WhatId);
            if (contract == null) return;
            if (o == null) return;
            lock (loc)
            {
                if (!dic.ContainsKey(contract.Code))
                {
                    dic.Add(contract.Code, new KlineData(contract.Code, contract.Name));
                }
            }

            dic[contract.Code].Add(o);
        }
        public KlineData GetByConctractCode(string code)
        {
            if (code == null) return null;
            if (!dic.ContainsKey(code)) return null;
            return dic[code];
        }
        
    }
}

using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Imp;
using Com.BitsQuan.Option.Match.Imp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Com.BitsQuan.Option.Match
{
    public static class PosExtension2
    {
        public static decimal GetReleasePerPos(this PositionSummary up,Func<string,Contract>GetContractByCode, decimal curPrice)
        {
            if (up.PositionType == "义务仓")
            {
                var c = GetContractByCode(up.CCode);
                if (c == null) return 0;
                var one = c.GetMaintainForContract(curPrice);
                var delta = one - curPrice;
                return delta;
            }
            else
            {
                return curPrice;
            }
        }

        public static Contract GetContract(this PositionSummary p, IOptionModel model)
        {
            return model.Contracts.Where(a => a.Code == p.CCode).FirstOrDefault();
        }

        static BooleanProperty<string> sellDic = new BooleanProperty<string>();
        static string MakeKey(Trader t, PositionSummary p)
        {
            return string.Format("{0}-{1}", t.Id, p.CCode);
        }
        public static bool IsSelling(this Trader t, PositionSummary p)
        {
            var k = MakeKey(t, p);
            return sellDic.Get(k);
        }
        public static void SetSell(this Trader t, PositionSummary p, bool isSelling)
        {
            var k = MakeKey(t, p);
            sellDic.Set(k, isSelling);
        }
        public static void ClearAllSelling(this Trader t)
        {
            var id = t.Id.ToString();
            var q = sellDic.Items.Where(a => a.StartsWith(id));
            if (q == null) return;
            var ql = q.ToList();
            foreach (var v in ql)
            {
                sellDic.Set(v, false);
            }
        }
    } 
}

using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Imp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Com.BitsQuan.Option.Match.Imp;
using System.Text;
using System.Threading.Tasks;

namespace Com.BitsQuan.Option.Match
{
    /// <summary>
    /// 行权记录处理器
    /// </summary>
    public class CExeRecHandler
    {
        public ContractExecuteRecord CreateRecord(Trader t, Contract c, PositionType posType, int count, decimal basePrice, bool isAddTo, decimal total)
        {
            return new ContractExecuteRecord
            {
                Id         = IdService<ContractExecuteRecord>.Instance.NewId(),
                Contract   = c,
                ContractId = c.Id,
                Trader     = t,
                TraderId   = t.Id,
                Count      = count,
                BasePrice  = basePrice,
                IsAddTo    = isAddTo,
                PosType    = posType,
                Total      = total,
                When       = DateTime.Now
            };
        }

        ContractExeSaver ces = new ContractExeSaver();
        public void SaveRecord(Trader t, Contract c, PositionType posType, int count, decimal basePrice, bool isAddTo, decimal total)
        {
            var r = CreateRecord(t, c, posType, count, basePrice, isAddTo, total);
            ces.Save(r);
        }

        public void Flush()
        {
            ces.Flush();
        }


        /// <summary>
        /// 更新委托状态为已行权
        /// </summary>
        /// <param name="orderIds"></param>
        public void UpdateOrderStateToExecuted(List<int> orderIds)
        {
            if (orderIds == null || orderIds.Count == 0) return;
            Task.Factory.StartNew(() =>
            {
                try
                {
                    string f         = "update Orders set State =5,price =0 where Id in ({0}) ";
                    StringBuilder sb = new StringBuilder();
                    foreach (var v in orderIds)
                    {
                        sb.AppendFormat("{0},", v);
                    }
                    sb.Remove(sb.Length - 1, 1);
                    string s = string.Format(f, sb.ToString());
                    using (DBServer db = new DBServer())
                    {
                        db.ExecNonQuery(s);
                    }
                }
                catch (Exception e)
                {
                    Singleton<TextLog>.Instance.Error(e, "update order state for execute");
                }
            });
        }
    }
}

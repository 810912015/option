using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Match.Imp;
using System;
using System.Linq;

namespace Com.BitsQuan.Option.Match
{  
    /// <summary>
    /// 爆仓执行:计算数量,金额,调用下单器,含爆仓事件
    /// </summary>
    public class BlastExecutor
    {
        Market Market;
        decimal threshold { get { return SysPrm.Instance.MonitorParams.BlastMaintainRatio; } }
        Func<Trader, decimal,bool, BlastRecord> CreateRecord; 
        Action<BlastRecord> OnBlasting;
        Action<BlastRecord> OnBlasted;
        PositionType positionType;
        TextLog Log;
        BlastSellor sellor;
        Func<Trader, PositionSummary,decimal, decimal> CalRatio;
        ReorderCollection fehc;
        Func<string, Contract> getContractByCode;

        public BlastExecutor(Market Market,
        Func<Trader, decimal,bool, BlastRecord> CreateRecord, 
          Action<BlastRecord> OnBlasting,
         Action<BlastRecord> OnBlasted,
         PositionType positionType,
         TextLog Log,
         BlastSellor sellor,
         Func<Trader,PositionSummary, decimal,decimal> CalRatio, ReorderCollection fehc, Func<string, Contract> getContractByCode)
        {
            this.getContractByCode = getContractByCode;
            this.Market = Market; 
            this.CreateRecord = CreateRecord; 
            this.OnBlasted = OnBlasted;
            this.OnBlasting = OnBlasting;
            this.positionType = positionType;
            this.Log = Log;
            this.sellor = sellor;
            this.CalRatio = CalRatio;
            this.fehc = fehc;
        }
        

       /// <summary>
       /// 爆仓
       /// </summary>
        /// <param name="t">要爆仓的用户</param>
        /// <returns>true表示保证率满足要求,false表示保证率不满足要求</returns>
        public bool Blast(Trader t)
        {
            //Id小于1的用户是系统机器人
            if (t.Id < 1) return true;
            var mr = t.GetMaintainRatio(Market);
            if (mr >= threshold)
            {
                return true;
            }
            var br = CreateRecord(t, mr,true);
            if (OnBlasting != null)
            {
                if (t.ShouldMsg())
                {
                   OnBlasting(br);
                }
            }
                var ps = t.GetPositionSummaries();

                //所有持仓都平了:虽然不一定成交
                if (ps == null || ps.Count() == 0) return false;

                var bail = t.GetMaintain(Market);
                var needed = bail*SysPrm.Instance.MonitorParams.BlastOnceRatio;// / 10m;
                if (needed < 500m) needed = 500m;
                foreach (var up in ps)
                {
                    if (up == null || up.Count == 0) continue;
                    if (t.IsSelling(up)) continue;
                    var buyRatio = CalRatio(t,up, needed);
                    if (buyRatio == 0m) continue;
                    Log.Info(string.Format("{0}爆仓开始:{1}-{2}-仓{3}-平仓份数{4}-需{5}",
                      this.positionType, t.Name, mr, up.CCode, buyRatio, needed));

                    var count =(int)Math.Ceiling(buyRatio);
                    var closableCount = t.GetClosableCount(up.CCode, this.positionType == PositionType.权利仓 ? TradeDirectType.卖 : TradeDirectType.买);
                    if (count > closableCount) count = closableCount;
                    sellor.Sell(t,up, count, br.Id);
                    if (t.GetMaintainRatio(Market) >= threshold)
                    {
                        break;
                    }
                }
                var br1 = CreateRecord(t, mr, false);
            if (OnBlasted != null)
            {
                OnBlasted(br1);
            }
            if (t.GetMaintainRatio() >= 1) return true;
            return false;
        }
    }
}

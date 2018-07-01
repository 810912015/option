using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Provider;
using System;
using System.Threading;

namespace Com.BitsQuan.Option.Imp
{ 
    public class ContractService : IInitialbe
    { 
        /// <summary>
        /// 累加的当年认沽(义务仓)编号
        /// </summary>
        public int DutyIdOfThisYear;
        /// <summary>
        /// 累加的当年认购(权利仓)编号
        /// </summary>
        public int RightIdOfThisYear;
        /// <summary>
        /// 从数据库总读取并设置编号
        /// </summary>
       public void Init()
        {
            RightIdOfThisYear = 19;DutyIdOfThisYear = 20;
            using (OptionDbCtx db = new OptionDbCtx())
            {
                var q = db.GlobalPrms.Find("DutyIdOfThisYear");
                if (q != null)
                {

                    int.TryParse(q.Value, out DutyIdOfThisYear);
                }
                var p = db.GlobalPrms.Find("RightIdOfThisYear");
                if (p != null)
                {
                    RightIdOfThisYear = 0;
                    int.TryParse(p.Value, out RightIdOfThisYear);
                }
            }
        }
        /// <summary>
        /// 将编号写入数据库,系统停止时使用
        /// </summary>
        public void Flush()
        {
            using (OptionDbCtx db = new OptionDbCtx())
            {
                var q = db.GlobalPrms.Find("DutyIdOfThisYear");
                
                if (q != null)
                {
                    q.Value = DutyIdOfThisYear.ToString();
                }
                else
                {
                    db.GlobalPrms.Add(new Core.Infra.GlobalPrm { Value = DutyIdOfThisYear.ToString(), Name = "DutyIdOfThisYear" });
                }
                var p = db.GlobalPrms.Find("RightIdOfThisYear");
                if (p != null)
                {
                    p.Value = RightIdOfThisYear.ToString();
                }
                else
                    db.GlobalPrms.Add(new Core.Infra.GlobalPrm { Value = RightIdOfThisYear.ToString(), Name = "RightIdOfThisYear" });
                db.SaveChanges();
            }
        }
 
        /// <summary>
        /// 生成期权合约代码
        /// </summary>
        /// <param name="coinType"></param>
        /// <param name="contractType"></param>
        /// <param name="year"></param>
        /// <param name="optionType"></param>
        /// <returns></returns>
        public string GenerateOptionContractCode(Coin coinType, ContractType contractType, DateTime year,
            OptionType optionType,ContractTimeSpanType timeSpanType)
        {
            if ( contractType == 0 || optionType == 0)
                throw new ArgumentException("产生期权代码时货币类型或合约类型或持仓类型不能为0");
            if (optionType ==OptionType.认购期权)
            {
                if (RightIdOfThisYear == 0) Interlocked.Exchange(ref RightIdOfThisYear, 1);
                else
                    Interlocked.Add(ref RightIdOfThisYear, 2);
            }

            if (optionType == OptionType.认沽期权)
            {
                if (DutyIdOfThisYear == 0) Interlocked.Exchange(ref DutyIdOfThisYear, 2);
                else
                Interlocked.Add(ref DutyIdOfThisYear, 2);
            } 
            return string.Format("{0}{1}{2}{3}{4:D4}",
                timeSpanType,
                 coinType.CotractCode,
                (int)contractType - 1, 
                contractType==ContractType.货币?0: year.Year % 10, 
                contractType==ContractType.货币?0:
                optionType == OptionType.认购期权 ? RightIdOfThisYear :
                DutyIdOfThisYear);
        }
 
        /// <summary>
        /// 生成期权合约名称
        /// </summary>
        /// <param name="coinType"></param>
        /// <param name="executeTime"></param>
        /// <param name="optionType"></param>
        /// <param name="executePrice"></param>
        /// <returns></returns>
        public string GenerateOptionContractName(Coin coinType, DateTime executeTime, OptionType optionType, decimal executePrice)
        {
            if ( optionType == 0 || executePrice < 0||executeTime<DateTime.Now)
                throw new ArgumentException("期权货币类型,持仓类型错误或行权价为0或行权日已过");            
            return string.Format("{0}{1}{2}{3}", coinType.Name, executeTime.ToString("yyyyMMdd"),
                optionType == OptionType.认沽期权 ? "沽" : "购", (int)executePrice);
        }
    }
}

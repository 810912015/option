using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Imp;
using System;
using System.Collections.Generic;

namespace Com.BitsQuan.Option.Match.Imp.Share
{
    public class WithdrawRecord
    {
        public DateTime Date { get; set; }
        public decimal Sum { get; set; }
    }
    public static class TraderShareExtension
    {
        /// <summary>
        /// 用户保证金账户快照:某时刻的保证金总额
        ///     以用户id的字串为键,系统键为SystemAccount
        /// </summary>
        static MyProperty<string, decimal> bailSnap = new MyProperty<string, decimal>();

        static Dictionary<Trader,WithdrawRecord> WithdrawRecordsOfToday = new Dictionary<Trader, WithdrawRecord>();


        public static decimal GetPreTotal(this SystemAccount sa)
        {
            return bailSnap.Get("SystemAccount");
        }
        public static void SetPreTotal(this SystemAccount sa, decimal total)
        {
            bailSnap.Set("SystemAccount", total);
        }
        public static decimal GetLossToday(this SystemAccount sa)
        {
            var now = Math.Abs(sa.PublicSum);
            var pre = Math.Abs(sa.GetPreTotal());
            return now - pre;
        }
        public static bool IsLossToday(this SystemAccount sa)
        {
            return sa.GetLossToday() > 0;
        }
        /// <summary>
        /// 获取上一次保存的保证金总额
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static decimal GetPreBailTotalSnap(this Trader t)
        {
            return bailSnap.Get(t.Id.ToString());
        }
        /// <summary>
        /// 设置保证金快照
        /// </summary>
        /// <param name="t"></param>
        /// <param name="totalSnap"></param>
        public static void SetBailTotalSnap(this Trader t, decimal totalSnap)
        {
            bailSnap.Set(t.Id.ToString(), totalSnap);
        }
        /// <summary>
        /// 今日是否盈利
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static bool IsProfit(this Trader t)
        {
            return t.Account.BailAccount.Total > t.GetPreBailTotalSnap();
        }
        /// <summary>
        /// 获取今日盈利
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static decimal GetProfitToday(this Trader t)
        {
            return t.Account.BailAccount.Total - t.GetPreBailTotalSnap();
        }
        /// <summary>
        /// 获取分摊比例
        /// </summary>
        /// <param name="t"></param>
        /// <param name="lossTotal">系统亏损总额</param>
        /// <returns></returns>
        public static decimal GetShareRatio(this Trader t, decimal totalProfit)
        {
            if (totalProfit <= 0) return 0;
            var profit = t.GetProfitToday();
            return profit / totalProfit;
        }
        /// <summary>
        /// 扣除亏损分摊:返回分摊的金额
        /// </summary>
        /// <param name="t"></param>
        /// <param name="lossTotal"></param>
        /// <returns></returns>
        public static decimal ShareLoss(this Trader t, decimal lossTotal, decimal totalProfit)
        {
            var raito = t.GetShareRatio(totalProfit);
            var myloss = lossTotal * raito;
            var r = t.Account.BailAccount.Sub(myloss);
            return myloss;
        }


        /// <summary>
        /// 更新今日提现总额
        /// </summary>
        /// <param name="self"></param>
        /// <param name="number"></param>
        public static void UpdateWithdrawSumOfToday(this Trader self, decimal number)
        {
            lock (WithdrawRecordsOfToday)
            {
                var now = DateTime.Now.Date;
                if (!WithdrawRecordsOfToday.ContainsKey(self))
                {
                    WithdrawRecordsOfToday[self] = new WithdrawRecord { Date = now, Sum = 0 };
                }

                if (WithdrawRecordsOfToday[self].Date != now)
                {
                    WithdrawRecordsOfToday[self].Date = now;
                    WithdrawRecordsOfToday[self].Sum = 0;
                }
                if (WithdrawRecordsOfToday[self].Sum + number >= 0)
                    WithdrawRecordsOfToday[self].Sum += number;
            }
        }

        /// <summary>
        /// 获取今日提现总额
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static decimal GetWithdrawSumOfToday(this Trader self)
        {
            lock (WithdrawRecordsOfToday)
            {
                if (!WithdrawRecordsOfToday.ContainsKey(self) || WithdrawRecordsOfToday[self].Date != DateTime.Now.Date)
                {
                    return 0;
                }
                else
                    return WithdrawRecordsOfToday[self].Sum;
            }
        }
    }
}

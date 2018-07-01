using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Imp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Com.BitsQuan.Option.Match.Imp.Share
{
    /// <summary>
    /// 执行系统每日亏损分担任务
    ///     系统亏损有盈利用户按比例分担
    /// </summary>
    public class SysShare : SpotTimer
    {
        IEnumerable<Trader> traders;
        public SysShare(IEnumerable<Trader> traders, TimeSpan ts)
            : base(ts)
        {
            log.Info("分摊对象构建");
            this.traders = traders;
            SaveSnap();
        }
        TextLog log = new TextLog("sysShare.txt");
        public SysShare(IEnumerable<Trader> ts) : this(ts,MatchParams.ShareSpan) {
            
        }

        void SaveSnap()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("分摊前每用户钱数:");
            var ts = traders.ToList();
            foreach (var v in ts)
            {
                sb.AppendFormat("{0}-{1},", v.Name, v.Account.BailAccount.Total);
                v.SetBailTotalSnap(v.Account.BailAccount.Total);
            }
            SystemAccount.Instance.SetPreTotal(SystemAccount.Instance.PublicSum);
            sb.AppendFormat("总亏损:{0}", SystemAccount.Instance.PublicSum);
            log.Info(sb.ToString());
        }

        public override void Execute()
        {
            //找出系统中的亏损总额
            if (!SystemAccount.Instance.IsLossToday())
            {
                log.Info("今日无亏损");
                return;
            }
            StringBuilder sb = new StringBuilder();
            var totalLoss = SystemAccount.Instance.GetLossToday();
            sb.AppendFormat("总亏损:{0}", totalLoss);
            //找出盈利用户及其盈利数额
            //计算所有用户的盈利总额
            //分摊比例=单用户的盈利数额/盈利总额
            //从每个盈利账户中扣除金额=亏损总额*该账户分摊比例
            var ts = traders.ToList();
            decimal totalProfit = 0;
            foreach (var v in ts)
            {
                if (!v.IsProfit())
                {
                    continue;
                }
                var p = v.GetProfitToday();
                sb.AppendFormat("{0}盈利{1}-", v.Name, p);
                totalProfit += p;
            }
            sb.AppendFormat("用户总盈利(保证金总额计算):", totalProfit);
            foreach (var v in ts)
            {
                if (!v.IsProfit()) continue;
                var sl = v.ShareLoss(totalLoss, totalProfit);
                sb.AppendFormat("-{0}分摊了{1}", v.Name, sl);
                SystemAccount.Instance.Share(sl, v);
            }
            log.Info(sb.ToString());
            //分摊完毕后应重新保存账户最新余额
            SaveSnap();
        }
    }
}

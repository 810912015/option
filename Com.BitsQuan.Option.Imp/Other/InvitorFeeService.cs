using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Provider;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Com.BitsQuan.Option.Imp
{
    /// <summary>
    /// 执行推荐人手续费划转
    ///     1.每月最后一天执行
    ///     2.计算所有用户的交易量和交易金额
    ///     3.执行划转
    /// </summary>
    public class InvitorFeeService
    {
        static TextLog log = new TextLog("invitorFee.txt");
        public static decimal InvitorBonusInCny = 10;
        public static decimal TransBonus(Trader t, List<string> invitedNames)
        {
            if (t == null || invitedNames == null || invitedNames.Count == 0) return 0;
            decimal result = 0;
            foreach (var v in invitedNames)
            {
                if (string.IsNullOrEmpty(v)) continue;

                bool r = TraderService.OperateInvitorBonus(t, InvitorBonusInCny, "被推荐人" + v);
                result += InvitorBonusInCny;
                log.Info(string.Format("{0}成功推荐了{1},获取奖金{2}元", t.Name, v, InvitorBonusInCny));
            }
            return result;
        }
        public static void TransFee(List<InvitorFeeTrans> l, Trader to)
        {
            using (var db = new OptionDbCtx())
            {
                foreach (var v in l)
                {
                    if (v.Trader == null) continue;
                    OperateInvitorFee(v, to, db, (s) => log.Info(s));
                }

            }
        }
        static decimal OperateInvitorFee(InvitorFeeTrans trans, Trader to, OptionDbCtx db, Action<string> log)
        {
            try
            {
                if (trans.Trader == null) return 0;
                var start=trans.LastTransferTime ?? DateTime.Now.Date;
                var ls = db.SysAccountRecords.Where(a => a.ChangedType == SysAccountChangeType.收取手续费
                    && a.Who != null
                    && a.Who.Id == trans.Trader.Id
                    && a.When > start
                    && a.When < DateTime.Now
                     ).Select(a => new { Delta = a.Delta });
                if (ls == null) return 0;
                var count = ls.Count();
                if (count == 0) return 0;
                var total = ls.Sum(a => a.Delta);
                if (total == 0) return 0;
                var r = total * trans.Ratio;
                if (r == 0) return 0;
                string detal = string.Format("{0}从{1}到{2}共交易{3}笔,手续费总额{4},返还比例{5},返还总额{6}", trans.Trader.Name, start, DateTime.Now, count, total, trans.Ratio, r);
                bool result = TraderService.OperateInvitorFee(to, r, detal);
                if (result) trans.UpdateSumAction(r);
                if (log != null) log(detal);
                return r;
            }
            catch (Exception ex)
            {
                Singleton<TextLog>.Instance.Error(ex);
                return 0;
            }

        }
    }
}

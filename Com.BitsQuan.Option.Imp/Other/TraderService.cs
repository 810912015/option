using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Imp;
using Com.BitsQuan.Option.Provider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace Com.BitsQuan.Option.Imp
{  
    public class TraderService
    {
         
        /// <summary>
        /// 佣金比例
        /// </summary>
        public static decimal CommissionRatio=Com.BitsQuan.Miscellaneous.AppSettings.Read<decimal>("commisionRatio", 0.001m);
        /// <summary>
        /// 执行保证金自动转入功能
        /// </summary>
        /// <param name="t">用户</param>
        /// <param name="delta">需要转入的金额</param>
        /// <param name="o">委托</param>
        /// <returns>是否成功转入</returns>
        public static bool OperateAddBailFromCache(Trader t, decimal delta,Order o)
        {
            if (!t.IsAutoAddBailFromCache) return false;
            var r = OperateAccount(t, delta, AccountChangeType.现金转保证金, "system", o);
            return r;
        }

        /// <summary>
        /// 用户账户现金操作
        /// </summary>
        /// <param name="t"></param>
        /// <param name="delta"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool OperateAccount(Trader t, decimal delta, AccountChangeType type, string byWho,Order o,decimal tcur=0)
        {
            if (delta <= 0) return false;
            bool r = false;
            bool needComm = false;
            bool needEvent = true;
            decimal cur = 0;
            Account ac = null;
            switch (type)
            {
                 
                case AccountChangeType.BTC付款:
                    r = t.Account.CacheAccount.BtcAccount.Sub(delta);
                    cur = t.Account.CacheAccount.BtcAccount.Total;
                    ac = t.Account.CacheAccount.BtcAccount;
                    break;
                case AccountChangeType.BTC收款:
                    r = t.Account.CacheAccount.BtcAccount.Add(delta);
                    cur = t.Account.CacheAccount.BtcAccount.Total;
                    ac = t.Account.CacheAccount.BtcAccount;
                    break;
                case AccountChangeType.保证金冻结:
                    r = t.Account.BailAccount.Freeze(delta);
                    
                    cur = t.Account.BailAccount.Total;
                    ac = t.Account.BailAccount;
                    break;
                case AccountChangeType.保证金解冻:
                    r = t.Account.BailAccount.UnFreeze(delta);
                    cur = t.Account.BailAccount.Total;
                    ac = t.Account.BailAccount;
                    break;
                case AccountChangeType.保证金转现金:
                    needEvent = false;
                    r=t.Account.BailAccount.Sub(delta);
                    if (r)
                    {
                        RaiseAccountChanged(t, delta, AccountChangeType.保证金转现金_保证金转出, r, byWho, "", t.Account.BailAccount.Total,t.Account.BailAccount);
                        r = t.Account.CacheAccount.CnyAccount.Add(delta);
                        RaiseAccountChanged(t, delta, AccountChangeType.保证金转现金_现金转入, r, byWho, "", t.Account.CacheAccount.CnyAccount.Total,t.Account.CacheAccount.CnyAccount);
                    }
                    cur = t.Account.CacheAccount.CnyAccount.Total;
                    break;
                case AccountChangeType.现金转保证金:
                    needEvent = false;
                    r = t.Account.CacheAccount.CnyAccount.Sub(delta);
                    if (r)
                    {
                        RaiseAccountChanged(t, delta, AccountChangeType.现金转保证金_现金转出, r, byWho, "", t.Account.CacheAccount.CnyAccount.Total,t.Account.CacheAccount.CnyAccount);
                        r = t.Account.BailAccount.Add(delta);
                        RaiseAccountChanged(t, delta, AccountChangeType.现金转保证金_保证金转入, r, byWho, "", t.Account.BailAccount.Total,t.Account.BailAccount);
                    }
                       
                    cur = t.Account.BailAccount.Total;
                    break;
                case AccountChangeType.现金收款:
                     r = t.Account.CacheAccount.CnyAccount.Add(delta);
                     cur = t.Account.CacheAccount.CnyAccount.Total;
                     ac = t.Account.CacheAccount.CnyAccount;
                    break;
                case AccountChangeType.CNY充值:
                    r = t.Account.CacheAccount.CnyAccount.Add(delta);
                    cur = t.Account.CacheAccount.CnyAccount.Total;
                    ac = t.Account.CacheAccount.CnyAccount;
                    SystemAccount.Instance.OperateExceptBorrowRepay(delta, t, SysAccountChangeType.充值, true);
                    break;
                case AccountChangeType.现金付款:
                    r = t.Account.CacheAccount.CnyAccount.Sub(delta); 
                    cur = t.Account.CacheAccount.CnyAccount.Total;
                    ac = t.Account.CacheAccount.CnyAccount;
                    break;
                case AccountChangeType.CNY提现:
                    //Sub
                    t.Account.CacheAccount.CnyAccount.UnFreeze(delta);
                    r = t.Account.CacheAccount.CnyAccount.Sub(delta);//再扣除可用
                    cur = t.Account.CacheAccount.CnyAccount.Total;
                    ac = t.Account.CacheAccount.CnyAccount;
                    SystemAccount.Instance.OperateExceptBorrowRepay(delta, t, SysAccountChangeType.提现, false);
                    break;
                case AccountChangeType.BTC充值:
                    r = t.Account.CacheAccount.BtcAccount.Add(delta);
                    cur = t.Account.CacheAccount.BtcAccount.Total;
                    ac = t.Account.CacheAccount.BtcAccount;
                    break;
                case AccountChangeType.BTC提现:
                    t.Account.CacheAccount.BtcAccount.UnFreeze(delta);
                    r = t.Account.CacheAccount.BtcAccount.Sub(delta);
                    cur = t.Account.CacheAccount.BtcAccount.Total;
                    ac = t.Account.CacheAccount.BtcAccount;
                    
                    break;
                case AccountChangeType.现金冻结:
                    r = t.Account.CacheAccount.CnyAccount.Freeze(delta);
                    cur = t.Account.CacheAccount.CnyAccount.Total;
                    ac = t.Account.CacheAccount.CnyAccount;
                    break;
                case AccountChangeType.BTC冻结:
                    r = t.Account.CacheAccount.BtcAccount.Freeze(delta);
                    cur = t.Account.CacheAccount.BtcAccount.Total;
                    ac = t.Account.CacheAccount.BtcAccount;
                    break;
                case AccountChangeType.BTC解冻:
                    r = t.Account.CacheAccount.BtcAccount.UnFreeze(delta);
                    cur = t.Account.CacheAccount.BtcAccount.Total;
                    ac = t.Account.CacheAccount.BtcAccount;
                    break;
                case AccountChangeType.现金解冻:
                    r = t.Account.CacheAccount.CnyAccount.UnFreeze(delta);
                    cur = t.Account.CacheAccount.CnyAccount.Total;
                    ac = t.Account.CacheAccount.CnyAccount;
                    break;
                case AccountChangeType.保证金收款:
                    r = t.Account.BailAccount.Collect(delta);
                    needComm = true;
                    cur = t.Account.BailAccount.Total;
                    ac = t.Account.BailAccount;
                    break;
                case AccountChangeType.保证金付款:
                    r = true;
                    needComm = true;
                    cur = tcur;
                    ac = t.Account.BailAccount;
                    break;
                case AccountChangeType.行权划入:
                    r = true;
                    needComm = false;
                    cur = t.Account.BailAccount.Total;
                    ac = t.Account.BailAccount;
                    break;
                case AccountChangeType.行权划出:
                    r = true;
                    needComm = false;
                    cur = tcur;
                    ac = t.Account.BailAccount;
                    break;
                default:
                    break;
            }
            if (needEvent) RaiseAccountChanged(t, delta, type, r,
                byWho,o==null?type.ToString(): o.ToShortString(),cur==0?t.Account.BailAccount.Sum:cur,ac);

            if (needComm)
            {
                PayCommission(delta, t.Account.BailAccount, t, o);
            }
            return r;
        }

        static void PayCommission(decimal d,Account a,Trader t,Order o){
            if (CommissionRatio == 0m) return;
            if (a is BailAccount)
            {
                var ba = a as BailAccount;
                var bdc = d * CommissionRatio;
                if (bdc < 0.1m) bdc = 0.1m;
                var br = ba.Sub(bdc);
                var cur = ba.Total;
                RaiseAccountChanged(t, Math.Round(bdc, 2, MidpointRounding.AwayFromZero), AccountChangeType.佣金支付, br, "system", o.ToShortString(), cur, a);
                SystemAccount.Instance.OperateExceptBorrowRepay(bdc, t, SysAccountChangeType.收取手续费, true);
            }
            else
            {
                var dc = d * CommissionRatio;
                if (dc < 0.1m) dc = 0.1m;
                var r = a.Sub(dc);
                var cur = a.Total;
                RaiseAccountChanged(t, Math.Round(dc, 2, MidpointRounding.AwayFromZero), AccountChangeType.佣金支付, r, "system", o.ToShortString(), cur, a);
                SystemAccount.Instance.OperateExceptBorrowRepay(dc, t, SysAccountChangeType.收取手续费, true);
            }
        }

        public static bool ManualChange(Trader t, decimal delta, AccountChangeType type, string byWho,bool isAboutFreeze=false,bool isFreeze=false)
        {
            bool result=false;
            decimal cur=0;
            Account account = null;
            if (isAboutFreeze)
            {
                switch (type)
                {
                    case AccountChangeType.手动减少BTC:
                        account = t.Account.CacheAccount.BtcAccount;
                        result =isFreeze? account.Freeze(delta):account.UnFreeze(delta);
                        cur = t.Account.CacheAccount.BtcAccount.Total;
                        break;
                    case AccountChangeType.手动减少保证金:
                        account = t.Account.BailAccount;
                        result = isFreeze ? account.Freeze(delta) : account.UnFreeze(delta);
                        cur = t.Account.BailAccount.Total;
                        break;
                    case AccountChangeType.手动减少现金:
                        account = t.Account.CacheAccount.CnyAccount;
                        result = isFreeze ? account.Freeze(delta) : account.UnFreeze(delta);
                        cur = t.Account.CacheAccount.CnyAccount.Total;
                        break;
                    case AccountChangeType.手动增加BTC:
                        account = t.Account.CacheAccount.BtcAccount;
                        result = isFreeze ? account.Freeze(delta) : account.UnFreeze(delta);
                        cur = t.Account.CacheAccount.BtcAccount.Total;
                        break;
                    case AccountChangeType.手动增加保证金:
                        account = t.Account.BailAccount;
                        result = isFreeze ? account.Freeze(delta) : account.UnFreeze(delta);
                        cur = t.Account.BailAccount.Total;
                        break;
                    case AccountChangeType.手动增加现金:
                        account = t.Account.CacheAccount.CnyAccount;
                        result = isFreeze ? account.Freeze(delta) : account.UnFreeze(delta);
                        cur = t.Account.CacheAccount.CnyAccount.Total;
                        break;
                }
            }
            else
            {
                switch (type)
                {
                    case AccountChangeType.手动减少BTC:
                        account = t.Account.CacheAccount.BtcAccount;
                        result = t.Account.CacheAccount.BtcAccount.Sub(delta);
                        cur = t.Account.CacheAccount.BtcAccount.Total;
                        
                        break;
                    case AccountChangeType.手动减少保证金:
                        account = t.Account.BailAccount;
                        result = t.Account.BailAccount.Sub(delta);
                        cur = t.Account.BailAccount.Total;
                        //手动操作资金要在系统私有总额中反方向操作,以保证总额恒定
                        SystemAccount.Instance.OperateExceptBorrowRepay(delta, t, SysAccountChangeType.手动调整_减少保证金, true);
                        break;
                    case AccountChangeType.手动减少现金:
                        account = t.Account.CacheAccount.CnyAccount;
                        result = t.Account.CacheAccount.CnyAccount.Sub(delta);
                        cur = t.Account.CacheAccount.CnyAccount.Total;
                        SystemAccount.Instance.OperateExceptBorrowRepay(delta, t, SysAccountChangeType.手动调整_减少现金, true);
                        break;
                    case AccountChangeType.手动增加BTC:
                        account = t.Account.CacheAccount.BtcAccount;
                        result = t.Account.CacheAccount.BtcAccount.Add(delta);
                        cur = t.Account.CacheAccount.BtcAccount.Total;
                        break;
                    case AccountChangeType.手动增加保证金:
                        account = t.Account.BailAccount;
                        result = t.Account.BailAccount.Add(delta);
                        cur = t.Account.BailAccount.Total;
                        SystemAccount.Instance.OperateExceptBorrowRepay(delta, t, SysAccountChangeType.手动调整_增加保证金, false);
                        break;
                    case AccountChangeType.手动增加现金:
                        account = t.Account.CacheAccount.CnyAccount;
                        result = t.Account.CacheAccount.CnyAccount.Add(delta);
                        cur = t.Account.CacheAccount.CnyAccount.Total;
                        SystemAccount.Instance.OperateExceptBorrowRepay(delta, t, SysAccountChangeType.手动调整_增加现金, false);
                        break;
                }
                RaiseAccountChanged(t, delta, type, result,byWho, "管理员手动调整", cur, account);
            }           
            return result;
        }

        #region 推荐用户手续费划转记录
        static object invFeeLock = new object();
        /// <summary>
        /// 推荐用户手续费划转操作
        /// </summary>
        /// <param name="t">划给的用户</param>
        /// <param name="delta">划给的金额</param>
        /// <param name="detail">详情:包含交易笔数,总金额,时间信息等</param>
        /// <returns>操作是否成功</returns>
        public static bool OperateInvitorFee(Trader t, decimal delta, string detail)
        {
            lock (invFeeLock)
            {
                t.Account.BailAccount.Add(delta);
                RaiseAccountChanged(t, delta, AccountChangeType.推荐用户交易手续费返还_划入, true, "system", detail, t.Account.BailAccount.Total, t.Account.BailAccount);
                SystemAccount.Instance.OperateExceptBorrowRepay(delta, t, SysAccountChangeType.推荐用户手续费返还_划出, false);
            }
            return true;
        }
        public static bool OperateInvitorBonus(Trader t, decimal delta, string detail)
        {
            t.Account.BailAccount.Add(delta);
            RaiseAccountChanged(t, delta, AccountChangeType.推荐用户奖金_划入, true, "system", detail, t.Account.BailAccount.Total, t.Account.BailAccount);
            SystemAccount.Instance.OperateExceptBorrowRepay(delta, t, SysAccountChangeType.推荐用户奖金_划出, false);
            return true;
        }
        #endregion
        /// <summary>
        /// 用户资金账户改变事件
        ///     参数:用户,改变的值,改变类型,操作是否成功,操作人,委托描述,账户当前总额
        /// </summary>
        public static event Action<Trader, decimal, AccountChangeType, bool, string,string,decimal,Account> OnAccountChanged;
        public static event Action<Trader, decimal> OnBailPayWhenInsufficent;

        static void RaiseAccountChanged(Trader t, decimal delta, AccountChangeType type, bool isSuccess, string who, string oderdesc, decimal curTotal,Account account)
        {
            if (OnAccountChanged != null)
            {
                OnAccountChanged(t, delta, type, isSuccess, who, oderdesc, curTotal,account);
            }
        }
        public static void RaiseOnPayWhenInsufficent(Trader u, decimal d)
        {
            if (OnBailPayWhenInsufficent != null)
            {
                foreach (var v in OnBailPayWhenInsufficent.GetInvocationList())
                {
                    try
                    {

                        ((Action<Trader, decimal>)v).BeginInvoke(u, d, null, null);
                    }
                    catch(Exception e) {
                        Singleton<TextLog>.Instance.Error(e, "raise on pay when insufficent");
                    }
                }
            }
        }
        
    }
    public static class TraderManualExtension
    {
        public static bool Manual(this Trader t, decimal delta, AccountChangeType type, string byWho,bool isAbountFreeze=false,bool isFreeze=false)
        {
            return TraderService.ManualChange(t, delta, type, byWho,isAbountFreeze,isFreeze);
        }
    }

    
}

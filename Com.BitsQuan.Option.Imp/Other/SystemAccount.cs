using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Provider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Com.BitsQuan.Option.Imp
{

    /// <summary>
    /// 系统资金账户
    /// </summary>
    public class SystemAccount : SingletonWithInit<SystemAccount>, IInitialbe
    {
        public TextLog Log = new TextLog("systemaccount.txt");
        public decimal PrivateSum { get; private set; }
        public decimal PublicSum { get; private set; }
        /// <summary>
        /// 还钱:系统亏损分担,应计入日志
        /// </summary>
        /// <param name="d"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public bool Repay(decimal d, Trader t)
        {
            var r = AddPublic(d); if (!r) return false;
            var cur = t.Account.BailAccount.Total;

            if (OnSystemAccountChanged != null)
            {
                Log.Info(string.Format("系统还款:{0}-金额{1}-系统金额{2}-用户还款前金额{3}-线程{4}", t.Name, d, PublicSum, t.Account.BailAccount.Sum, Thread.CurrentThread.ManagedThreadId));
                OnSystemAccountChanged(d, t, null, PublicSum, PrivateSum, SysAccountChangeType.还款, cur);
            }
            return true;
        }
        public bool Share(decimal d, Trader t)
        {
            var r = AddPublic(d); if (!r) return false;
            var cur = t.Account.BailAccount.Total;

            if (OnSystemAccountChanged != null)
            {
                Log.Info(string.Format("系统分摊:{0}-金额{1}-系统金额{2}-用户还款前金额{3}-线程{4}", t.Name, d, PublicSum, t.Account.BailAccount.Sum, Thread.CurrentThread.ManagedThreadId));
                OnSystemAccountChanged(d, t, null, PublicSum, PrivateSum, SysAccountChangeType.亏损分摊, cur);
            }
            return true;
        }
        bool AddPrivate(decimal d)
        {
            if (d <= 0) return false;
            lock (this)
            {
                PrivateSum += d;
            }
            return true;
        }
        bool AddPublic(decimal d)
        {
            if (d <= 0) return false;
            lock (this)
            {
                PublicSum += d;
            }
            return true;
        }
        /// <summary>
        /// 系统账户操作记录
        ///            参数: delt      交易员   委托   公开总额   私有总额     操作类型            用户账户金额
        /// </summary>
        public event Action<decimal, Trader, Order, decimal, decimal, SysAccountChangeType, decimal> OnSystemAccountChanged;
        /// <summary>
        /// 借钱:爆仓时用户保证金不足,应计入日志
        /// </summary>
        /// <param name="d"></param>
        /// <param name="o"></param>
        /// <returns></returns>
        public bool Borrow(decimal d, Order o)
        {
            if (d <= 0) return false;
            decimal cur;
            lock (this)
            {
                PublicSum -= d;
                o.Trader.Account.BailAccount.Add(d);
                cur = o.Trader.Account.BailAccount.Total;
            }
            if (OnSystemAccountChanged != null)
            {
                Log.Info(string.Format("系统借款:{0}-金额{1}-系统金额{2}-用户借款后金额{3}-线程{4}", o.Trader.Name, d, PublicSum, o.Trader.Account.BailAccount.Sum, Thread.CurrentThread.ManagedThreadId));
                OnSystemAccountChanged(d, o.Trader, o, PublicSum, PrivateSum, SysAccountChangeType.借款, cur);
            }
            return true;
        }
        bool SubPrivate(decimal d)
        {
            if (d <= 0) return false;
            lock (this)
            {
                PrivateSum -= d;
            }
            return true;
        }
        bool SubPublic(decimal d)
        {
            if (d <= 0) return false;
            lock (this)
            {
                PublicSum -= d;
            }
            return true;
        }
        public bool OperateExceptBorrowRepay(decimal d, Trader t, SysAccountChangeType type, bool IsAdd)
        {
            if (type == SysAccountChangeType.还款 || type == SysAccountChangeType.借款) throw new ArgumentException("此方法不能执行借款还款");
            if (IsAdd)
            {
                AddPrivate(d);
            }
            else SubPrivate(d);
            if (OnSystemAccountChanged != null)
            {
                OnSystemAccountChanged(d, t, null, PublicSum, PrivateSum, type, t.Account.BailAccount.Sum);
            }
            return true;
        }
        /// <summary>
        /// 从数据库中读取系统账号金额
        /// </summary>
        public void Init()
        {
            using (var db = new OptionDbCtx())
            {
                var conn = db.Database.Connection.Database;
                var sql = string.Format("SELECT TOP 1 *  FROM [{0}].[dbo].[SysAccountRecords] order by Id desc", conn);

                var sar = db.Database.SqlQuery<SysAccountRecord>(sql).FirstOrDefault();
                if (sar != null)
                {
                    PrivateSum = sar.PrivateSum;
                    PublicSum = sar.PublicSum;
                }

            }
        }
    }
}

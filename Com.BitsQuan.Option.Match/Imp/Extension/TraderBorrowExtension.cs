using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Imp;
using Com.BitsQuan.Option.Match.Imp;
using System;
using System.Threading;

namespace Com.BitsQuan.Option.Match
{

    public static class TraderBorrowExtension
    {
        static MyProperty<Trader, decimal> borrow = new MyProperty<Trader, decimal>();
        static BooleanProperty<Trader> cp = new BooleanProperty<Trader>();

        public static bool IsClearing(this Trader t)
        {
            return cp.Get(t);
        }
        public static void SetClearing(this Trader t, bool val)
        {
            cp.Set(t, val);
        }

        static bool ShouldRepay(this Trader t)
        {
            var o = borrow.Get(t);
            return o > 0;
        }
        public static void RecordBorrow(this Trader t, decimal delta)
        {
            var o = borrow.Get(t);
            borrow.Set(t, o + delta);
        }

        public static decimal GetBorrowedSum(this Trader self)
        {
            return borrow.Get(self);
        }
        /// <summary>
        /// 系统还款
        ///     返回---0:不欠款,-1:足额还清,-2:还款失败原因未知,-3:账户无钱,-4:正在还款此次不操作,大于0:部分还款,数字为还款金额
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static int RepayToSystem(this Trader t)
        {
            if (t.Account.BailAccount.Sum <= 0)
            {
                return -3;
            }
            var o = borrow.Get(t);
            if (o == 0)
            {
                return 0;
            }
            lock (t.Account.BailAccount)
            {
                SystemAccount.Instance.Log.Info(string.Format("还款:{0}-线程{1}", DateTime.Now.ToString("HH:mm:ss.fff"), Thread.CurrentThread.ManagedThreadId));
                if (t.Account.BailAccount.Sum >= o)
                {
                    var r = t.Account.BailAccount.Sub(o);
                    if (r)
                    {
                        SystemAccount.Instance.Repay(o, t);
                        borrow.Set(t, 0);
                        return -1;
                    }
                    else
                    {
                        return (int)t.PayAsMany();
                    }
                }
                else
                {
                    return (int)t.PayAsMany();
                }
            }

        }
        /// <summary>
        /// 有多少还多少:10块钱以下不还
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        static decimal PayAsMany(this Trader t)
        {
            if (t.Account.BailAccount.Sum < 10m) return -3;
            var now = t.Account.BailAccount.Sum;
            var r1 = t.Account.BailAccount.Sub(now);
            if (r1)
            {
                SystemAccount.Instance.Repay(now, t);
                var o = borrow.Get(t);
                var left=o - now;
                borrow.Set(t, left <= 0 ? 0 : left);
                return now;
            }
            return -2;
        }
    }


}

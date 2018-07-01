using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Imp;
using Com.BitsQuan.Option.Match.Imp;
using System;

namespace Com.BitsQuan.Option.Match
{
    public class AccountChangeHandler : IDisposable
    {
        AccountRecordSaver ars;
        public AccountChangeHandler()
        {
            ars = new AccountRecordSaver();
        }
        public void Save(Trader arg1, decimal arg2, AccountChangeType arg3, bool arg4, string byWho, string orderDesc, decimal current, Account ac)
        {
            var rec = new AccountTradeRecord
            {
                Id = IdService<AccountTradeRecord>.Instance.NewId(),
                ByWho = byWho,
                Delta = arg2,
                IsAddTo = arg4,
                When = DateTime.Now,
                Who = arg1,
                OperateType = arg3,
                OrderDesc = orderDesc,
                Current = current,
                Frozen =
                    arg3 == AccountChangeType.保证金冻结 || arg3 == AccountChangeType.保证金解冻 ?
                    arg1.Account.BailAccount.Frozen :
                    arg3 == AccountChangeType.现金冻结 || arg3 == AccountChangeType.现金解冻 ?
                    arg1.Account.CacheAccount.CnyAccount.Frozen : 0m,
                IsBail = ac is BailAccount,
                CoinId = ac.CacheType.Id
            };
            ars.Save(rec);
        }

        public void Save(decimal arg1, Trader arg2, Order arg3, SysAccountChangeType arg5, decimal bsum)
        {
            var rec = new AccountTradeRecord
            {
                Id = IdService<AccountTradeRecord>.Instance.NewId(),
                ByWho = "system",
                Delta = arg1,
                IsAddTo = true,
                When = DateTime.Now,
                Who = arg2,
                OperateType = arg5==SysAccountChangeType.亏损分摊?AccountChangeType.亏损分摊:
                arg5 == SysAccountChangeType.还款 ? AccountChangeType.系统还款 : AccountChangeType.系统借款,
                OrderDesc = arg3 == null ? "" : arg3.ToShortString(),
                Current = bsum,
                Frozen = 0
            };
            ars.Save(rec);
        }

        public void Dispose()
        {
            if (ars != null)
            {
                ars.Flush();
                ars.Dispose();
                ars = null;
            }
        }
        public void Flush()
        {
            ars.Flush();
        }
    }
}

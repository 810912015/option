using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Imp;
using Com.BitsQuan.Option.Match.Imp;
using System;

namespace Com.BitsQuan.Option.Match
{
    public class SysAccountChangeHandler : IDisposable, IFlush
    {
        SysAccountRecodeSaver srs;
        public SysAccountChangeHandler()
        {
            srs = new SysAccountRecodeSaver();
        }
        public void Save(decimal arg1, Trader arg2, Order arg3, decimal publicSum, decimal privateSum, SysAccountChangeType arg5, decimal bsum)
        {
            //保存系统借款记录
            SysAccountRecord sar = new SysAccountRecord
            {
                Id = IdService<SysAccountRecord>.Instance.NewId(),
                Order = arg3,
                Delta = arg1,
                ChangedType = arg5,
                PublicSum = publicSum,
                When = DateTime.Now,
                Who = arg2,
                TraderSum = bsum,
                PrivateSum = privateSum
            };
            srs.Save(sar);
        }
        public void Dispose()
        {
            srs.Flush();
            if (srs != null)
            {
                srs.Flush();
                srs.Dispose();
                srs = null;
            }
        }

        public void Flush()
        {
            srs.Flush();
        }
    }
}

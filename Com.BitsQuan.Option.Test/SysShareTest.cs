using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Imp;
using Com.BitsQuan.Option.Match.Imp.Share;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Com.BitsQuan.Option.Test
{
    [TestClass]
    public class SysShareTest
    {
        public List<Trader> GetTrader()
        {
            List<Trader> l = new List<Trader>();
            int id = 0;
            for (int i = 0; i < 100; i++)
            {
                var tid = i + 1;
                Trader t = new Trader
                {
                    Id = tid,
                    Name = "hello" + i,
                    Account = new TraderAccount
                    {
                        Id = tid,
                        BailAccount = new BailAccount { Id = id++, Sum = 1000000 },
                        CacheAccount = new CacheAccount
                        {
                            Id = tid,
                            CnyAccount = new Account { Id = id++, Sum = 1000000 },
                            BtcAccount = new Account { Id = id++, Sum = 1000000 }
                        }
                    }
                };
                l.Add(t);
            }
            return l;
        }
        [TestMethod]
        public void snap_test()
        {
            var ts = GetTrader();
            SysShare ss = new SysShare(ts);
            ss.Execute();
            foreach (var v in ts)
                Assert.AreEqual(v.Account.BailAccount.Total, v.GetPreBailTotalSnap());
        }

        [TestMethod]
        public void share_test()
        {
            var ts = GetTrader();
            SysShare ss = new SysShare(ts);
            ss.Execute();

            SystemAccount.Instance.Borrow(1000, new Order { Trader = ts[0] });

            for (int i = 1; i < 10; i++)
            {
                ts[i].Account.BailAccount.Add(i * 10000);
            }
            ss.SetIsFirst(false);
            ss.Execute();
            for (int i = 1; i < 10; i++)
            {
                Assert.IsTrue(ts[i].Account.BailAccount.Total < 1000000 + i * 10000);
            }
            Assert.AreEqual(0, SystemAccount.Instance.PublicSum);

            for (int i = 0; i < 10; i++)
            {
                Assert.AreEqual(ts[i].Account.BailAccount.Total, ts[i].GetPreBailTotalSnap());
            }
            Assert.AreEqual(SystemAccount.Instance.PublicSum, SystemAccount.Instance.GetPreTotal());
        }
    }
}

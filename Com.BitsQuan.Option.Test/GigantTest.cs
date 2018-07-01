using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Com.BitsQuan.Option.Match.Imp;
using System.Data;
using Com.BitsQuan.Option.Provider;
using Com.BitsQuan.Option.Core;
using System.Collections.Generic;

namespace Com.BitsQuan.Option.Test
{
    [TestClass]
    public class GigantTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            try
            {
                var c1 = new Coin { Id = 1, Name = "CNY", MainBailRatio = 1, MainBailTimes = 1, CotractCode = "9" };
                var c2 = new Coin { Id = 2, Name = "BTC", MainBailRatio = 0.1m, MainBailTimes = 3, CotractCode = "1" };
                var c3 = new Coin { Id = 3, Name = "LTC", MainBailRatio = 0.2m, MainBailTimes = 4, CotractCode = "2" };
                int id = 331;
                List<Trader> l = new List<Trader>();

                for (int i = 101; i < 20101; i++)
                {
                    var tid = i + 1;
                    Trader t = new Trader
                    {
                        Id = tid,
                        Name = "hello" + i,
                        Account = new TraderAccount
                        {
                            Id = tid,
                            BailAccount = new BailAccount { Id = id++, CacheType = c1, Sum = 1000000 },
                            CacheAccount = new CacheAccount
                            {
                                Id = tid,
                                CnyAccount = new Account { Id = id++, CacheType = c1, Sum = 1000000 },
                                BtcAccount = new Account { Id = id++, CacheType = c2, Sum = 1000 }
                            }
                        }
                    };
                    l.Add(t);
                }
                TraderSaver ts = new TraderSaver();
                TraderAccountSaver tas = new TraderAccountSaver();
                AccountSaver acs = new AccountSaver();
                CacheAccountSaver cas = new CacheAccountSaver();
                for (int i = 0; i < l.Count;i++ ) 
                    {
                        var v = l[i];
                        acs.Save(v.Account.BailAccount);
                        acs.Save(v.Account.CacheAccount.BtcAccount);
                        acs.Save(v.Account.CacheAccount.CnyAccount);
                        if (i % 999 == 0)
                            acs.Flush();
                    }
                acs.Flush();
                for (int i = 0; i < l.Count; i++) 
                {
                    var v = l[i];
                    cas.Save(v.Account.CacheAccount);
                    if (i % 999 == 0) cas.Flush();
                }
                cas.Flush();
                for (int i = 0; i < l.Count; i++) 
                {
                    var v = l[i];
                    tas.Save(v.Account);
                    if (i % 999 == 0)
                        tas.Flush();
                }
                tas.Flush();
                for (int i = 0; i < l.Count; i++) 
                {
                    var v = l[i];
                    ts.Save(v);
                    if (i % 999 == 0)
                        ts.Flush();
                }
                ts.Flush();
                Assert.AreEqual(20000, l.Count);
            }
            catch (Exception ex)
            {

            }
            
        }
    }
}

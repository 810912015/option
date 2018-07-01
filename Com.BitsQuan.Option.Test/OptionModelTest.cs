using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Com.BitsQuan.Option.Match;
using System.Linq;
using Com.BitsQuan.Option.Provider;
using Com.BitsQuan.Option.Core;

namespace Com.BitsQuan.Option.Test
{
    [TestClass]
    public class OptionModelTest
    {
        [TestMethod]
        public void flush_test()
        {
            var om = new OptionModel();
            om.Init();
            var q = om.Traders.Where(a => a.Name == "hello0").FirstOrDefault();
            q.Account.BailAccount.Sum = 9;
            q.Account.BailAccount.Frozen = 8;
            //q.Account.BailAccount.MaintainCount = 7;
            om.Flush();
            var db = new OptionDbCtx();
            var t = db.Set<BailAccount>().Where(a => a.Id == q.Id).FirstOrDefault();
            Assert.AreEqual(t.Sum, 9);
            Assert.AreEqual(t.Frozen, 8);
            //Assert.AreEqual(t.MaintainCount, 7);  
        }
        [TestMethod]
        public void init_test()
        {
            var om = new OptionModel();
            om.Init();
             
            Assert.IsNotNull(om.Traders);
        }
        public class Coin2 : Order { }
        [TestMethod]
        public void poco()
        {
            var db = new OptionDbCtx();
            var r = db.Set<Order>().Select(a=>new Coin2{ Id=a.Id, Contract
            =a.Contract, Count =a.Count}).ToList<Coin2>();
            Assert.IsNotNull(r);
        }

    }
}

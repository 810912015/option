using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Com.BitsQuan.Option.Ui.Models;
using System.Linq;
using Com.BitsQuan.Option.Imp;
using System.Threading;

namespace Com.BitsQuan.Option.Ui.Tests
{
    [TestClass]
    public class InvitorFeeTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            InvitorFeeMgr ifm = new InvitorFeeMgr();
            ifm.TransferFee();
            Assert.IsNotNull(ifm);
        }

        [TestMethod]
        public void bonus_test()
        {
            Match.MatchService ms = new Match.MatchService();
            var t1 = ms.Model.Traders.Where(a => a.Name == "hello56").FirstOrDefault();

            InvitorFeeMgr ifm = new InvitorFeeMgr();
            var r=ifm.TransBonus(t1, new System.Collections.Generic.List<string>{"hello54", "hello55"});
            ms.Flush();

            Assert.AreEqual(r, InvitorFeeService.InvitorBonusInCny*2);
        }
    }
}

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Com.BitsQuan.Option.Imp;
using Com.BitsQuan.Option.Provider;
using Com.BitsQuan.Option.Core;

namespace Com.BitsQuan.Option.Test
{
    [TestClass]
    public class ContractRepoTest
    {

        [TestMethod]
        public void Add_Multi()
        {
            var db = new OptionDbCtx();
            var v = new ContractRepo(db);
             
            for (int i = 0; i < 10; i++)
            {
                var r = v.Add("btc", DateTime.Now.AddMonths(i+1), 100, OptionType.认购期权, "比特币");
            }

            for (int i = 0; i < 10; i++)
            {
                var r = v.Add("btc", DateTime.Now.AddMonths(i + 1), 100, OptionType.认沽期权, "比特币");
            }
            Assert.IsNotNull(v);

        }



        [TestMethod]
        public void Add_test()
        {
            var db = new OptionDbCtx();
            var v = new ContractRepo(db);
            var r = v.Add("btc", DateTime.Now.AddMonths(1), 100, OptionType.认购期权, "比特币");
            SingletonWithInit<ContractService>.Instance.Flush();
            Assert.AreEqual(true, r);
            
        }

        [TestMethod]
        public void GenId_1()
        {
            var cs = new ContractService();
            var r = cs.GenerateOptionContractCode(CoinRepo.Instance.CNY, Core.ContractType.货币, DateTime.Now, Core.OptionType.认购期权,ContractTimeSpanType.季);
            Assert.AreEqual("900000", r);
        }
        [TestMethod]
        public void GenId_2()
        {
            var cs = new ContractService();
            var r = cs.GenerateOptionContractCode(CoinRepo.Instance.BTC,
                Core.ContractType.货币, DateTime.Now, Core.OptionType.认购期权, ContractTimeSpanType.季);
            Assert.AreEqual("100000", r);
        }
        [TestMethod]
        public void GenId_3()
        {
            var cs = new ContractService();
            var r = cs.GenerateOptionContractCode(CoinRepo.Instance.BTC,
                Core.ContractType.期权, DateTime.Now, Core.OptionType.认购期权, ContractTimeSpanType.季);
            Assert.AreEqual("114001", r);
        }
        [TestMethod]
        public void GenId_4()
        {
            var cs = new ContractService();
            var r = cs.GenerateOptionContractCode(CoinRepo.Instance.BTC,
                Core.ContractType.期权, DateTime.Now, Core.OptionType.认沽期权, ContractTimeSpanType.季);
            Assert.AreEqual("114002", r);
            var r1 = cs.GenerateOptionContractCode(CoinRepo.Instance.BTC,
                Core.ContractType.期权, DateTime.Now, Core.OptionType.认沽期权, ContractTimeSpanType.季);
            Assert.AreEqual("114004", r1);
        }

        [TestMethod]
        public void GenName_1()
        {
            var c = new ContractService();
            var r = c.GenerateOptionContractName(CoinRepo.Instance.BTC, new DateTime(2014, 12, 31), Core.OptionType.认购期权, 3000);
            Assert.AreEqual("BTC20141231购3000", r);
        }

        [TestMethod]
        public void GenName_2()
        {
            var c = new ContractService();
            var r = c.GenerateOptionContractName(CoinRepo.Instance.BTC, new DateTime(2014, 12, 31), Core.OptionType.认沽期权, 3000);
            Assert.AreEqual("BTC20141231沽3000", r);
        }

         

        [TestMethod]
        public void Flush_test()
        {
            var cs = new ContractService();
            var r = cs.GenerateOptionContractCode(CoinRepo.Instance.BTC,
                Core.ContractType.期权, DateTime.Now, Core.OptionType.认购期权, ContractTimeSpanType.季);
            Assert.AreEqual("114002", r);
            var r1 = cs.GenerateOptionContractCode(CoinRepo.Instance.BTC,
                Core.ContractType.期权, DateTime.Now, Core.OptionType.认沽期权, ContractTimeSpanType.季);
            Assert.AreEqual("114004", r1);

            cs.Flush();
            using(var db=new OptionDbCtx())
            {
                var p = db.GlobalPrms.Find("DutyIdOfThisYear");
                Assert.IsNotNull(p);
                Assert.AreEqual("4", p.Value);
                var q = db.GlobalPrms.Find("RightIdOfThisYear");
                Assert.IsNotNull(q);
                Assert.AreEqual("0", q.Value);
            }
        }
    }
}

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Com.BitsQuan.Boss.Imp; 

namespace Com.BitsQuan.Boss.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            CHelper ch = new CHelper();
            ch.Refresh();
            Assert.AreEqual(ch.ContractList.Count, 20);
        }

        [TestMethod]
        public void TestMethod2()
        {
            CHelper ch = new CHelper();
            ch.Refresh();

            OptionOrderMaker oom = new OptionOrderMaker(ch, "hello1");
            var r = oom.OrderIt("115001", 10.1m, 10.1m, 1);
        }
    }
}

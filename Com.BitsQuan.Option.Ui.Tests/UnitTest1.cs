using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Com.BitsQuan.Option.Ui.Models;
using Com.BitsQuan.Option.Match.Dto;

namespace Com.BitsQuan.Option.Ui.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void t1()
        {
            var m = 334.125m;
            var t = Math.Round(m, 2);
            Assert.AreEqual(334.12m, t);
        }

        [TestMethod]
        public void TestMethod1()
        {
            MarketDto md = new MarketDto();
            DateTime dt = DateTime.Now.AddHours(2);
            var r = md.MakeCe(dt);
            Assert.IsTrue(r.Contains("小时"));
            dt = dt.AddHours(-1.5);
            var r1 = md.MakeCe(dt);
            Assert.IsTrue(r1.Contains("分钟"));

            dt = DateTime.Now.AddSeconds(50);
            var r2 = md.MakeCe(dt);
            Assert.IsTrue(r2.Contains("秒"));

            dt = DateTime.Now.AddDays(100);
            var r3 = md.MakeCe(dt);
            Assert.AreEqual(r3, "100天");
        }
    }
}

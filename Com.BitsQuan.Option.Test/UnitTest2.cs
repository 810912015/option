using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Com.BitsQuan.Option.Match.Imp;

namespace Com.BitsQuan.Option.Test
{
    [TestClass]
    public class UnitTest2
    {
        [TestMethod]
        public void TestMethod1()
        {
            BooleanProperty<int> bp = new BooleanProperty<int>();

            Assert.AreEqual(false, bp.Get(1));

            bp.Set(1, true);
            Assert.AreEqual(true, bp.Get(1));

            bp.Set(1, false);
            Assert.AreEqual(false, bp.Get(1));
            bp.Set(1, true);
            Assert.AreEqual(true, bp.Get(1));

            bp.Set(1, false);
            Assert.AreEqual(false, bp.Get(1));
            bp.Set(1, true);
            Assert.AreEqual(true, bp.Get(1));

            bp.Set(1, false);
            Assert.AreEqual(false, bp.Get(1));

        }
    }
}

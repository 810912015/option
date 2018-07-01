using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Com.BitsQuan.Option.Ui.Models;
using Com.BitsQuan.Option.Provider;
using Com.BitsQuan.Option.Core;
using System.Linq;

namespace Com.BitsQuan.Option.Test
{
    [TestClass]
    public class OrderDealReaderTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            using (OptionDbCtx db = new OptionDbCtx(@"Data Source=.\sqlexpress;Initial Catalog=Option;Integrated Security=True"))
            {
                OrderDealReader odr = new OrderDealReader(db);

                var r = odr.Query("hello1", 1);

                Assert.AreEqual(true, r.Item2);

                SpotOrderDealReader sor = new SpotOrderDealReader(db);
                var r2 = sor.Query("hello1", 1);
            }
        }


        [TestMethod]
        public void TestMethod2()
        {
            OptionDbCtx db = new OptionDbCtx(@"Data Source=.\sqlexpress;Initial Catalog=Option-dev;Integrated Security=True");
            var r = db.Database.SqlQuery<Deal>("select * from deals");
            var rr = r.ToList<Deal>();

            Assert.IsNotNull(r);
        }
    }
}

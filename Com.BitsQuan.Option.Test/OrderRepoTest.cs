using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Com.BitsQuan.Option.Imp;
using Com.BitsQuan.Option.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Com.BitsQuan.Option.Test
{
    [TestClass]
    public class OrderRepoTest
    {
        [TestMethod]
        public void guid_test()
        {
            var b = Guid.NewGuid().ToString().GetHashCode();
            Assert.AreEqual(16, b);
        }
        [TestMethod]
        public void add_test()
        {
            OrderRepo orp = new OrderRepo(new Provider.OptionDbCtx());
            var o = orp.CreateOrder(1, 1, Core.TradeDirectType.卖, OrderPolicy.限价申报, 10, 100);
            var r=orp.Add(o);
            Assert.AreEqual(true, r);
        }

        [TestMethod]
        public void deepth_fake()
        {
            MarketDeepth md = new MarketDeepth();
            var r = md.Fake();
            Assert.IsNotNull(r);
        }

        [TestMethod]
        public void id_test()
        {
            List<int> l = new List<int>();
            object o = new object();
            
            for (int i = 0; i < 100000; i++)
            {
                Task.Factory.StartNew(() =>
                {
                    var t = IdService<Order>.Instance.NewId();
                    lock (o)
                    {
                        l.Add(t);
                    }
                });                
            }
             
                Task.WaitAll(); 
            for (int i = 0; i < l.Count-1; i++)
            {
                Assert.AreNotEqual(l[i], l[i + 1]);
            }
        }
    }
}

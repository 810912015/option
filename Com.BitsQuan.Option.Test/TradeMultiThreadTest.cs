using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Com.BitsQuan.Option.Core;
using System.Collections.Generic;
using Com.BitsQuan.Option.Imp;
using System.Threading.Tasks;
using Com.BitsQuan.Option.Match;
using System.Linq;

namespace Com.BitsQuan.Option.Test
{
    [TestClass]
    public class TradeMultiThreadTest:TradeBase
    {
        [TestMethod]
        public void u_2()
        {
            object loc = new object();


            List<OrderResult> l = new List<OrderResult>();

            RestoreDb();
            ms = new Match.MatchService();
            List<Task> lt = new List<Task>();
            var task1= Task.Factory.StartNew(() => {
                for (int i = 0; i < 10; i++)
                {
                    var tr = ms.AddOrder(t1.Id, c.Id, TradeDirectType.卖, OrderType.开仓, OrderPolicy.限价申报, 10, 88);
                    l.Add(tr);
                }
            });
            var task2= Task.Factory.StartNew(() => {
                for (int i = 0; i < 10; i++)
                {
                    var tr = ms.AddOrder(t2.Id, c.Id, TradeDirectType.买, OrderType.开仓, OrderPolicy.限价申报, 10, 88);
                    l.Add(tr);
                }
            });
            lt.Add(task1);
            lt.Add(task2);
            Task.WaitAll(lt.ToArray());
            ms.Flush();
            Arrange.log.Flush();
            Assert.AreEqual(20, l.Count);
            for (int i = 0; i < l.Count;i++ )
            {
                var v = l[i];
                Assert.AreEqual(v.ResultCode, 0);
                Assert.AreEqual(v.Order.State, OrderState.已成交);
            }
        }



        [TestMethod]
        public void u_100()
        {
            List<OrderResult> l = new List<OrderResult>(1200);
            //TextLog log = new TextLog("u_100.txt");
            RestoreDb();
            ms = new Match.MatchService();
            List<Task> lt = new List<Task>();
            var ts = ms.Model.Traders.ToList();
            var total = ts.Count;
            for (int j = 0; j < total; j++)
            {
                var v = ts[j];
                var dir=j<total/2 ? TradeDirectType.卖 : TradeDirectType.买;
                var ta = Task.Factory.StartNew(() =>
                {
                    for (int i = 0; i < 10; i++)
                    {
                        var tr = ms.AddOrder(v.Id, c.Id, dir, OrderType.开仓, OrderPolicy.限价申报, 10, 88);
                        l.Add(tr);
                        //log.Info(tr.ToString());
                    }
                });
                lt.Add(ta);
            }
            Task.WaitAll(lt.ToArray());
            ms.Flush();
            Arrange.log.Flush();
            Assert.IsTrue(l.Count <= total * 10);
            Assert.IsTrue(l.Count >= total * 8);
            //Assert.AreEqual(total*10, l.Count);
            int h = 0;
            for (int i = 0; i < l.Count; i++)
            {
                var v = l[i];
                Assert.AreEqual(v.ResultCode, 0);
                h += v.Order.State == OrderState.已成交 ? 1 : 0; 
            }
            Assert.IsTrue(h <= total * 10);
            Assert.IsTrue(h >= total * 5);
        }
    }
}

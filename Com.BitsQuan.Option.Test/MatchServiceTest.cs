using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Com.BitsQuan.Option.Match;
using System.Threading;
using Com.BitsQuan.Option.Imp;
using System.Collections.Generic;
using Com.BitsQuan.Option.Provider;
using System.Linq;
using Com.BitsQuan.Option.Core;

namespace Com.BitsQuan.Option.Test
{
    [TestClass]
    public class MatchServiceTest
    {
        
        [TestMethod]
        public void Cons_test()
        {
            var cs = new MatchService();
            Assert.IsNotNull(cs);
        }
         
        [TestMethod]
        public void sell_open_ok()
        {
            var ms = new MatchService();
            var r = ms.AddOrder(1, 1, Core.TradeDirectType.卖, OrderType.开仓,OrderPolicy.限价申报, 10, 10);
            Assert.AreEqual(true, r.IsSuccess);
             
            Assert.AreEqual(ms.Matcher.Container.Orders.Count, 1);

        }
        [TestMethod]
        public void sell_close_fail_because_no_position()
        {
            var ms = new MatchService();
            var r = ms.AddOrder(1, 1, Core.TradeDirectType.买,  OrderType.平仓, OrderPolicy.限价申报, 10, 10);
            Assert.AreEqual(false, r.IsSuccess); 
        }
         [TestMethod]
        public void one_deal_ok()
        {
            var ms = new MatchService();
            var r = ms.AddOrder(3, 2, Core.TradeDirectType.卖,  OrderType.开仓, OrderPolicy.限价申报, 10, 10);
            Assert.AreEqual(true, r.IsSuccess);
            Assert.AreEqual(ms.Matcher.Container.Orders.Count, 1);

            var r1 = ms.AddOrder(2, 2, Core.TradeDirectType.买,  OrderType.开仓, OrderPolicy.限价申报, 10, 10);
            
            Assert.AreEqual(true, r1.IsSuccess);
            Assert.AreEqual(ms.Matcher.Container.Orders.Count, 1); 

        }

        [TestMethod]
         public void multi_deal_ok()
         {
             var ms = new MatchService();
             List<OperationResult> l = new List<OperationResult>();
             for (int i = 0; i < 10; i++)
             {
                 var r = ms.AddOrder(i + 1, 1, Core.TradeDirectType.卖,  OrderType.开仓, OrderPolicy.限价申报, 10, 10);
                 l.Add(r);
             }

             for (int i = 0; i < 10; i++)
             {
                 var r = ms.AddOrder(i + 1, 1, Core.TradeDirectType.买, OrderType.开仓, OrderPolicy.限价申报, 10, 10);
                 l.Add(r);
             }
             foreach (var v in l)
             {
                 Assert.AreEqual(true, v.IsSuccess);
             }
         }
        [TestMethod]
        public void multi_trader_sell_buy_count_not_eaque()
        {
            var ms = new MatchService();
            List<OperationResult> l = new List<OperationResult>();
            for (int i = 0; i < 10; i++)
            {
                var r = ms.AddOrder(i + 1, 1, Core.TradeDirectType.卖,  OrderType.开仓, OrderPolicy.限价申报, (i + 1) * 10, 10);
                l.Add(r);
            }

            for (int i = 0; i < 10; i++)
            {
                var r = ms.AddOrder(i + 1, 1, Core.TradeDirectType.买,  OrderType.开仓, OrderPolicy.限价申报, (i + 1) * 11, 10);
                l.Add(r);
            }
            foreach (var v in l)
            {
                Assert.AreEqual(true, v.IsSuccess);
            }
        }
        
        [TestMethod]
        public void sell_and_has_position()//买卖开仓
        {
            var ms = new MatchService();
            var t =ms.Model.Traders.Where(a => a.Id == 1).FirstOrDefault();// ms.dataCenter.Db.Set<Trader>().Find(1);
            var t2 = ms.Model.Traders.Where(a => a.Id == 2).FirstOrDefault();//ms.dataCenter.Db.Set<Trader>().Find(2);
            var orginalCount = t.Positions.Count;
            var orginalCount2 = t2.Positions.Count;
            var r = ms.AddOrder(1, 1, Core.TradeDirectType.卖,  OrderType.开仓, OrderPolicy.限价申报, 10, 10);
            var r1 = ms.AddOrder(2, 1, Core.TradeDirectType.买, OrderType.开仓, OrderPolicy.限价申报, 10, 10);
             
            Assert.AreEqual(1, t.Positions.Count-orginalCount);
            Assert.AreEqual(1, t2.Positions.Count - orginalCount2);
        }

        [TestMethod]
        public void anti_sell_and_has_position()//买卖平仓
        {
            var ms = new MatchService();
            var t = ms.Model.Traders.Where(a => a.Id == 1).FirstOrDefault();// ms.dataCenter.Db.Set<Trader>().Find(1);
            var t2 = ms.Model.Traders.Where(a => a.Id == 2).FirstOrDefault();//ms.dataCenter.Db.Set<Trader>().Find(2);
            var orginalCount = t.Positions.Count;
            var orginalCount2 = t2.Positions.Count;
            var r = ms.AddOrder(1, 1, Core.TradeDirectType.买,  OrderType.开仓, OrderPolicy.限价申报, 10, 10);
            var r1 = ms.AddOrder(2, 1, Core.TradeDirectType.卖,  OrderType.开仓, OrderPolicy.限价申报, 10, 10);

            Assert.AreEqual(1, t.Positions.Count - orginalCount);
            Assert.AreEqual(1, t2.Positions.Count - orginalCount2);
        }
        //[TestMethod]
        //public void GetTimeByPrice_Test()
        //{ //熔断机制测试（购）
        //    var fuse = new MarketItem();
        //  //  var r = fuse.GetTimeByPrice(OptionType.认购期权, 2000);
        ////    Assert.AreEqual("最高熔断价：110,最低熔断价：110", r);
        //}

       // [TestMethod]
       // public void GetTimeByPrice_Test2()
       // { //熔断机制测试（沽）
       //     var fuse = new MarketItem();
       ////     var r = fuse.GetTimeByPrice(OptionType.认购期权, 100);
       // //    Assert.AreEqual("最高熔断价：110,最低熔断价：50", r);
       // }
    }
}

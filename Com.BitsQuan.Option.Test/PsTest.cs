using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Match;
using System.Collections.Generic;
using Com.BitsQuan.Miscellaneous;
using Com.BitsQuan.Option.Match.Imp;
using System.Collections.ObjectModel;

namespace Com.BitsQuan.Option.Test
{
    [TestClass]
    public class PsTest
    {
        UserPosition createPosition(int count, decimal price, OrderType type, TradeDirectType dir)
        {
            UserPosition up = new UserPosition
            {
                Order = new Order
                {
                    Contract = new Contract
                    {
                        Code = "1",
                        Name = "1",
                        Coin = new Coin { Id = 1 }
                    },
                    OrderType = type,
                    Direction = dir,
                    Count = count,
                    DonePrice = price,
                    DoneCount=count,
                    
                },
                Count = count,
                Trader = new Trader { }
            };
            return up;
        }

        [TestMethod]
        public void CreateTest18()
        {
            UserPosition up = createPosition(10, 90,
                OrderType.开仓, TradeDirectType.买);

            var ps = new PositionSummary(up, 90m);

            var up1 = createPosition(20, 120,
                OrderType.开仓, TradeDirectType.买);
            ps.Update(up1, true, 120);

            var up2 = createPosition(5, 200,
                OrderType.平仓, TradeDirectType.卖);
            ps.Update(up2, false, 200);

            var up3 = createPosition(10, 200,
                OrderType.开仓, TradeDirectType.买);
            ps.Update(up3, true, 200);

            var up4 = createPosition(15, 140,
                OrderType.平仓, TradeDirectType.卖);
            ps.Update(up4, false, 140);

            var up5 = createPosition(10, 80,
                OrderType.开仓, TradeDirectType.卖);
            ps.Update(up5, true, 80);

            Assert.AreEqual(10, ps.Count);
           // Assert.AreEqual(10, ps.ClosableCount);
            Assert.IsTrue(135.714m - ps.BuyPrice < 0.001m);
            Assert.IsTrue(1357.14m - ps.BuyTotal < 0.01m);
            Assert.IsTrue(-557.14m - ps.FloatProfit < 0.01m);
            Assert.IsTrue(-42.85m - ps.CloseProfit < 0.01m);
            Assert.AreEqual(800, ps.TotalValue);
        }



        [TestMethod]
        public void CreateTest17()
        {
            UserPosition up = createPosition(10, 90,
                OrderType.开仓, TradeDirectType.买);

            var ps = new PositionSummary(up, 90m);

            var up1 = createPosition(20, 120,
                OrderType.开仓, TradeDirectType.买);
            ps.Update(up1, true, 120);

            var up2 = createPosition(5, 200,
                OrderType.平仓, TradeDirectType.卖);
            ps.Update(up2, false, 200);

            var up3 = createPosition(10, 200,
                OrderType.开仓, TradeDirectType.买);
            ps.Update(up3, true, 200);

            var up4 = createPosition(15, 140,
                OrderType.平仓, TradeDirectType.卖);
            ps.Update(up4, false, 140);

            var up5 = createPosition(10, 80,
                OrderType.平仓, TradeDirectType.卖);
            ps.Update(up5, false, 80);

            var up6 = createPosition(10, 90,
                OrderType.平仓, TradeDirectType.卖);
            ps.Update(up6, false, 90);

            Assert.AreEqual(0, ps.Count);
            //Assert.AreEqual(0, ps.ClosableCount);
            Assert.IsTrue(0 - ps.BuyPrice < 0.001m);
            Assert.IsTrue(0 - ps.BuyTotal < 0.01m);
            Assert.IsTrue(-0 - ps.FloatProfit < 0.01m);
            Assert.IsTrue(-0 - ps.CloseProfit < 0.01m);
            Assert.AreEqual(0, ps.TotalValue);
        }


        [TestMethod]
        public void CreateTest16()
        {
            UserPosition up = createPosition(10, 90,
                OrderType.开仓, TradeDirectType.买);

            var ps = new PositionSummary(up, 90m);

            var up1 = createPosition(20, 120,
                OrderType.开仓, TradeDirectType.买);
            ps.Update(up1, true, 120);

            var up2 = createPosition(5, 200,
                OrderType.平仓, TradeDirectType.卖);
            ps.Update(up2, false, 200);

            var up3 = createPosition(10, 200,
                OrderType.开仓, TradeDirectType.买);
            ps.Update(up3, true, 200);

            var up4 = createPosition(15, 140,
                OrderType.平仓, TradeDirectType.卖);
            ps.Update(up4, false, 140);

            var up5 = createPosition(10, 80,
                OrderType.平仓, TradeDirectType.卖);
            ps.Update(up5, false, 80);

            Assert.AreEqual(10, ps.Count);
            //Assert.AreEqual(10, ps.ClosableCount);
            Assert.IsTrue(135.714m - ps.BuyPrice < 0.001m);
            Assert.IsTrue(1357.14m - ps.BuyTotal < 0.01m);
            Assert.IsTrue(-557.14m - ps.FloatProfit < 0.01m);
            Assert.IsTrue(-42.85m - ps.CloseProfit < 0.01m);
            Assert.AreEqual(800, ps.TotalValue);
        }

        [TestMethod]
        public void CreateTest15()
        {
            UserPosition up = createPosition(10, 90,
                OrderType.开仓, TradeDirectType.买);

            var ps = new PositionSummary(up, 90m);

            var up1 = createPosition(20, 120,
                OrderType.开仓, TradeDirectType.买);
            ps.Update(up1, true, 120);

            var up2 = createPosition(5, 200,
                OrderType.平仓, TradeDirectType.卖);
            ps.Update(up2, false, 200);

            var up3 = createPosition(10, 200,
                OrderType.开仓, TradeDirectType.买);
            ps.Update(up3, true, 200);

            var up4 = createPosition(15, 140,
                OrderType.平仓, TradeDirectType.卖);
            ps.Update(up4, false, 140);

            Assert.AreEqual(20, ps.Count);
            //Assert.AreEqual(20, ps.ClosableCount);
            Assert.IsTrue(135.714m - ps.BuyPrice < 0.001m);
            Assert.IsTrue(2714.28m - ps.BuyTotal < 0.01m);
            Assert.IsTrue(85.72m - ps.FloatProfit < 0.01m);
            Assert.IsTrue(514.29m - ps.CloseProfit < 0.01m);
            Assert.AreEqual(2800, ps.TotalValue);
        }


        [TestMethod]
        public void CreateTest14()
        {
            UserPosition up = createPosition(10, 90,
                OrderType.开仓, TradeDirectType.买);

            var ps = new PositionSummary(up, 90m);

            var up1 = createPosition(20, 120,
                OrderType.开仓, TradeDirectType.买);
            ps.Update(up1, true, 120);

            var up2 = createPosition(5, 200,
                OrderType.平仓, TradeDirectType.卖);
            ps.Update(up2, false, 200);

            var up3 = createPosition(10, 200,
                OrderType.开仓, TradeDirectType.买);
            ps.Update(up3, true, 200);

            Assert.AreEqual(35, ps.Count);
            //Assert.AreEqual(35, ps.ClosableCount);
            Assert.IsTrue(135.714m - ps.BuyPrice < 0.001m);
            Assert.IsTrue(4750m - ps.BuyTotal < 0.01m);
            Assert.IsTrue(2250m - ps.FloatProfit < 0.01m);
            Assert.IsTrue(450m - ps.CloseProfit < 0.01m);
            Assert.AreEqual(7000, ps.TotalValue);
        }


        [TestMethod]
        public void CreateTest13()
        {
            UserPosition up = createPosition(10, 90,
                OrderType.开仓, TradeDirectType.买);

            var ps = new PositionSummary(up, 90m);

            var up1 = createPosition(20, 120,
                OrderType.开仓, TradeDirectType.买);
            ps.Update(up1, true, 120);

            var up2 = createPosition(5, 200,
                OrderType.平仓, TradeDirectType.卖);
            ps.Update(up2, false, 200);

            Assert.AreEqual(25, ps.Count);
           // Assert.AreEqual(25, ps.ClosableCount);
            Assert.IsTrue(110m - ps.BuyPrice < 0.001m);
            Assert.IsTrue(2750m - ps.BuyTotal < 0.01m);
            Assert.IsTrue(2250m - ps.FloatProfit < 0.01m);
            Assert.IsTrue(450m - ps.CloseProfit < 0.01m);
            Assert.AreEqual(5000, ps.TotalValue);
        }


        [TestMethod]
        public void CreateTest12()
        {
            UserPosition up = createPosition(10, 90,
                OrderType.开仓, TradeDirectType.买);

            var ps = new PositionSummary(up, 90m);

            var up1 = createPosition(20, 120,
                OrderType.开仓, TradeDirectType.买);
            ps.Update(up1, true, 120);

            Assert.AreEqual(30, ps.Count);
            //Assert.AreEqual(30, ps.ClosableCount);
            Assert.IsTrue(110m - ps.BuyPrice < 0.001m);
            Assert.IsTrue(3300m - ps.BuyTotal < 0.01m);
            Assert.IsTrue(300m - ps.FloatProfit < 0.01m);
            Assert.IsTrue(0m - ps.CloseProfit < 0.01m);
            Assert.AreEqual(3600, ps.TotalValue);
        }

        [TestMethod]
        public void CreateTest11()
        {
            UserPosition up = createPosition(10, 90,
                OrderType.开仓, TradeDirectType.买);


            var ps = new PositionSummary(up, 90m);
            Assert.AreEqual(10, ps.Count);
            //Assert.AreEqual(10, ps.ClosableCount);
            Assert.AreEqual(90, ps.BuyPrice);
            Assert.AreEqual(900, ps.BuyTotal);
            Assert.AreEqual(0, ps.FloatProfit);
            Assert.AreEqual(0, ps.CloseProfit);
            Assert.AreEqual(900, ps.TotalValue);
        }


        [TestMethod]
        public void CreateTest7()
        {
            UserPosition up = createPosition(10, 90,
                OrderType.开仓, TradeDirectType.卖);
            var ps = new PositionSummary(up, 90m);

            var up1 = createPosition(20, 120,
                OrderType.开仓, TradeDirectType.卖);
            ps.Update(up1, true, 120);

            var up2 = createPosition(5, 200, OrderType.平仓,
                TradeDirectType.买);
            ps.Update(up2, false, 200);

            var up3 = createPosition(10, 200, OrderType.开仓,
                TradeDirectType.卖);
            ps.Update(up3, true, 200);

            var up4 = createPosition(15, 140, OrderType.平仓,
                TradeDirectType.买);
            ps.Update(up4, false, 140);

            var up5 = createPosition(10, 80, OrderType.平仓,
                TradeDirectType.买);
            ps.Update(up5, false, 80);


            var up6 = createPosition(10, 90, OrderType.平仓,
                TradeDirectType.买);
            ps.Update(up6, false, 90);

            Assert.AreEqual(0, ps.Count);
           // Assert.AreEqual(0, ps.ClosableCount);
            Assert.IsTrue(0m - ps.BuyPrice < 0.001m);
            Assert.IsTrue(0m - ps.BuyTotal < 0.01m);
            Assert.IsTrue(0m - ps.FloatProfit < 0.01m);
            Assert.IsTrue(0m - ps.CloseProfit < 0.01m);
            Assert.AreEqual(0, ps.TotalValue);
        }


        [TestMethod]
        public void CreateTest6()
        {
            UserPosition up = createPosition(10, 90,
                OrderType.开仓, TradeDirectType.卖);
            var ps = new PositionSummary(up, 90m);

            var up1 = createPosition(20, 120,
                OrderType.开仓, TradeDirectType.卖);
            ps.Update(up1, true, 120);

            var up2 = createPosition(5, 200, OrderType.平仓,
                TradeDirectType.买);
            ps.Update(up2, false, 200);

            var up3 = createPosition(10, 200, OrderType.开仓,
                TradeDirectType.卖);
            ps.Update(up3, true, 200);

            var up4 = createPosition(15, 140, OrderType.平仓,
                TradeDirectType.买);
            ps.Update(up4, false, 140);

            var up5 = createPosition(10, 80, OrderType.平仓,
                TradeDirectType.买);
            ps.Update(up5, false, 80);


            Assert.AreEqual(10, ps.Count);
           // Assert.AreEqual(10, ps.ClosableCount);
            Assert.IsTrue(135.714m - ps.BuyPrice < 0.001m);
            Assert.IsTrue(1357.14m - ps.BuyTotal < 0.01m);
            Assert.IsTrue(557.14m - ps.FloatProfit < 0.01m);
            Assert.IsTrue(-42.85m - ps.CloseProfit < 0.01m);
            Assert.AreEqual(-800, ps.TotalValue);
        }


        [TestMethod]
        public void CreateTest5()
        {
            UserPosition up = createPosition(10, 90,
                OrderType.开仓, TradeDirectType.卖);
            var ps = new PositionSummary(up, 90m);

            var up1 = createPosition(20, 120,
                OrderType.开仓, TradeDirectType.卖);
            ps.Update(up1, true, 120);

            var up2 = createPosition(5, 200, OrderType.平仓,
                TradeDirectType.买);
            ps.Update(up2, false, 200);

            var up3 = createPosition(10, 200, OrderType.开仓,
                TradeDirectType.卖);
            ps.Update(up3, true, 200);

            var up4 = createPosition(15, 140, OrderType.平仓,
                TradeDirectType.买);
            ps.Update(up4, false, 140);


            Assert.AreEqual(20, ps.Count);
           // Assert.AreEqual(20, ps.ClosableCount);
            Assert.IsTrue(135.714m - ps.BuyPrice < 0.001m);
            Assert.IsTrue(2714.28m- ps.BuyTotal<0.01m);
            Assert.IsTrue(-85.72m - ps.FloatProfit < 0.01m);
            Assert.IsTrue(-514.29m - ps.CloseProfit < 0.01m);
            Assert.AreEqual(-2800, ps.TotalValue);
        }


        [TestMethod]
        public void CreateTest4()
        {
            UserPosition up = createPosition(10, 90,
                OrderType.开仓, TradeDirectType.卖);
            var ps = new PositionSummary(up, 90m);

            var up1 = createPosition(20, 120,
                OrderType.开仓, TradeDirectType.卖);
            ps.Update(up1, true, 120);

            var up2 = createPosition(5, 200, OrderType.平仓,
                TradeDirectType.买);
            ps.Update(up2, false, 200);

            var up3 = createPosition(10, 200, OrderType.开仓,
                TradeDirectType.卖);
            ps.Update(up3, true, 200);


            Assert.AreEqual(35, ps.Count);
          //  Assert.AreEqual(35, ps.ClosableCount);
            Assert.IsTrue(135.714m- ps.BuyPrice<0.001m);
            Assert.AreEqual(4750, ps.BuyTotal);
            Assert.AreEqual(-2250, ps.FloatProfit);
            Assert.AreEqual(-450, ps.CloseProfit);
            Assert.AreEqual(-7000, ps.TotalValue);
        }
        [TestMethod]
        public void CreateTest3()
        {
            UserPosition up = createPosition(10, 90,
                OrderType.开仓, TradeDirectType.卖);
            var ps = new PositionSummary(up, 90m);

            var up1 = createPosition(20, 120,
                OrderType.开仓, TradeDirectType.卖);
            ps.Update(up1, true, 120);

            var up2 = createPosition(5, 200, OrderType.平仓,
                TradeDirectType.买);
            ps.Update(up2, false, 200);


            Assert.AreEqual(25, ps.Count);
          //  Assert.AreEqual(25, ps.ClosableCount);
            Assert.AreEqual(110, ps.BuyPrice);
            Assert.AreEqual(2750, ps.BuyTotal);
            Assert.AreEqual(-2250, ps.FloatProfit);
            Assert.AreEqual(-450, ps.CloseProfit);
            Assert.AreEqual(-5000, ps.TotalValue);
        }

        [TestMethod]
        public void CreateTest2()
        {
            UserPosition up = createPosition(10, 90,
                OrderType.开仓, TradeDirectType.卖);
            var ps = new PositionSummary(up, 90m);

            var up1 = createPosition(20, 120, 
                OrderType.开仓, TradeDirectType.卖);
            ps.Update(up1, true, 120);


            Assert.AreEqual(30, ps.Count);
          //  Assert.AreEqual(30, ps.ClosableCount);
            Assert.AreEqual(110, ps.BuyPrice);
            Assert.AreEqual(3300, ps.BuyTotal);
            Assert.AreEqual(-300, ps.FloatProfit);
            Assert.AreEqual(0, ps.CloseProfit);
            Assert.AreEqual(-3600, ps.TotalValue);
        }
        [TestMethod]
        public void CreateTest()
        {
            UserPosition up = createPosition(10, 90,
                OrderType.开仓, TradeDirectType.卖);


            var ps = new PositionSummary(up, 90m);
            Assert.AreEqual(10, ps.Count);
           // Assert.AreEqual(10, ps.ClosableCount);
            Assert.AreEqual(90, ps.BuyPrice);
            Assert.AreEqual(900, ps.BuyTotal);
            Assert.AreEqual(0, ps.FloatProfit);
            Assert.AreEqual(0, ps.CloseProfit);
            Assert.AreEqual(-900, ps.TotalValue);
        }


        [TestMethod]
        public void TestMethod1()
        {
            List<Order> l1=new List<Order> ();
            List<Order> l2=new List<Order> ();
            object ll1=new object ();
            object ll2=new object ();

            for (int i = 0; i < 10; i++)
            {
                var o = new Order { Id = i + 1, OrderTime = DateTime.Now.AddDays(-2),
                                    Price = 1,
                                    Contract = new Contract {  Name="test"}
                };
                l1.Add(o);
                l2.Add(o);
            }
            for (int i = 10; i < 20; i++)
            {
                var o = new Order
                {
                    Id = i + 1,
                    OrderTime = DateTime.Now,
                    Price = 1,
                    Contract = new Contract { Name = "test" }
                };
                l1.Add(o);
                l2.Add(o);
            }

            OrderLifeManager<Order> olm = new OrderLifeManager<Order>(l1, l2, ll1, ll2);
            Singleton<TextLog>.Instance.Flush();

            Assert.AreEqual(10, l1.Count);
            Assert.AreEqual(10, l2.Count);
        }

        

        [TestMethod]
        public void block_chain_test()
        {
            BlockChainBtcPay bcb = new BlockChainBtcPay();
            var r = bcb.GenerateAddress("hello");
            Assert.IsNotNull(r);
        }
    }
}

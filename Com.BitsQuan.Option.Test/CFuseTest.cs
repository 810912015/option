using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Com.BitsQuan.Option.Match;
using Com.BitsQuan.Option.Core;

namespace Com.BitsQuan.Option.Test
{
    [TestClass]
    public class ContractFuseTest
    {
        [TestMethod]
        public void edit_price_test()
        {
            BtcPrice.SetPrice(2000);
            Contract c = new Contract
            {
                Id = 1,
                Name = "1",
                Code = "1",
                ExcutePrice = 2100,
                ContractType = ContractType.期权,
                OptionType = OptionType.认购期权
            };
            ContractFuse cf = new ContractFuse(c);

            cf.Handle(new Deal { Price = 300 });

            Assert.AreEqual(600, cf.MaxPrice);
            Assert.AreEqual(150, cf.MinPrice);
            Assert.AreEqual(false, cf.IsFusing);

            Order o1 = new Order { Price = 700, Direction = TradeDirectType.买, OrderType = OrderType.开仓 };
            var r1 = cf.ShouldAccept(o1);
            Assert.AreEqual(600, o1.Price);
            Assert.AreEqual(true, r1);

            Order o2= new Order { Price = 700, Direction = TradeDirectType.卖, OrderType = OrderType.开仓 };
            var r2 = cf.ShouldAccept(o2);
            Assert.AreEqual(700, o2.Price);
            Assert.AreEqual(true, r2);

            Order o3 = new Order { Price = 70, Direction = TradeDirectType.卖, OrderType = OrderType.开仓 };
            var r3 = cf.ShouldAccept(o3);
            Assert.AreEqual(150, o3.Price);
            Assert.AreEqual(true, r3);


            Order o4 = new Order { Price = 70, Direction = TradeDirectType.买, OrderType = OrderType.开仓 };
            var r4 = cf.ShouldAccept(o4);
            Assert.AreEqual(70, o4.Price);
            Assert.AreEqual(true, r4);

        }


        [TestMethod]
        public void handle_deal_test()
        {
            BtcPrice.SetPrice(2000);
            Contract c=new Contract {Id =1,Name="1", Code="1",ExcutePrice=2100, ContractType=ContractType.期权,
            OptionType=OptionType.认购期权};
            ContractFuse cf = new ContractFuse(c);

            cf.Handle(new Deal { Price = 300 });

            Assert.AreEqual(600,cf.MaxPrice);
            Assert.AreEqual(150, cf.MinPrice);
            Assert.AreEqual(false, cf.IsFusing);
        }

        [TestMethod]
        public void fuse_max()
        {
            BtcPrice.SetPrice(2000);
            Contract c = new Contract
            {
                Id = 1,
                Name = "1",
                Code = "1",
                ExcutePrice = 2100,
                ContractType = ContractType.期权,
                OptionType = OptionType.认购期权
            };
            ContractFuse cf = new ContractFuse(c);

            cf.Handle(new Deal { Price = 300 });

            Assert.AreEqual(600, cf.MaxPrice);
            Assert.AreEqual(150, cf.MinPrice);
            Assert.AreEqual(false, cf.IsFusing);

            cf.Handle(new Deal { Price = 700 });
            Assert.AreEqual(true, cf.IsFusing);

            Order o1 = new Order { Price = 700, Direction = TradeDirectType.卖, OrderType = OrderType.开仓 };
            var r1 = cf.ShouldAccept(o1);
            Assert.AreEqual(true, r1);

            Order o2 = new Order { Price = 70, Direction = TradeDirectType.卖, OrderType = OrderType.开仓 };
            var r2 = cf.ShouldAccept(o2);
            Assert.AreEqual(false, r2);

            Order o3 = new Order { Price = 700, Direction = TradeDirectType.买, OrderType = OrderType.开仓 };
            var r3 = cf.ShouldAccept(o3);
            Assert.AreEqual(false, r3);

            Order o4 = new Order { Price = 70, Direction = TradeDirectType.买, OrderType = OrderType.开仓 };
            var r4 = cf.ShouldAccept(o4);
            Assert.AreEqual(true, r4);
        }


        [TestMethod]
        public void fuse_max_2()
        {
            BtcPrice.SetPrice(2000);
            Contract c = new Contract
            {
                Id = 1,
                Name = "1",
                Code = "1",
                ExcutePrice = 2100,
                ContractType = ContractType.期权,
                OptionType = OptionType.认沽期权
            };
            ContractFuse cf = new ContractFuse(c);

            cf.Handle(new Deal { Price = 300 }); 

            cf.Handle(new Deal { Price = 700 });
            Assert.AreEqual(true, cf.IsFusing);

            Order o1 = new Order { Price = 700, Direction = TradeDirectType.卖, OrderType = OrderType.开仓 };
            var r1 = cf.ShouldAccept(o1);
            Assert.AreEqual(true, r1);

            Order o2 = new Order { Price = 70, Direction = TradeDirectType.卖, OrderType = OrderType.开仓 };
            var r2 = cf.ShouldAccept(o2);
            Assert.AreEqual(false, r2);

            Order o3 = new Order { Price = 700, Direction = TradeDirectType.买, OrderType = OrderType.开仓 };
            var r3 = cf.ShouldAccept(o3);
            Assert.AreEqual(false, r3);

            Order o4 = new Order { Price = 70, Direction = TradeDirectType.买, OrderType = OrderType.开仓 };
            var r4 = cf.ShouldAccept(o4);
            Assert.AreEqual(true, r4);
        }


        [TestMethod]
        public void handle_deal_test_2()
        {
            BtcPrice.SetPrice(2000);
            Contract c = new Contract
            {
                Id = 1,
                Name = "1",
                Code = "1",
                ExcutePrice = 2100,
                ContractType = ContractType.期权,
                OptionType = OptionType.认沽期权
            };
            ContractFuse cf = new ContractFuse(c);

            cf.Handle(new Deal { Price = 300 });

            Assert.AreEqual(510, cf.MaxPrice);
            Assert.AreEqual(150, cf.MinPrice);
            Assert.AreEqual(false, cf.IsFusing);
        }
        [TestMethod]
        public void accept_max_test()
        {
            BtcPrice.SetPrice(2000);
            Contract c = new Contract
            {
                Id = 1,
                Name = "1",
                Code = "1",
                ExcutePrice = 2100,
                ContractType = ContractType.期权,
                OptionType = OptionType.认购期权
            };
            ContractFuse cf = new ContractFuse(c);

            cf.Handle(new Deal { Price = 300 });

            Order o = new Order { Price = 700, Direction = TradeDirectType.买, OrderType = OrderType.开仓 };
            var r = cf.ShouldAccept(o);
            Assert.AreEqual(true, r);
        }
        [TestMethod]
        public void accept_max_test_2()
        {
            BtcPrice.SetPrice(2000);
            Contract c = new Contract
            {
                Id = 1,
                Name = "1",
                Code = "1",
                ExcutePrice = 2100,
                ContractType = ContractType.期权,
                OptionType = OptionType.认购期权
            };
            ContractFuse cf = new ContractFuse(c);

            cf.Handle(new Deal { Price = 300 });

            Order o = new Order { Price = 70, Direction = TradeDirectType.买, OrderType = OrderType.开仓 };
            var r = cf.ShouldAccept(o);
            Assert.AreEqual(true, r);
        }
    }
}

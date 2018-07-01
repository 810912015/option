using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Com.BitsQuan.Option.Provider;
using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Match;


namespace Com.BitsQuan.Option.Test
{
    //[TestClass]
    //public class FuseTest
    //{

    //    [TestMethod]
    //    public void GetOrderValue_Test()
    //    { //熔断机制测试（购）
    //        var fuse = new Fuse();
    //        Contract contract = new Contract//合约
    //        {
    //            OptionType = OptionType.认沽期权,//认沽
    //            ExcutePrice = 2000//行权价
    //        };

    //        Order order = new Order
    //        {
    //            Contract = contract,
    //            Direction = TradeDirectType.买,//买
    //            Price = 101,//期权价
    //            positionType = PositionType.义务仓,//义务/权利仓
    //            OrderType = OrderType.平仓
    //        };
    //        var r = fuse.GetOrderValue(order);
    //        Assert.AreEqual("期权价格：101,行权价：2000,期权类型：CallOption,type：买平", r);
    //    }
    //    //[TestMethod]
    //    //public void GetTimeByPrice_Test()
    //    //{ //熔断机制测试（购）
    //    //    var fuse = new Fuse();
    //    //    fuse.BTClow = 1000;
    //    //    fuse.BTChei = 2000;
    //    //    var r = fuse.GetTimeByPrice(OptionType.CallOption, 2000);
    //    //    Assert.AreEqual("最高熔断价：110,最低熔断价：50", r);
    //    //}

    //    //[TestMethod]
    //    //public void GetTimeByPrice_Test2()
    //    //{ //熔断机制测试（沽）
    //    //    var fuse = new Fuse();
    //    //    var r = fuse.GetTimeByPrice(OptionType.PutOption, 2000);
    //    //    Assert.AreEqual("最高熔断价：210,最低熔断价：50", r);
    //    //}

    //    //[TestMethod]
    //    //public void FuseMechFalseOrTrue_Test()
    //    //{ //没触发上机制或下机制
    //    //    var fuse = new Fuse();
    //    //    //    fuse.State = null;
    //    //    var r = fuse.FuseMechFalseOrTrue(100, "买开");
    //    //    Assert.AreEqual(null, r);
    //    //}

    //    //[TestMethod]
    //    //public void FuseMechFalseOrTrue_Test2()
    //    //{ //触发上机制的允许委托（买平，卖开）
    //    //    var fuse = new Fuse();
    //    //    //  fuse.State = "上涨机制";
    //    //    //   fuse.UpMax = 110;
    //    //    var r = fuse.FuseMechFalseOrTrue(100, "卖开");
    //    //    Assert.AreEqual("允许委托", r);
    //    //}

    //    //[TestMethod]
    //    //public void FuseMechFalseOrTrue_Test3()
    //    //{ //触发上机制的不允许
    //    //    var fuse = new Fuse();
    //    //    //  fuse.State = "上涨机制";
    //    //    //  fuse.UpMax = 110;
    //    //    var r = fuse.FuseMechFalseOrTrue(100, "买开");
    //    //    Assert.AreEqual("上涨熔断机制开启，禁止买开或卖平,以及大于熔断价格的买平卖开", r);
    //    //}

    //    //[TestMethod]
    //    //public void FuseMechFalseOrTrue_Test4()
    //    //{ //触发下机制的允许委托（买开，卖平）
    //    //    var fuse = new Fuse();
    //    //    //  fuse.State = "下跌机制";
    //    //    //    fuse.DownMax = 10;
    //    //    var r = fuse.FuseMechFalseOrTrue(11, "卖平");
    //    //    Assert.AreEqual("允许委托", r);
    //    //}
    //    //[TestMethod]
    //    //public void FuseMechFalseOrTrue_Test5()
    //    //{ //触发下机制的不允许
    //    //    var fuse = new Fuse();
    //    //    //  fuse.State = "下跌机制";
    //    //    //  fuse.UpMax = 10;
    //    //    var r = fuse.FuseMechFalseOrTrue(11, "买平");
    //    //    Assert.AreEqual("下跌熔断机制开启，禁止买开或卖开,以及小于熔断价格的卖平买开", r);
    //    //}

    //    ////是否开启熔断机制
    //    //[TestMethod]
    //    //public void OpenFuseMech_Test()
    //    //{ //不开启
    //    //    var fuse = new Fuse();
    //    //    //fuse.UpMax = 110;
    //    //    //fuse.DownMax = 10;
    //    //    var r = fuse.OpenFuseMech(80, "卖开");
    //    //    Assert.AreEqual(null, r);
    //    //}

    //    //[TestMethod]
    //    //public void OpenFuseMech_Test2()
    //    //{ //开启上涨
    //    //    var fuse = new Fuse();
    //    //    //fuse.UpMax = 110;
    //    //    //fuse.DownMax = 10;
    //    //    var r = fuse.OpenFuseMech(111, "卖开");
    //    //    Assert.AreEqual("上涨熔断机制开启，禁止买开或卖平,以及大于熔断价格的买平卖开", r);
    //    //}

    //    //[TestMethod]
    //    //public void OpenFuseMech_Test3()
    //    //{ //开启下跌
    //    //    var fuse = new Fuse();
    //    //    //fuse.UpMax = 110;
    //    //    //fuse.DownMax = 10;
    //    //    var r = fuse.OpenFuseMech(9, "卖开");
    //    //    Assert.AreEqual("下跌熔断机制开启，禁止买开或卖开,以及小于熔断价格的卖平买开", r);
    //    //}

    //    [TestMethod]
    //    public void StartFuseMech_Test()
    //    { //没超出5%,不跟踪机制的情况
    //        var fuse = new Fuse();
    //        Contract contract = new Contract//合约
    //        {
    //            OptionType = OptionType.认沽期权,//认沽
    //            ExcutePrice = 2000//行权价
    //        };

    //        Order order = new Order
    //        {
    //            Contract = contract,
    //            Direction = TradeDirectType.买,//买
    //            Price = 20,//期权价
    //            positionType = PositionType.权利仓//义务/权利仓
    //        };
    //        var r = fuse.StartFuseMech(order, 2000);//(买平)
    //        Assert.AreEqual(null, r);
    //    }

    //    [TestMethod]
    //    public void StartFuseMech_Test2()
    //    { //超出5%,跟踪机制的情况
    //        var fuse = new Fuse();
    //        Contract contract = new Contract//合约
    //        {
    //            OptionType = OptionType.认沽期权,//认沽
    //            ExcutePrice = 2000//行权价
    //        };

    //        Order order = new Order
    //        {
    //            Contract = contract,
    //            Direction = TradeDirectType.买,//买
    //            Price = 101,//期权价
    //            positionType = PositionType.权利仓//义务/权利仓
    //        };
    //        var r = fuse.StartFuseMech(order, 2000);//(买平)
    //        Assert.AreEqual(null, r);
    //    }


    //    [TestMethod]
    //    public void StartFuseMech_Test3()
    //    { //机制跟踪后开启的情况(5分钟内,允许委托的条件)
    //        var fuse = new Fuse();
    //        Contract contract = new Contract//合约
    //        {
    //            OptionType = OptionType.认沽期权,//认沽
    //            ExcutePrice = 2000//行权价
    //        };

    //        Order order = new Order
    //        {
    //            Contract = contract,
    //            Direction = TradeDirectType.买,//买
    //            Price = 101,//期权价
    //            positionType = PositionType.权利仓,//义务/权利仓
    //            OrderType = OrderType.平仓
    //        };
    //        //   fuse.Time = System.DateTime.Now;
    //        //   fuse.State = "上涨机制";
    //        // fuse.UpMax = 200;
    //        var r = fuse.StartFuseMech(order, 2000);//(买平)
    //        Assert.AreEqual("允许委托", r);
    //    }


    //    [TestMethod]
    //    public void StartFuseMech_Test4()
    //    { //机制跟踪后开启的情况(5分钟内,允许委托的条件)
    //        var fuse = new Fuse();
    //        Contract contract = new Contract//合约
    //        {
    //            OptionType = OptionType.认购期权,//认沽
    //            ExcutePrice = 2000,//行权价
    //            Name = "BTC20150101购100",
    //            Code = "115001"
    //        };

    //        Order order = new Order
    //        {
    //            Contract = contract,
    //            Direction = TradeDirectType.卖,//买
    //            Price = 222,//期权价
    //            positionType = PositionType.权利仓,//义务/权利仓
    //            OrderType = OrderType.平仓
    //        };
    //      //MarketItem mk = MarketItem.Create(order);
    //      //   //fuse.MarketItems.Add(mk);
         
    //      //  mk.RiseTime = System.DateTime.Now;
    //      //  mk.RiseState = "上涨机制";
    //      //  MarketItem.GetTimeByPrice(OptionType.认购期权, 2000, Convert.ToDateTime(""), "", Convert.ToDateTime(""), "", 0, 0, 0, 0, 2000, 2000);
    //      //  var r = fuse.StartFuseMech(order, 2000);//(买平)

     
    //     //   StartFuseMech_Test5();
    //      //  Assert.AreEqual("允许委托", r);
    //    }

    //    public void StartFuseMech_Test5()
    //    { //机制跟踪后开启的情况(5分钟内,允许委托的条件)
    //        var fuse = new Fuse();
    //        Contract contract = new Contract//合约
    //        {
    //            OptionType = OptionType.认购期权,//认沽
    //            ExcutePrice = 2000,//行权价
    //            Name = "BTC20150101购100",
    //            Code = "115001"
    //        };

    //        Order order = new Order
    //        {
    //            Contract = contract,
    //            Direction = TradeDirectType.买,//买
    //            Price = 101,//期权价
    //            positionType = PositionType.权利仓,//义务/权利仓
    //            OrderType = OrderType.平仓
    //        };
    //   //     MarketItem mk = MarketItem.Create(order);
    //   ////     fuse.MarketItems.Add(mk);
    //   //     mk.RiseTime = System.DateTime.Now;
    //   //     mk.FallState = "上涨机制";
    //   //     var r = fuse.StartFuseMech(order, 2000);//(买平)
    //   //     Assert.AreEqual("允许委托", r);
    //    }
    //}
}

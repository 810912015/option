using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Miscellaneous;
using Com.BitsQuan.Option.Ui.Models;

namespace Com.BitsQuan.Option.Test
{
    [TestClass]
    public class BtcPriceTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            var r = BtcPriceGenerator.GetBtcChina();
        }

        [TestMethod]
        public void TestMethod2()
        {
            var r = BtcPriceGenerator.GetCnBtc();
        }
        [TestMethod]
        public void our_test()
        {
            var r = BtcPriceGenerator.Our();
        }


        BlockChainBtcPay bb = new BlockChainBtcPay();
        [TestMethod]
        public void rtest()
        {
            var hh = bb.CallBack(300000000, "*OK", "1HsuPyJ6KLnepAgZavznDM5PfAPQMGMTUm", 300000000, "hello1");
        //    bb.OnSuccess+=bb_OnSuccess;    
           // return hh;
        }
        //ApplicationDbContext aa = new ApplicationDbContext();
        //public void bb_OnSuccess(decimal btc, string address,decimal qrBtc,string user)
        //{
        //    //根据用户名称查Uid

        //    var bo = new BankRecord
        //    {
        //        Id = 0,
        //        AddressNum = address,
        //        Num = btc,
        //        AppUserName =user,
        //        BankRecordType = BankRecordType.充值,
        //        When = DateTime.Now,
        //        Address = address,
        //        coin = "BTC",
        //        Uid = "1",
        //        ActualDelta = qrBtc,//审核通过金额（实际充值金额）
        //        Delta = btc,//用户输入金额
        //        IsApproved = true//审核成功

        //    };
        //    adb.BankRecords.Add(bo);

        //   // var r = adb.SaveChanges() > 0;

        //}
    }
}

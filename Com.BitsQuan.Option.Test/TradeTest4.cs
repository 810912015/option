using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Com.BitsQuan.Option.Imp;
using System.Linq;
using Com.BitsQuan.Option.Core;
using System.Collections.Generic;
using System.Diagnostics;

namespace Com.BitsQuan.Option.Test
{
    [TestClass]
    public class TradeTest4:TradeBase
    {
        [TestMethod]
        public void s_o_b_o_all_5()
        {
            RestoreDb();
            ms = new Match.MatchService();
            OrderPreHandler.CountPerMinuteLimit = 1000000;
            var cs = ms.Model.Contracts.Where(a => a.IsDel == false).ToList();
            var ts = ms.Model.Traders.ToList();
            List<OrderResult> l = new List<OrderResult>();
            Stopwatch sw = new Stopwatch();
            sw.Start();
            //每种合约,每个人交易10次
            for(int i=0;i<cs.Count;i++)
                for (int j = 0; j < ts.Count; j++)
                    for (int k = 0; k < 5; k++)
                    {
                        var r = ms.AddOrder(ts[j].Id, cs[i].Id,
                            TradeDirectType.卖, OrderType.开仓, OrderPolicy.限价申报,
                            1, 1);
                        l.Add(r);
                    }
            for (int i = 0; i < cs.Count; i++)
                for (int j = 0; j < ts.Count; j++)
                    for (int k = 0; k < 5; k++)
                    {
                        var r = ms.AddOrder(ts[j].Id, cs[i].Id,
                            TradeDirectType.买, OrderType.开仓, OrderPolicy.限价申报,
                            1, 1);
                        l.Add(r);
                    }
            sw.Stop();
            Assert.IsTrue(sw.Elapsed.TotalMilliseconds<20000);
            for (int i = 0; i < l.Count; i++)
            {
                var t = l[i];
                Assert.AreEqual(t.ResultCode, 0);
            }
        }
    }
}

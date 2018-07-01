using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Com.BitsQuan.Option.Match;
using Com.BitsQuan.Option.Core;
using System.Linq;
using Com.BitsQuan.Option.Imp;
using System.Data.Entity;
using Com.BitsQuan.Option.Provider;
using Com.BitsQuan.Option.Provider.Migrations;
using Com.BitsQuan.Option.Match.Imp;
using System.Threading;
using System.Collections.Generic;

namespace Com.BitsQuan.Option.Test
{
    [TestClass]
    public class TradeTest
    {
        /// <summary>
        /// sell_open_s1.price_s1.count..._buy_close_b1.price_b1.count
        /// </summary>
        [TestMethod]
        public void s_o_1_1_2_2_b_o_3_2()
        {
            RestoreDb();
            MatchService ms = new MatchService();
            var t1 = ms.Model.Traders.Where(a => a.Id == 1).First();
            var t2 = ms.Model.Traders.Where(a => a.Id == 2).First();
            var c = ms.Model.Contracts.Where(a => a.Id == 1&&a.IsDel==false).First();

            //1块钱卖1个
            var r1 = ms.AddOrder(t1.Id, 1, TradeDirectType.卖, OrderType.开仓, OrderPolicy.限价申报, 1, 1);
            //2块钱卖2个
            var r2 = ms.AddOrder(t1.Id, 1, TradeDirectType.卖, OrderType.开仓, OrderPolicy.限价申报, 2, 2);
            //3块钱买2个
            var r3 = ms.AddOrder(t2.Id, 1, TradeDirectType.买, OrderType.开仓, OrderPolicy.限价申报, 2, 3);

            //成交
            vo_s_dp_dc(r1.Order, OrderState.已成交, 1, 1);
            vo_s_dp_dc(r2.Order, OrderState.部分成交, 2,1);
            vo_s_dp_dc(r3.Order, OrderState.已成交, 2, 1); 
            //持仓
            var p1 = t1.GetPositionSummary(c.Code, PositionType.义务仓);
            vp_c_bp_m(p1, 2, 1.5m, 0);

            var p2 = t2.GetPositionSummary(c.Code,PositionType.权利仓);

            vp_c_bp_m(p2, 2, 1.5m, 0m);
            //金额
            var delta = (decimal)(1 * 1 + 2 * 1);
            var commission = (decimal)(0.001 * 3);
            vb_t_f_m(t1.Account.BailAccount,  orignalSum + delta - commission,2m, 0); 
            vb_t_f_m(t2.Account.BailAccount, (decimal)(orignalSum - delta - commission), 0m, 0m); 
        }

        [TestMethod]
        public void s_o_i_to10_i_to10_b_o_10_55()
        {
            RestoreDb();
            MatchService ms = new MatchService();
            var t1 = ms.Model.Traders.Where(a => a.Id == 1).First();
            var t2 = ms.Model.Traders.Where(a => a.Id == 2).First();
            var c = ms.Model.Contracts.Where(a => a.Id == 1&&a.IsDel==false).First();

            var sl = new List<OrderResult>();

            for (int i = 0; i < 10; i++)
            {
                var t = ms.AddOrder(t1.Id, c.Id, TradeDirectType.卖, OrderType.开仓, OrderPolicy.限价申报, i + 1, i + 1);
                sl.Add(t);
            }
            var b1 = ms.AddOrder(t2.Id, c.Id, TradeDirectType.买, OrderType.开仓, OrderPolicy.限价申报, 55, 10);
            //成交确认
            for (int i = 0; i < 10; i++)
            {
                vo_s_dp_dc(sl[i].Order, OrderState.已成交, i + 1, i + 1);
            }
            vo_s_dp_dc(b1.Order, OrderState.已成交, 10, 10);
            //持仓确认
            var p1 = t1.GetPositionSummary(c.Code, PositionType.义务仓);
            decimal bt=0;
            decimal bm=0;
            for(int i=0;i<10;i++){
                bt+=(decimal)((i+1)*(i+1));
                bm+=0;
            }
            vp_c_bp_m(p1, 55, bt / 55m, bm);


            var p2 = t2.GetPositionSummary(c.Code, PositionType.权利仓);
            vp_c_bp_m(p2, 55, bt/55m, 0);
            //金额确认
            vb_t_f_m(t1.Account.BailAccount, orignalSum + bt - (decimal)(0.001m * bt), 0, bm);
            vb_t_f_m(t2.Account.BailAccount, orignalSum - bt - (decimal)(0.001m * bt), 0, 0);
        }
        [TestMethod]
        public void close_all_position()
        {
            RestoreDb();
            MatchService ms = new MatchService();
            var t1 = ms.Model.Traders.Where(a => a.Id == 1).First();
            var t2 = ms.Model.Traders.Where(a => a.Id == 2).First();
            var c = ms.Model.Contracts.Where(a => a.Id == 1&&a.IsDel==false).First();

            var sl = new List<OrderResult>();

            for (int i = 0; i < 6; i++)
            {
                var t = ms.AddOrder(t1.Id, c.Id, TradeDirectType.卖, OrderType.开仓, OrderPolicy.限价申报, i + 1, i + 1);
                sl.Add(t);
            }
            var b1 = ms.AddOrder(t2.Id, c.Id, TradeDirectType.买, OrderType.开仓, OrderPolicy.限价申报, 21, 6);

            var t1bt = t1.Account.BailAccount.Total;
            var t2bt = t2.Account.BailAccount.Total;

            var sc = ms.AddOrder(t1.Id, c.Id, TradeDirectType.买, OrderType.平仓, OrderPolicy.限价申报, 21, 6);
            var bc = ms.AddOrder(t2.Id, c.Id, TradeDirectType.卖, OrderType.平仓, OrderPolicy.限价申报, 21, 6);

            vo_s_dp_dc(sc.Order, OrderState.已成交, 6, 21);
            vo_s_dp_dc(bc.Order, OrderState.已成交, 6, 21);

            var ps = t1.GetPositionSummary(c.Code, PositionType.义务仓);
            var pb = t2.GetPositionSummary(c.Code, PositionType.权利仓);
            vp_c_bp_m(ps, 0, 6, 0);
            vp_c_bp_m(pb, 0, 6, 0);

            vb_t_f_m(t1.Account.BailAccount, t1bt -(decimal)( 21 * 6) -(decimal) (21 * 6 * 0.001), 0, 0);
            vb_t_f_m(t2.Account.BailAccount, t2bt + (decimal)(21 * 6) - (decimal)(21 * 6 * 0.001), 0, 0);
        }

        #region helps
        void RestoreDb()
        {

            string sql = @"
USE [master]
/****** Object:  Database [Option-test]    Script Date: 11/26/2014 11:17:06 ******/
DROP DATABASE [Option-test]";
            var ds = new DBServer(@"Data Source=.\sqlexpress;Initial Catalog=master;Integrated Security=True");
            ds.ExecNonQuery(sql);
            ds.Dispose(); ds = null;

            var context = new OptionDbCtx();
            var initor = new TraderInitor();
            initor.Init(context);

            //var ci = new ContractInitor();
            //ci.Init(context);
        }
        decimal orignalSum = 1000000m;
        /// <summary>
        /// 确认委托
        /// </summary>
        /// <param name="o"></param>
        /// <param name="state"></param>
        /// <param name="donePrice"></param>
        /// <param name="doneCount"></param>
        void vo_s_dp_dc(Order o, OrderState state, decimal donePrice, int doneCount)
        {
            Assert.AreEqual(o.State, state);
            Assert.AreEqual(o.DonePrice, donePrice);
            Assert.AreEqual(o.DoneCount, doneCount);
        }
        /// <summary>
        /// 确认持仓
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="count"></param>
        /// <param name="buyPrice"></param>
        /// <param name="maintain"></param>
        void vp_c_bp_m(PositionSummary p1, int count, decimal buyPrice, decimal maintain)
        {
            Assert.AreEqual(p1.Count, count);
            Assert.AreEqual(p1.BuyPrice, buyPrice);
            //Assert.AreEqual(p1.Maintain, maintain);
        }
        /// <summary>
        /// 确认保证金
        /// </summary>
        /// <param name="ba"></param>
        /// <param name="total"></param>
        /// <param name="froze"></param>
        /// <param name="maintain"></param>
        void vb_t_f_m(BailAccount ba, decimal total, decimal froze, decimal maintain)
        {
            Assert.AreEqual(ba.Total, total);
            //Assert.AreEqual(ba.MaintainCount, maintain);
            Assert.AreEqual(ba.Frozen, froze);
        }
        #endregion

        #region before

        [TestMethod]
        public void time_convert()
        {
            long l = 1149120000000;
            var d = new DateTime(1970,1,1);
            var d2 = d.AddMilliseconds(l);
            Assert.AreNotEqual(DateTime.Now, d2);
        }
        #endregion
    }
    
}

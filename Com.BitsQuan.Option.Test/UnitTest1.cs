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
    /// <summary>
    /// 做一系列操作:每次下单都在前面下单的基础上,从上到下流水执行
    /// </summary>
    [TestClass]
    public class TadeTest2
    {
        #region helps
        static void RestoreDb()
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

            var ci = new ContractInitor();
            ci.Init(context);
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

        /// <summary>
        /// 确认熔断价格
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="count"></param>
        /// <param name="buyPrice"></param>
        /// <param name="maintain"></param>
        void vp_c_bp_m_fuce(MarketItem mt, int RiseFusePrice, decimal FallFusePrice, string RiseState, string FallState)
        {
            //Assert.AreEqual(mt.RiseFusePrice, RiseFusePrice);
            //Assert.AreEqual(mt.FallFusePrice, FallFusePrice);
            //Assert.AreEqual(mt.RiseState, RiseState);
            //Assert.AreEqual(mt.FallState, FallState);
        }
        #endregion

        static MatchService ms;
        static Trader t1 { get { return ms.Model.Traders.Where(a => a.Id == 1).First(); } }
        static Trader t2 { get { return ms.Model.Traders.Where(a => a.Id == 2).First(); } }
        static Contract c { get {return  ms.Model.Contracts.Where(a => a.Id == 1&&a.IsDel==false).First(); } }

        [TestInitialize()]
        public void Init()
        {
            RestoreDb();
            ms = new MatchService();
        }
        
        [TestMethod]
        public void close_all()
        {
            

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

            vb_t_f_m(t1.Account.BailAccount, t1bt - (decimal)(21 * 6) - (decimal)(21 * 6 * 0.001), 0, 0);
            vb_t_f_m(t2.Account.BailAccount, t2bt + (decimal)(21 * 6) - (decimal)(21 * 6 * 0.001), 0, 0);
        }

        //部分成交的情况。A卖21个，B买6个
        [TestMethod]
        public void buy_partial()
        {
            var sl = new List<OrderResult>();

            for (int i = 0; i < 6; i++)
            {
                var t = ms.AddOrder(t1.Id, c.Id, TradeDirectType.卖, OrderType.开仓, OrderPolicy.限价申报, i + 1, i + 1);
                sl.Add(t);
            }

            var b1 = ms.AddOrder(t2.Id, c.Id, TradeDirectType.买, OrderType.开仓, OrderPolicy.限价申报, 21, 3);
            for (int i = 0; i < 6; i++)
            {
                if (i + 1 <= 3)//价格小于等于3元的卖开才成交
                {
                    vo_s_dp_dc(sl[i].Order, OrderState.已成交, i + 1, i + 1);
                }
                else {
                    vo_s_dp_dc(sl[i].Order, OrderState.等待中, 0,0);
                
                }
            }
            vo_s_dp_dc(b1.Order, OrderState.部分成交, 3, 3);
        }

        //全部撤销
        [TestMethod]
        public void close_partial()
        {
            var sl = new List<OrderResult>();
            var b1 = ms.AddOrder(t2.Id, c.Id, TradeDirectType.买, OrderType.开仓, OrderPolicy.限价申报, 21, 6);
            ms.RedoOrder(t2.Id, c.Id);//t1用户撤单
            for (int i = 0; i < 6; i++)
            {
                var t = ms.AddOrder(t1.Id, c.Id, TradeDirectType.卖, OrderType.开仓, OrderPolicy.限价申报, i + 1, i + 1);
                sl.Add(t);
            }
            vo_s_dp_dc(b1.Order, OrderState.已撤销, 0, 0);//撤单
           
        }


        //计算卖开保证金(已被占用)
        [TestMethod]
        public void Aount_bail()
        {
            var t1bt = t1.Account.BailAccount.Total;
            var t2bt = t2.Account.BailAccount.Total;
            var t = ms.AddOrder(t1.Id, c.Id, TradeDirectType.卖, OrderType.开仓, OrderPolicy.限价申报, 1, 1);
            var d = ms.AddOrder(t2.Id, c.Id, TradeDirectType.买, OrderType.开仓, OrderPolicy.限价申报, 1, 1);

            vb_t_f_m(t1.Account.BailAccount, t1bt + (decimal)(1 * 1) - (decimal)(1 * 1 * 0.001), 0,201);
            vb_t_f_m(t2.Account.BailAccount, t2bt -(decimal)(1 * 1) - (decimal)(1 * 1 * 0.001), 0, 0);
        }



        //只要不是卖开则不需要保证金
        [TestMethod]
        public void Aount_not_bail()
        {
            var t1bt = t1.Account.BailAccount.Total;
            var t2bt = t2.Account.BailAccount.Total;
            var t = ms.AddOrder(t1.Id, c.Id, TradeDirectType.买, OrderType.开仓, OrderPolicy.限价申报, 1, 1);

            vb_t_f_m(t1.Account.BailAccount, t1bt, 1*1, 0);
        }

        //没有符合的成交要求
        [TestMethod]
        public void not_turnover()
        {
            var sl = new List<OrderResult>();

            for (int i = 0; i < 6; i++)
            {
                var t = ms.AddOrder(t1.Id, c.Id, TradeDirectType.卖, OrderType.开仓, OrderPolicy.限价申报, i + 1, i + 1);
                sl.Add(t);
            }

            var b1 = ms.AddOrder(t2.Id, c.Id, TradeDirectType.买, OrderType.开仓, OrderPolicy.限价申报, 21,(decimal)0.1);
            for (int i = 0; i < 6; i++)
            {
                    vo_s_dp_dc(sl[i].Order, OrderState.等待中, 0, 0);
            }
            vo_s_dp_dc(b1.Order, OrderState.等待中, 0, 0);
        }

        //计算持仓量(全买全卖)
        [TestMethod]
        public void get_positions()
        {
            var t1bt = t1.Account.BailAccount.Total;
            var t2bt = t2.Account.BailAccount.Total;
            var b1 = ms.AddOrder(t1.Id, c.Id, TradeDirectType.买, OrderType.开仓, OrderPolicy.限价申报, 10, 2);
            var b2 = ms.AddOrder(t2.Id, c.Id, TradeDirectType.卖, OrderType.开仓, OrderPolicy.限价申报, 10, 2);
            var ps = t1.GetPositionSummary(c.Code, PositionType.权利仓);
            var bs = t2.GetPositionSummary(c.Code, PositionType.义务仓);
            vo_s_dp_dc(b1.Order, OrderState.已成交, 2, 10);
            vo_s_dp_dc(b2.Order, OrderState.已成交, 2, 10);
            vb_t_f_m(t1.Account.BailAccount, t1bt - (decimal)(10 * 2) - (decimal)(10 * 2 * 0.001), 0, 0);
            vb_t_f_m(t2.Account.BailAccount, t2bt + (decimal)(10 * 2) - (decimal)(10 * 2 * 0.001), 0, 220);
            vp_c_bp_m(ps, 10, 2, 0);
            vp_c_bp_m(bs, 10, 2, 220);
        }

         [TestMethod]
        //计算持仓量(买10卖10卖平5)
        public void get_positions2() {
            var t1bt = t1.Account.BailAccount.Total;
            var t2bt = t2.Account.BailAccount.Total;
            var b1 = ms.AddOrder(t1.Id, c.Id, TradeDirectType.买, OrderType.开仓, OrderPolicy.限价申报, 10, 2);
            var b2 = ms.AddOrder(t2.Id, c.Id, TradeDirectType.卖, OrderType.开仓, OrderPolicy.限价申报, 10, 2);
            vo_s_dp_dc(b1.Order, OrderState.已成交, 2, 10);
            vo_s_dp_dc(b2.Order, OrderState.已成交, 2, 10);

            var b3 = ms.AddOrder(t1.Id, c.Id, TradeDirectType.卖, OrderType.平仓, OrderPolicy.限价申报, 10, 2);
            var b4 = ms.AddOrder(t2.Id, c.Id, TradeDirectType.卖, OrderType.平仓, OrderPolicy.限价申报, 10, 2);
      
            vo_s_dp_dc(b3.Order, OrderState.已成交, 2, 10); //为什么错了不是应该与买开成交了么？
            vo_s_dp_dc(b4.Order, OrderState.等待中, 0, 0);

            var ps = t1.GetPositionSummary(c.Code, PositionType.权利仓);
            var bs = t2.GetPositionSummary(c.Code, PositionType.义务仓);
            vb_t_f_m(t1.Account.BailAccount, t1bt - (decimal)(10 * 2) - (decimal)(10 * 2 * 0.001) + (decimal)(5 * 2) - (decimal)(5 * 2 * 0.001), 0, 0);
            vb_t_f_m(t2.Account.BailAccount, t2bt + (decimal)(10 * 2) - (decimal)(10 * 2 * 0.001), 0, 220);
            vp_c_bp_m(ps, 5, 2, 0);
            vp_c_bp_m(bs, 10, 2, 220);
            //vo_s_dp_dc(b1.Order, OrderState.已成交, 2, 10);
            //vo_s_dp_dc(b2.Order, OrderState.已成交, 2, 10);
            //vb_t_f_m(t1.Account.BailAccount, t1bt - (decimal)(10 * 2) - (decimal)(10 * 2 * 0.001), 0, 0);
            //vb_t_f_m(t2.Account.BailAccount, t2bt + (decimal)(10 * 2) - (decimal)(10 * 2 * 0.001), 0, 220);
            //vp_c_bp_m(ps, 10, 2, 0);
            //vp_c_bp_m(bs, 10, 2, 220);
        }

        //
        //确认熔断机制
         //[TestMethod]
         //public void Fuse()
         //{
         //    //  var t1bt = t1.Orders.
         //    var b1 = ms.AddOrder(t1.Id, c.Id, TradeDirectType.买, OrderType.开仓, OrderPolicy.限价申报, 1, 20);
         //    var b2 = ms.AddOrder(t2.Id, c.Id, TradeDirectType.卖, OrderType.开仓, OrderPolicy.限价申报, 1, 20);
         //    vo_s_dp_dc(b1.Order, OrderState.已成交, 20, 1);
         //    vo_s_dp_dc(b2.Order, OrderState.已成交, 20, 1);
         //    MarketItem mt = Market.MarketItems[c.Name];
         //    vp_c_bp_m_fuce(mt, 40, 10,null,null);
         //}

         //触发下跌熔断机制
         // [TestMethod]
         //public void RiseFuce()
         //{

         //    var b1 = ms.AddOrder(t1.Id, c.Id, TradeDirectType.买, OrderType.开仓, OrderPolicy.限价申报, 1, 20);
         //    var b2 = ms.AddOrder(t2.Id, c.Id, TradeDirectType.卖, OrderType.开仓, OrderPolicy.限价申报, 1, 20);
         //    vo_s_dp_dc(b1.Order, OrderState.已成交, 20, 1);
         //    vo_s_dp_dc(b2.Order, OrderState.已成交, 20, 1);
         //    MarketItem mt = Market.MarketItems[c.Name];
            
         //    vp_c_bp_m_fuce(mt, 40, 10, null, null);
         //    var b3 = ms.AddOrder(t1.Id, c.Id, TradeDirectType.买, OrderType.开仓, OrderPolicy.限价申报, 1, 800);
         //    MarketItem mt2 = Market.MarketItems[c.Name];
         //    vp_c_bp_m_fuce(mt2, 40, 10, "上涨机制", "");
         //}
          //触发下跌熔断机制
           //[TestMethod]
          //public void FallFuce()
          //{

          //    var b1 = ms.AddOrder(t1.Id, c.Id, TradeDirectType.买, OrderType.开仓, OrderPolicy.限价申报, 1, 500);
          //    var b2 = ms.AddOrder(t2.Id, c.Id, TradeDirectType.卖, OrderType.开仓, OrderPolicy.限价申报, 1, 500);
          //    vo_s_dp_dc(b1.Order, OrderState.已成交, 500, 1);
          //    vo_s_dp_dc(b2.Order, OrderState.已成交, 500, 1);
          //    MarketItem mt = Market.MarketItems[c.Name];
          //    vp_c_bp_m_fuce(mt, 1000, 250, null, null);
          //    var b3 = ms.AddOrder(t1.Id, c.Id, TradeDirectType.买, OrderType.开仓, OrderPolicy.限价申报, 1,101);
          //    MarketItem mt2 = Market.MarketItems[c.Name];
          //    vp_c_bp_m_fuce(mt2, 1000, 250, "", "下跌机制");
          //}



    }
}

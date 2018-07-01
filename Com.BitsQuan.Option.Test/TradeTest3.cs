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
    public class TradeBase
    {
        protected MatchService ms;
        protected Trader t1 { get { return ms.Model.Traders.Where(a => a.Id == 1).First(); } }
        protected Trader t2 { get { return ms.Model.Traders.Where(a => a.Id == 2).First(); } }
        protected Trader t3 { get { return ms.Model.Traders.Where(a => a.Id == 3).First(); } }
        protected Contract c { get { return ms.Model.Contracts.Where(a => a.Id == 1&&a.IsDel==false).First(); } }
        

        #region helps
        protected void RestoreDb()
        {
            string sql = @"use master  
declare @dbname sysname 
set @dbname='Option-test' --这个是要删除的数据库库名

declare @s nvarchar(1000) 
declare tb cursor local for 
select s='kill '+cast(spid as varchar) 
from master..sysprocesses 
where dbid=db_id(@dbname) 
open tb 
fetch next from tb into @s 
while @@fetch_status=0 
begin 
exec(@s) 
fetch next from tb into @s 
end 
close tb 
deallocate tb 
exec('drop database ['+@dbname+']')";

            var ds = new DBServer(@"Data Source=.\sqlexpress;Initial Catalog=master;Integrated Security=True");
            ds.ExecNonQuery(sql);
            ds.Dispose(); ds = null;



            //Database.SetInitializer<OptionDbCtx>(new DropCreateDatabaseAlways<OptionDbCtx>());

            var context = new OptionDbCtx();
            context.Database.Initialize(true);
            //if (!context.Database.Exists())
            //{
            //    context.Database.Create();
            //}
            //var initor = new TraderInitor();
            //initor.Init(context);

            //var ci = new ContractInitor();
            //ci.Init(context);
        }

        protected decimal orignalSum = 1000000m;
        /// <summary>
        /// 确认委托
        /// </summary>
        /// <param name="o"></param>
        /// <param name="state"></param>
        /// <param name="donePrice"></param>
        /// <param name="doneCount"></param>
        protected void vo_s_dp_dc(Order o, OrderState state, decimal donePrice, int doneCount)
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
        protected void vp_c_bp_m(PositionSummary p1, int count, decimal buyPrice, decimal maintain)
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
        protected void vb_t_f_m(BailAccount ba, decimal total, decimal froze, decimal maintain)
        {
            Assert.AreEqual(ba.Total, total);
            //Assert.AreEqual(ba.MaintainCount, maintain);
            Assert.AreEqual(ba.Frozen, froze);
        }
        #endregion
    }
    /// <summary>
    /// 做一系列操作:每次下单都在前面下单的基础上,从上到下流水执行
    /// </summary>
    [TestClass]
    public class TradeTest3:TradeBase
    {
        [TestInitialize]
        public void Init()
        {
            if (ms != null)
            {
                ms.Dispose();
                ms = null;
            }
            
            RestoreDb();
            ms = new MatchService();
        }
        
        //[TestMethod]
        public void t3_sell_to_6()
        {
            var sl = new List<OrderResult>();

            for (int i = 0; i < 6; i++)
            {
                var t = ms.AddOrder(t1.Id, c.Id, TradeDirectType.卖, OrderType.开仓, OrderPolicy.限价申报, i + 1, i + 1);
                sl.Add(t);
            }
            var b1 = ms.AddOrder(t2.Id, c.Id, TradeDirectType.买, OrderType.开仓, OrderPolicy.限价申报, 21, 6);
            for (int i = 0; i < 6; i++)
            {
                vo_s_dp_dc(sl[i].Order, OrderState.已成交, i + 1, i + 1);
            }
            vo_s_dp_dc(b1.Order, OrderState.已成交, 6, 6);
        }

        //[TestMethod]
        public void t3_1_close_all()
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

        //[TestMethod]
        public void sell_close_error()
        {
            var r = ms.AddOrder(t1.Id, c.Id, TradeDirectType.买, OrderType.平仓, OrderPolicy.限价申报, 1, 1);
            Assert.IsTrue(r.ResultCode != 0);
        }
        //[TestMethod]
        public void sell_close_ok()
        {
            var r1 = ms.AddOrder(t1.Id, c.Id, TradeDirectType.卖, OrderType.开仓, OrderPolicy.限价申报, 1, 1);
            var r2 = ms.AddOrder(t2.Id, c.Id, TradeDirectType.买, OrderType.开仓, OrderPolicy.限价申报, 1, 1);

            var r3 = ms.AddOrder(t1.Id, c.Id, TradeDirectType.买, OrderType.平仓, OrderPolicy.限价申报, 1, 1);
            Assert.IsTrue(r3.ResultCode == 0);
            var r4 = ms.AddOrder(t1.Id, c.Id, TradeDirectType.买, OrderType.平仓, OrderPolicy.限价申报, 1, 1);
            Assert.IsTrue(r4.ResultCode != 0);
        }
        [TestMethod]
        public void match_price()
        {
           
            List<OrderResult> l = new List<OrderResult>();
            for (int i = 0; i < 5; i++)
            {
                l.Add(ms.AddOrder(t1.Id, c.Id, TradeDirectType.卖, OrderType.开仓, OrderPolicy.限价申报, i + 1, i + 1));
            }
            var r = ms.AddOrder(t2.Id, c.Id, TradeDirectType.买, OrderType.开仓, OrderPolicy.限价申报, 15, 4);

            for (int i = 0; i < 5; i++)
            {
                if (i < 4)
                {
                    vo_s_dp_dc(l[i].Order, OrderState.已成交, i + 1, i + 1);
                }
                else
                    vo_s_dp_dc(l[i].Order, OrderState.等待中, 0, 0);
            }
            vo_s_dp_dc(r.Order, OrderState.部分成交, 4, 4);
            Assert.AreEqual(r.Order.TotalDoneCount, 10);
        }
         
        [TestMethod]
        public void match_price_reverse()
        {
           
            List<OrderResult> l = new List<OrderResult>();
            for (int i = 0; i < 5; i++)
            {
                l.Add(ms.AddOrder(t1.Id, c.Id, TradeDirectType.买, OrderType.开仓, OrderPolicy.限价申报, i + 1, i + 1));
            }
            var r = ms.AddOrder(t2.Id, c.Id, TradeDirectType.卖, OrderType.开仓, OrderPolicy.限价申报, 15, 5);

            for (int i = 0; i < 5; i++)
            {
                if (i < 4)
                {
                    vo_s_dp_dc(l[i].Order, OrderState.等待中, 0, 0);
                }
                else
                    vo_s_dp_dc(l[i].Order, OrderState.已成交, 5, 5);
            }
            vo_s_dp_dc(r.Order, OrderState.部分成交, 5, 5);
            Assert.AreEqual(r.Order.TotalDoneCount, 5);
        }

        [TestMethod]
        public void Giant1()
        { 
            t3_sell_to_6();
            t3_1_close_all();
            sell_close_error();
            sell_close_ok();
            
        }
        
    }
}

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Com.BitsQuan.Option.Provider;
using System.Linq;
using System.Data.Entity;
using System.Collections.Generic;
using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Ui.Models.Query;

namespace Com.BitsQuan.Option.Test
{
    [TestClass]
    public class HugeTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            List<string> l = new List<string>();
            OptionDbCtx db = new OptionDbCtx();
            db.Database.Log = (a) => { l.Add(a); };
            var c = db.Set<AccountTradeRecord>().Count();
            var q = db.Set<AccountTradeRecord>().Where(a=>true).OrderBy(a => a.Id).Take(10).AsQueryable().ToList();
            Assert.AreEqual(10, q.Count);
        }


        [TestMethod]
        public void T2()
        {
            List<string> l = new List<string>();
            OptionDbCtx db = new OptionDbCtx();
            db.Database.Log = (a) => { 
                l.Add(a); 
            };
            QueryEngine aqa = new QueryEngine();
            BaseRepository<AccountTradeRecord> cerRepo = new BaseRepository<AccountTradeRecord>();
            cerRepo.SetCtx(db);
            var r = QueryEngine.Query<AccountTradeRecord, int>(ref aqa, cerRepo, "/TradeData/CerQuery", a => a.Id);
            Assert.AreEqual(20, r.Count);
        }
    }
}

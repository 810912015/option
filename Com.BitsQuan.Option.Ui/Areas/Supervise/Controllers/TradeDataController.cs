using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Core.Spot;
using Com.BitsQuan.Option.Imp;
using Com.BitsQuan.Option.Provider;
using Com.BitsQuan.Option.Ui.Models;
using Com.BitsQuan.Option.Ui.Models.Query;
using Com.BitsQuan.Option.Ui.Models.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Com.BitsQuan.Option.Ui.Areas.Supervise.Controllers
{
    [AuthViaMenu(Roles = "网站管理员")]
    [Log]
    public class TradeDataController : Controller
    {
        OptionDbCtx db;
        OrderRepo orderRepo;
        BaseRepository<AccountTradeRecord> atr;
        BaseRepository<Deal> dealRepo;
        BaseRepository<BlastRecord> blastRepo;
        BaseRepository<FuseRecord> fuseRepo;
        BaseRepository<BlasterOperaton> boRepo;
        BaseRepository<TraderMsg> tmRepo;
        BaseRepository<SysAccountRecord> saRepo;
        BaseRepository<UserPosition> upRepo;
        BaseRepository<PositionSummaryData> upRepoData;
        BaseRepository<ContractExecuteRecord> cerRepo;
        BaseRepository<SpotOrder> soRepo;
        BaseRepository<SpotDeal> sdRepo;

        ApplicationDbContext adb;
       BaseRepository<UserOpLog> uolRepo;
        public TradeDataController()
        {
            db = new OptionDbCtx();
            orderRepo = new OrderRepo(db);
            atr = new BaseRepository<AccountTradeRecord>(); atr.SetCtx(db);
            dealRepo = new BaseRepository<Deal>(); dealRepo.SetCtx(db);
            blastRepo = new BaseRepository<BlastRecord>(); blastRepo.SetCtx(db);
            fuseRepo = new BaseRepository<FuseRecord>(); fuseRepo.SetCtx(db);
            boRepo = new BaseRepository<BlasterOperaton>(); boRepo.SetCtx(db);
            tmRepo = new BaseRepository<TraderMsg>(); tmRepo.SetCtx(db);
            saRepo = new BaseRepository<SysAccountRecord>(); saRepo.SetCtx(db);
            upRepo = new BaseRepository<UserPosition>(); upRepo.SetCtx(db);
            upRepoData = new BaseRepository<PositionSummaryData>(); upRepoData.SetCtx(db);
            cerRepo = new BaseRepository<ContractExecuteRecord>(); cerRepo.SetCtx(db);
            soRepo = new BaseRepository<SpotOrder>(); soRepo.SetCtx(db);
            sdRepo = new BaseRepository<SpotDeal>(); sdRepo.SetCtx(db);

            adb = new ApplicationDbContext();
            uolRepo = new BaseRepository<UserOpLog>();
            uolRepo.SetCtx(adb);
        }
        protected override void Dispose(bool disposing)
        {
            if (!disposing&&db!=null)
            {
                db.Dispose();
                db = null;
            }
            if (!disposing && adb != null)
            {
                adb.Dispose();
                adb = null;
            }
            base.Dispose(disposing);
        }
        public ActionResult Index()
        {
            return View();
        }
        public PartialViewResult Orders()
        {
            QueryEngine aqa = null;
            var r = QueryEngine.Query<Order, int>(ref aqa, orderRepo, "/TradeData/OrderQuery", a => a.Id);
            ViewData["args"] = aqa;
            ViewData["action"] = "OrderQuery";
            ViewData["controller"] = "TradeData";
            return PartialView();
        }

        public PartialViewResult OrderQuery(QueryEngine aqa = null)
        {
            var r = QueryEngine.Query<Order, int>(ref aqa, orderRepo, "/TradeData/OrderQuery", a => a.Id);
            ViewData["args"] = aqa;
            return PartialView(r);
        }

        public PartialViewResult AccountTradeRecords()
        {
            QueryEngine aqa = null;
            var r = QueryEngine.Query<AccountTradeRecord, int>(ref aqa, atr, "/TradeData/AccountTradeRecordQuery", a => a.Id);
            ViewData["args"] = aqa;
            ViewData["action"] = "AccountTradeRecordQuery";
            ViewData["controller"] = "TradeData";
            return PartialView();
        }

        public PartialViewResult AccountTradeRecordQuery(QueryEngine aqa = null)
        {
            var r = QueryEngine.Query<AccountTradeRecord, int>(ref aqa, atr, "/TradeData/AccountTradeRecordQuery", a => a.Id);
            ViewData["args"] = aqa;
            return PartialView(r);
        }

        public PartialViewResult Deals()
        {
            QueryEngine aqa = null;
            var r = QueryEngine.Query<Deal, int>(ref aqa, dealRepo, "/TradeData/DealQuery", a => a.Id);
            ViewData["args"] = aqa;
            ViewData["action"] = "DealQuery";
            ViewData["controller"] = "TradeData";
            return PartialView();
        }

        public PartialViewResult DealQuery(QueryEngine aqa = null)
        {
            var r = QueryEngine.Query<Deal, int>(ref aqa, dealRepo, "/TradeData/DealQuery", a => a.Id);
            ViewData["args"] = aqa;
            return PartialView(r);
        }



        public PartialViewResult BlastRecords()
        {
            QueryEngine aqa = null;
            var r = QueryEngine.Query<BlastRecord, int>(ref aqa, blastRepo, "/TradeData/BlastRecordQuery", a => a.Id);
            ViewData["args"] = aqa;
            ViewData["action"] = "BlastRecordQuery";
            ViewData["controller"] = "TradeData";
            return PartialView();
        }

        public PartialViewResult BlastRecordQuery(QueryEngine aqa = null)
        {
            var r = QueryEngine.Query<BlastRecord, int>(ref aqa, blastRepo, "/TradeData/BlastRecordQuery", a => a.Id);
            ViewData["args"] = aqa;
            return PartialView(r);
        }
        public PartialViewResult BlastOps()
        {
            QueryEngine aqa = null;
            var r = QueryEngine.Query<BlasterOperaton, int>(ref aqa, boRepo, "/TradeData/BlastOpQuery", a => a.Id);
            ViewData["args"] = aqa;
            ViewData["action"] = "BlastOpQuery";
            ViewData["controller"] = "TradeData";
            return PartialView();
        }

        public PartialViewResult BlastOpQuery(QueryEngine aqa = null)
        {
            var r = QueryEngine.Query<BlasterOperaton, int>(ref aqa, boRepo, "/TradeData/BlastOpQuery", a => a.Id);
            //foreach (var item in r.ToList())
            //{
            //    //根据持仓编号查询对应持仓
            
            //}
            ViewData["args"] = aqa;
            //ViewData["hrji"] = "号号号号号号号号号号哈吉斯顶哈健康";
            return PartialView(r);
        }

        public PartialViewResult FuseRecords()
        {
            QueryEngine aqa = null;
            var r = QueryEngine.Query<FuseRecord, int>(ref aqa, fuseRepo, "/TradeData/FuseRecordQuery", a => a.Id);
            ViewData["args"] = aqa;
            ViewData["action"] = "FuseRecordQuery";
            ViewData["controller"] = "TradeData";
            return PartialView();
        }

        public PartialViewResult FuseRecordQuery(QueryEngine aqa = null)
        {
            var r = QueryEngine.Query<FuseRecord, int>(ref aqa, fuseRepo, "/TradeData/FuseRecordQuery", a => a.Id);
            ViewData["args"] = aqa;
            return PartialView(r);
        }
        public PartialViewResult TraderMsgRecords()
        {
            QueryEngine aqa = null;
            var r = QueryEngine.Query<TraderMsg, int>(ref aqa, tmRepo, "/TradeData/TraderMsgQuery", a => a.Id);
            ViewData["args"] = aqa;
            ViewData["action"] = "TraderMsgQuery";
            ViewData["controller"] = "TradeData";
            return PartialView();
        }

        public PartialViewResult TraderMsgQuery(QueryEngine aqa = null)
        {
            var r = QueryEngine.Query<TraderMsg, int>(ref aqa, tmRepo, "/TradeData/TraderMsgQuery", a => a.Id);
            ViewData["args"] = aqa;
            return PartialView(r);
        }

        public PartialViewResult SysAccountRecords()
        {
            QueryEngine aqa = null;
            var r = QueryEngine.Query<SysAccountRecord, int>(ref aqa, saRepo, "/TradeData/SysAccountQuery", a => a.Id);
            ViewData["args"] = aqa;
            ViewData["action"] = "SysAccountQuery";
            ViewData["controller"] = "TradeData";
            return PartialView();
        }

        public PartialViewResult SysAccountQuery(QueryEngine aqa = null)
        {
            var r = QueryEngine.Query<SysAccountRecord, int>(ref aqa, saRepo, "/TradeData/SysAccountQuery", a => a.Id);
            ViewData["args"] = aqa;
            return PartialView(r);
        }

        public PartialViewResult UserPositionRecords()
        {
            QueryEngine aqa = null;
            var r = QueryEngine.Query<UserPosition, int>(ref aqa, upRepo, "/TradeData/UserPositionQuery", a => a.Id);
            ViewData["args"] = aqa;
            ViewData["action"] = "UserPositionQuery";
            ViewData["controller"] = "TradeData";
            return PartialView();
        }
        public PartialViewResult UserDynamicPositionRecords()
        {
            QueryEngine aqa = null;
            var r = QueryEngine.Query<PositionSummaryData, int>(ref aqa, upRepoData, "/TradeData/UserDynamicPositionQuery", a => a.Id);
            ViewData["args"] = aqa;
            ViewData["action"] = "UserDynamicPositionQuery";
            ViewData["controller"] = "TradeData";
            return PartialView();
        }
        public PartialViewResult UserDynamicPositionQuery(QueryEngine aqa = null)
        {
            var r = QueryEngine.Query<PositionSummaryData, int>(ref aqa, upRepoData, "/TradeData/UserDynamicPositionQuery", a => a.Id);
            ViewData["args"] = aqa;
            return PartialView(r);
        }
        public PartialViewResult UserPositionQuery(QueryEngine aqa = null)
        {
            var r = QueryEngine.Query<UserPosition, int>(ref aqa, upRepo, "/TradeData/UserPositionQuery", a => a.Id);
            ViewData["args"] = aqa;
            return PartialView(r);
        }


        public PartialViewResult CerRecords()
        {
            QueryEngine aqa = null;
            var r = QueryEngine.Query<ContractExecuteRecord, int>(ref aqa, cerRepo, "/TradeData/CerQuery", a => a.Id);
            ViewData["args"] = aqa;
            ViewData["action"] = "CerQuery";
            ViewData["controller"] = "TradeData";
            return PartialView();
        }

        public PartialViewResult CerQuery(QueryEngine aqa = null)
        {
            var r = QueryEngine.Query<ContractExecuteRecord, int>(ref aqa, cerRepo, "/TradeData/CerQuery", a => a.Id);
            ViewData["args"] = aqa;
            return PartialView(r);
        }



        public PartialViewResult SORecords()
        {
            QueryEngine aqa = null;
            var r = QueryEngine.Query<SpotOrder, int>(ref aqa, soRepo, "/TradeData/SOQuery", a => a.Id);
            ViewData["args"] = aqa;
            ViewData["action"] = "SOQuery";
            ViewData["controller"] = "TradeData";
            return PartialView();
        }

        public PartialViewResult SOQuery(QueryEngine aqa = null)
        {
            var r = QueryEngine.Query<SpotOrder, int>(ref aqa, soRepo, "/TradeData/SOQuery", a => a.Id);
            ViewData["args"] = aqa;
            return PartialView(r);
        }
        public PartialViewResult SdRecords()
        {
            QueryEngine aqa = null;
            var r = QueryEngine.Query<SpotDeal, int>(ref aqa, sdRepo, "/TradeData/SdQuery", a => a.Id);
            ViewData["args"] = aqa;
            ViewData["action"] = "SdQuery";
            ViewData["controller"] = "TradeData";
            return PartialView();
        }

        public PartialViewResult SdQuery(QueryEngine aqa = null)
        {
            var r = QueryEngine.Query<SpotDeal, int>(ref aqa, sdRepo, "/TradeData/SdQuery", a => a.Id);
            ViewData["args"] = aqa;
            return PartialView(r);
        }

        public PartialViewResult LogRecords()
        {
            QueryEngine aqa = null;
            var r = QueryEngine.Query<UserOpLog, int>(ref aqa, uolRepo, "/TradeData/LogQuery", a => a.Id);
            ViewData["args"] = aqa;
            ViewData["action"] = "LogQuery";
            ViewData["controller"] = "TradeData";
            return PartialView();
        }

        public PartialViewResult LogQuery(QueryEngine aqa = null)
        {
            var r = QueryEngine.Query<UserOpLog, int>(ref aqa, uolRepo, "/TradeData/LogQuery", a => a.Id);
            ViewData["args"] = aqa;
            return PartialView(r);
        }
	}
}
using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Imp;
using Com.BitsQuan.Option.Match;
using Com.BitsQuan.Option.Match.Imp;
using Com.BitsQuan.Option.Provider;
using Com.BitsQuan.Option.Ui.Models;
using Com.BitsQuan.Option.Ui.Models.Security;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Com.BitsQuan.Option.Ui.Areas.Supervise.Controllers
{

    public class BlastModel
    {
        [Range(1, 10)]
        [Display(Name = "保证率正常阀值")]
        public decimal NormalMaintainRatio { get; set; }
        [Range(1, 10)]
        [Display(Name = "保证率警告阀值")]
        public decimal AlarmMaintainRatio { get; set; }
        [Range(1, 10)]
        [Display(Name = "保证率爆仓阀值")]
        public decimal BlastMaintainRatio { get; set; }
        [Range(0.01, 1)]
        [Display(Name = "平仓金额阶梯阀值")]
        public decimal BlastOnceRatio { get; set; }
        [Range(1, 1000)]
        [Display(Name = "平仓份数最大值")]
        public int MaxCountPerSell { get; set; }
        public bool IsValid
        {
            get
            {
                return MaxCountPerSell > 1
                    && BlastOnceRatio > 0
                    && BlastMaintainRatio >= 1
                    && NormalMaintainRatio > AlarmMaintainRatio
                    && AlarmMaintainRatio > BlastMaintainRatio;
            }
        }

        public BlastModel()
        {
            this.NormalMaintainRatio = SysPrm.Instance.MonitorParams.NormalMaintainRatio;
            this.AlarmMaintainRatio = SysPrm.Instance.MonitorParams.AlarmMaintainRatio;
            this.BlastMaintainRatio = SysPrm.Instance.MonitorParams.BlastMaintainRatio;
            this.BlastOnceRatio = SysPrm.Instance.MonitorParams.BlastOnceRatio;
            this.MaxCountPerSell = SysPrm.Instance.MonitorParams.MaxCountPerSell;
        }
    }

    public class LimitModel
    {
        [Display(Name = "未成交委托存活最大小时数")]
        [Range(2, 24)]
        public double SpanInHours { get; set; }
        [Display(Name = "每分钟下单最大份数")]
        [Range(1, 500)]
        public int CountPerMinuteLimit { get; set; }
        [Display(Name = "每合约最大下单数")]
        [Range(1, 500)]
        public int CountPerContractLimit { get; set; }

        public LimitModel()
        {
            this.CountPerContractLimit = Com.BitsQuan.Option.Imp.OrderPreHandler.CountPerContractLimit;
            this.CountPerMinuteLimit = Com.BitsQuan.Option.Imp.OrderPreHandler.CountPerMinuteLimit;
            this.SpanInHours = OrderLifeManager<IOrder>.SpanInHours;
        }
    }

    public class FuseModel
    {
        [Display(Name = "比特币熔断阀值(低于不计算熔断)")]
        [Range(0.001, 0.5)]
        public decimal MinFuseRatioOfBtcPrice { get; set; }
        [Display(Name = "熔断时长(分钟)")]
        [Range(0.5, 60)]
        public double FuseSpanInMin { get; set; }
        [Display(Name = "熔断期间无成交则结束后上涨比例")]
        [Range(1, 2)]
        public decimal RaiseRatio { get; set; }
        [Display(Name = "熔断期间无成交则结束后下降比例")]
        [Range(1, 2)]
        public decimal FallRatio { get; set; }

        public FuseModel()
        {
            this.FuseSpanInMin = ContractFuse.FuseSpanInMin;
            this.MinFuseRatioOfBtcPrice = ContractFuse.MinFuseRatioOfBtcPrice;
            this.RaiseRatio = FuseExcutor.RaiseRatio;
            this.FallRatio = FuseExcutor.FallRatio;
        }
    }

    public class FeeModel
    {
        [Display(Name = "手续费比例")]
        [Range(0.001, 1)]
        public decimal CommissionRatio { get; set; }
        public FeeModel()
        {
            this.CommissionRatio = TraderService.CommissionRatio;
        }
    }

    public class ExecuteModel
    {
        [Display(Name = "当前比特币合约行权基准价(-1表示当前市价)")]
        [Range(0.0001, 1000000)]
        public decimal BtcExeBasePrice { get; set; }
        public ExecuteModel()
        {
            this.BtcExeBasePrice = OrderExecutor.BtcExeBasePrice ?? -1;
        }
    }
    [AuthViaMenu(Roles = "网站管理员")]
    [Log]
    public class ParameterController : Controller
    {
        //
        // GET: /Supervise/Parameter/
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public PartialViewResult Blast()
        {
            var param =  SysPrm.Instance.MonitorParams;
            return PartialView(new BlastModel
            {
                AlarmMaintainRatio = param.AlarmMaintainRatio,
                NormalMaintainRatio = param.NormalMaintainRatio,
                BlastMaintainRatio = param.BlastMaintainRatio,
                BlastOnceRatio = param.BlastOnceRatio,
                MaxCountPerSell = param.MaxCountPerSell
            });

        }

        public JsonResult StopSystem()
        {
            MvcApplication.OptionService.Flush();
            MvcApplication.SpotService.Flush();
            return Json(0, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public PartialViewResult Blast(BlastModel omp)
        {
            if (omp != null && omp.IsValid)
            {
                SysPrm.Instance.MonitorParams.AlarmMaintainRatio = omp.AlarmMaintainRatio;
                SysPrm.Instance.MonitorParams.NormalMaintainRatio = omp.NormalMaintainRatio;
                SysPrm.Instance.MonitorParams.BlastMaintainRatio = omp.BlastMaintainRatio;
                SysPrm.Instance.MonitorParams.BlastOnceRatio = omp.BlastOnceRatio;
                SysPrm.Instance.MonitorParams.MaxCountPerSell = omp.MaxCountPerSell;
                ViewBag.msg = "爆仓参数设置成功!";
            }

            return PartialView(omp);

        }
        [HttpGet]
        public PartialViewResult Limit()
        {
            return PartialView(new LimitModel());
        }
        [HttpPost]
        public PartialViewResult Limit(LimitModel lm)
        {
            if (ModelState.IsValid)
            {
                Com.BitsQuan.Option.Imp.OrderPreHandler.CountPerMinuteLimit = lm.CountPerMinuteLimit;
                Com.BitsQuan.Option.Imp.OrderPreHandler.CountPerContractLimit = lm.CountPerContractLimit;
                OrderLifeManager<Order>.SpanInHours = lm.SpanInHours;
                OrderLifeManager<Com.BitsQuan.Option.Core.Spot.SpotOrder>.SpanInHours = lm.SpanInHours;
                ViewBag.msg = "大户限仓参数设置成功";
            }
            return PartialView(lm);
        }

        [HttpGet]
        public PartialViewResult Fuse()
        {
            return PartialView(new FuseModel());
        }
        [HttpPost]
        public PartialViewResult Fuse(FuseModel fm)
        {
            if (ModelState.IsValid)
            {
                ContractFuse.FuseSpanInMin = fm.FuseSpanInMin;
                ContractFuse.MinFuseRatioOfBtcPrice = fm.MinFuseRatioOfBtcPrice;
                FuseExcutor.RaiseRatio = fm.RaiseRatio;
                FuseExcutor.FallRatio = fm.FallRatio;
                ViewBag.msg = "熔断参数设置成功";
            }
            return PartialView(fm);
        }

        [HttpGet]
        public PartialViewResult Fee()
        {
            return PartialView();
        }
        [HttpPost]
        public PartialViewResult Fee(FeeModel fm)
        {
            if (ModelState.IsValid)
            {
                TraderService.CommissionRatio = fm.CommissionRatio;
                ViewBag.msg = "收费参数设置成功";
            }
            return PartialView(fm);
        }

        [HttpGet]
        public PartialViewResult Execute()
        {
            ViewData["contracts"] = ToExe();
            return PartialView();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public PartialViewResult Execute(ManualExcuteModel model)
        {
            //撤销正在挂出的单子
            try
            {
                TradeManager tradeMgr = new TradeManager();
                if (ModelState.IsValid)
                {
                    var name = MvcApplication.OptionService.Model.Contracts.Where(a => a.IsDel == false && a.Id == model.ContractId).FirstOrDefault();
                    MvcApplication.OptionService.Executor.ManualExcute(model.ContractId, model.ExecuteBasePrice, MvcApplication.OptionService.MarketBoard, name.Name);
                    Singleton<TextLog>.Instance.Info(string.Format("手动行权:合约编号{0},行权基准价{1},执行人{2}", model.ContractId, model.ExecuteBasePrice, User.Identity.Name));
                    ViewBag.msg = "手动行权结束";

                }
                ViewData["contracts"] = ToExe();

            }
            catch (Exception ex)
            {
                Singleton<TextLog>.Instance.Error(ex);
            }
            return PartialView(model);
        }

        public JsonResult ExecuteAllTest()
        {
            try
            {
                MvcApplication.OptionService.Executor.ExecuteAll();
                return Json(0, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Singleton<TextLog>.Instance.Error(e, string.Format("全部行权测试-{0}", User.Identity.Name));
                return Json(1, JsonRequestBehavior.AllowGet);
            }
        }


        List<SelectListItem> ToExe()
        {
            using (OptionDbCtx db = new OptionDbCtx())
            {
                var q = db.Contracts.Where(a => a.IsNotInUse == false && a.IsDel == false)// && a.ExcuteTime <= DateTime.Now)
                    .ToList();
                List<SelectListItem> l = new List<SelectListItem>();
                if (q == null || q.Count() == 0) return l;
                foreach (var v in q)
                {
                    l.Add(new SelectListItem { Text = v.Code + " - " + v.Name, Value = v.Id.ToString() });
                }
                return l;
            }
        }

        public PartialViewResult SetBtcPrice()
        {
            return PartialView();
        }

        public JsonResult SetBtcPriceUpdateState()
        {
            try
            {
                BtcPrice.IsAutoUpdateBtcPrice = !BtcPrice.IsAutoUpdateBtcPrice;
                var r=BtcPrice.IsAutoUpdateBtcPrice?1:2;
                return Json(new OperationResult {ResultCode=r ,Desc="成功"}, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Singleton<TextLog>.Instance.Error(e);
                return Json(new OperationResult { ResultCode=-1, Desc=e.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult SetBtcCurPrice(decimal price)
        {
            try
            {
                if (BtcPrice.IsAutoUpdateBtcPrice)
                {
                    return Json(new OperationResult {Desc="当前不允许手动设置",ResultCode=-2 }, JsonRequestBehavior.AllowGet);
                }
                BtcPrice.SetPrice(price);
                return Json(new OperationResult { Desc = "成功", ResultCode = (int)BtcPrice.Current }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Singleton<TextLog>.Instance.Error(e);
                return Json(new OperationResult { ResultCode = -1, Desc = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult OperateInvitorFee()
        {
            try
            {
                MvcApplication.ifm.Init();
                MvcApplication.ifm.TransferFee();
                return Json("手续费返还成功", JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(e.Message, JsonRequestBehavior.AllowGet);
            }
        }
        public PartialViewResult InvitorFee()
        {
            return PartialView();
        }
        public JsonResult SetBonus(decimal? bonus)
        {
            if (bonus == null) return Json("必须大于0");
            var b = (decimal)bonus;
            if (b == InvitorFeeService.InvitorBonusInCny)
                return Json("不能与当前值相同");
            InvitorFeeService.InvitorBonusInCny = b;
            return Json("设置成功");
        }
    }

    public class ManualExcuteModel
    {
        [Display(Name = "选择合约")]
        [Required]
        public int ContractId { get; set; }
        [Display(Name = "行权基准价")]
        [Range(0.01, 1000000)]
        public decimal ExecuteBasePrice { get; set; }
    }
}
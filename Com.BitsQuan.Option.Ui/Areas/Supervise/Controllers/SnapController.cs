using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Ui.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Com.BitsQuan.Option.Imp;
using Com.BitsQuan.Option.Ui.Models.Security;
using Com.BitsQuan.Option.Match.Imp;
using Com.BitsQuan.Option.Match;
using Com.BitsQuan.Option.Match.Spot; 

namespace Com.BitsQuan.Option.Ui.Areas.Supervise.Controllers
{
    public enum AccountChangeTypeByManual
    {
        人民币账户 = 1, 比特币账户 = 2, 保证金账户 = 3
    }
    public class ManualModel
    {
        public int TraderId { get; set; }
        [Display(Name = "账户类型")]
        [Required]
        public AccountChangeTypeByManual ManualType { get; set; }
        [Range(0.001, 1000000)]
        [Display(Name = "金额")]
        public decimal Delta { get; set; }
        [Display(Name = "添加到(否则为从中扣除)")]
        public bool IsAddTo { get; set; }
        [Display(Name="操作冻结")]
        public bool IsAboutFreeze { get; set; }
        [Display(Name="冻结(否则为解冻)")]
        public bool IsFreeze { get; set; }

        public AccountChangeType? ChangeType
        {
            get
            {
                AccountChangeType? r = null;
                switch (ManualType)
                {
                    case AccountChangeTypeByManual.保证金账户:
                        if (IsAddTo)
                        {
                            r = AccountChangeType.手动增加保证金;
                        }
                        else
                        {
                            r = AccountChangeType.手动减少保证金;
                        }
                        break;
                    case AccountChangeTypeByManual.比特币账户:
                        if (IsAddTo)
                        {
                            r = AccountChangeType.手动增加BTC;
                        }
                        else
                        {
                            r = AccountChangeType.手动减少BTC;
                        }
                        break;
                    case AccountChangeTypeByManual.人民币账户:
                        if (IsAddTo)
                        {
                            r = AccountChangeType.手动增加现金;
                        }
                        else
                        {
                            r = AccountChangeType.手动减少现金;
                        }
                        break;
                }
                return r;
            }
        }
    }
    [AuthViaMenu(Roles = "网站管理员")]
    [Log]
    public class SnapController : Controller
    {
        ApplicationDbContext db;
        public SnapController()
        {
            db = new ApplicationDbContext();
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing && db != null)
            {
                db.Dispose();
                db = null;
            }
            base.Dispose(disposing);
        }
        public ActionResult Index()
        {
            return View();
        }
        //
        // GET: /Supervise/Snap/
        public PartialViewResult Real()
        { 
            var ts = MvcApplication.OptionService.Model.Traders.ToList();
           
            return PartialView(ts);
        }


        public PartialViewResult Orders() {
            ViewBag.Orders = MvcApplication.OptionService.Matcher.Container.Orders;
            return PartialView();
        }
        public PartialViewResult OrdersQuery(string contractCode=null)
        {
            try
            {
                if (string.IsNullOrEmpty(contractCode))
                {
                    contractCode = MvcApplication.OptionService.Model.Contracts.First().Code;
                }
                ViewBag.name = contractCode;
                if (MvcApplication.OptionService.Matcher.Container.Orders.ContainsKey(contractCode))
                {
                    var q = MvcApplication.OptionService.Matcher.Container.Orders[contractCode];
                    return PartialView(q);
                }
                else return PartialView();
               
            }
            catch (Exception ex)
            {
                Singleton<TextLog>.Instance.Error(ex, "ordersquery");
                return PartialView();
            }
        }
       
        public ActionResult Complex(int id)
        {
            var t = MvcApplication.OptionService.Model.Traders.Where(a => a.Id == id).FirstOrDefault();
            var au = db.Set<ApplicationUser>().Where(a => a.UserName == t.Name).FirstOrDefault();
            ViewData["user"] = au;
            return View(t);
        }
        [HttpGet]
        public PartialViewResult CorrectFrozen()
        {
            return PartialView();
        }
        [HttpPost]
        public PartialViewResult CorrectFrozenPost()
        {
            try
            {
                foreach (var v in MvcApplication.OptionService.Model.Traders.ToList())
                {
                    foreach (var o in v.Orders().Items)
                    {
                        o.Unfreeze();
                    }

                }
                foreach (var v in MvcApplication.SpotService.Model.SpotOrders.ToList())
                {
                    v.UnFreeze();
                }


                foreach (var v in MvcApplication.OptionService.Model.Traders)
                {
                    v.Manual(v.Account.BailAccount.Frozen, AccountChangeType.手动增加保证金, User.Identity.Name, true, false);
                    v.Manual(v.Account.CacheAccount.CnyAccount.Frozen, AccountChangeType.手动增加现金, User.Identity.Name, true, false);
                    v.Manual(v.Account.CacheAccount.BtcAccount.Frozen, AccountChangeType.手动增加BTC, User.Identity.Name, true, false);
                }
                foreach (var v in MvcApplication.OptionService.Model.Traders.ToList())
                {
                    foreach (var o in v.Orders().Items)
                    {
                        o.GetSellOpenCountToFreeze();
                    }

                }
                foreach (var v in MvcApplication.SpotService.Model.SpotOrders.ToList())
                {
                    v.Freeze();
                }
                ViewBag.msg = "重新计算冻结成功！";
                return PartialView("CorrectFrozen");
            }
            catch (Exception ex)
            {
                Singleton<TextLog>.Instance.Error(ex, "correct frozen");
                ViewBag.msg = "重新计算冻结失败：" + ex.Message;
                return PartialView("CorrectFrozen");
            }
        }

        [HttpGet]
        public PartialViewResult Manual(int tid=-100)
        {
            var md = new ManualModel { TraderId = tid,IsAddTo=true };
            return PartialView(md);
        }

        [HttpPost]
        public PartialViewResult Manual(ManualModel mm)
        {
            if (ModelState.IsValid)
            {
                var r = mm.ChangeType;
                if (r == null)
                {
                    ModelState.AddModelError("ManualType", "必须选择账户类型");
                }
                else
                {
                    if (mm.TraderId == -100)
                    {
                        var ts = MvcApplication.OptionService.Model.Traders.ToList();
                        foreach (var v in ts)
                        {
                            v.Manual(mm.Delta, (AccountChangeType)r, User.Identity.Name);
                            ViewBag.msg = "为所有用户手动调整资金成功";
                        }
                    }
                    else
                    {

                        var t = MvcApplication.OptionService.Model.Traders.Where(a => a.Id == mm.TraderId).FirstOrDefault();
                        if (t == null)
                        {
                            ModelState.AddModelError("", "未找到相关用户");
                        }
                        else
                        {
                            if (mm.IsAboutFreeze)
                            {
                                var cr = t.Manual(mm.Delta, (AccountChangeType)r, User.Identity.Name,mm.IsAboutFreeze,mm.IsFreeze);
                                ViewBag.msg = "手动调整资金成功,您需要刷新当前页面查看用户资金";
                            }
                            else
                            {
                                var cr = t.Manual(mm.Delta, (AccountChangeType)r, User.Identity.Name);
                                ViewBag.msg = "手动调整资金成功,您需要刷新当前页面查看用户资金";
                            }
                        }
                    }
                }
            }
            return PartialView(mm);
        }

        public PartialViewResult Cache()
        {
            var ts = MvcApplication.OptionService.Model.Traders.ToList();

            return PartialView(ts);
        }
        public PartialViewResult Coin()
        {
            var ts = MvcApplication.OptionService.Model.Traders.ToList();

            return PartialView(ts);
        }
        public PartialViewResult Bail()
        {
            var ts = MvcApplication.OptionService.Model.Traders.ToList();

            return PartialView(ts);
        }

        public PartialViewResult Pos(string contractCode=null,PositionType type=PositionType.义务仓)
        {
            return PartialView(new PosModel(contractCode,type));
        }

        public PartialViewResult OptionTop()
        {
            var ts = MvcApplication.OptionService.Model.Traders
                .Select(a => new OptionTopModel { Trader=a,
                    PosCount=a.GetPositionSummaries().Count,
                    Sum = a.Account.BailAccount.Total + a.GetPositionSummaries().Select(b => b.TotalValue).Sum() })
                    .Where(a=>a.PosCount >0)
                .OrderByDescending(a=>a.Sum)
                .Take(30)
                .ToList();

            return PartialView(ts);
        }

    }
    public class OptionTopModel
    {
        public Trader Trader { get; set; }
        public int PosCount { get; set; }
        public decimal Sum { get; set; }
    }
    public class PosModel
    {
        public List<SelectListItem> Contracts { get; set; }
         

       
        public string ContractCode { get; set; }
        public PositionType PType { get; set; }

        public PosModel(string contractCode = null, PositionType type = PositionType.义务仓)
        {
            if (contractCode != null)
                this.ContractCode = contractCode;
            else
            {
                if(MvcApplication.OptionService .Model .Contracts.Count ()>0)
                    this.ContractCode = MvcApplication.OptionService.Model.Contracts.First().Code;

            } 
            this.PType = type;
            Contracts = MvcApplication.OptionService.Model.Contracts.Select(a => 
                new SelectListItem { Value = a.Code, Text = a.Code + " " + a.Name,Selected=a.Code==contractCode }).ToList();
        }
    }
    
}
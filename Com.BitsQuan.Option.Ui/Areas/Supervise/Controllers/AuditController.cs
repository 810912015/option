using Com.BitsQuan.Miscellaneous;
using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Imp;
using Com.BitsQuan.Option.Provider;
using Com.BitsQuan.Option.Ui.Areas.Supervise.Data;
using Com.BitsQuan.Option.Ui.Models;
using Com.BitsQuan.Option.Ui.Models.Query;
using Com.BitsQuan.Option.Ui.Models.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Com.BitsQuan.Option.Ui.Areas.Supervise.Controllers
{
    [AuthViaMenu(Roles = "网站管理员")]
    [Log]
    public class AuditController : Controller
    {
        ApplicationDbContext db;
        BaseRepository<BankRecord> tmRepo;
        public AuditController()
        {
            db = new ApplicationDbContext();
            tmRepo = new BaseRepository<BankRecord>();
            tmRepo.SetCtx(db);
        }
        protected override void Dispose(bool disposing)
        {
            if (!disposing && db != null)
            {
                db.Dispose();
                db = null;
            }
            base.Dispose(disposing);
        }
        // GET: Supervise/Audit
        public ActionResult Index()
        {
            return View();
        }
        public PartialViewResult BankRecords()
        {

            QueryEngine aqa = null;
            var r = QueryEngine.Query<BankRecord, int>(ref aqa, tmRepo, "/TradeData/BankRecordQuery", a => a.Id);
            ViewData["args"] = aqa;
            ViewData["action"] = "BankRecordQuery";
            ViewData["controller"] = "TradeData";
            return PartialView();
        }

        public PartialViewResult BankRecordQuery(QueryEngine aqa = null)
        {
            var r = QueryEngine.Query<BankRecord, int>(ref aqa, tmRepo, "/TradeData/BankRecordQuery", a => a.Id);
            ViewData["args"] = aqa;
            return PartialView(r);
        }

        public PartialViewResult AuditIt()
        {
            ViewData["sysBank"] = GetSysBank();
            return PartialView();
        }

        List<SelectListItem> GetSysBank()
        {
            var q = db.BankAccounts.Where(a => a.IsSystem == true);
            List<SelectListItem> l = new List<SelectListItem>();
            if (q != null)
            {
                foreach (var v in q)
                {
                    l.Add(new SelectListItem { Text = v.Number + "-" + (v.BankName == null ? "" : v.BankName), Value = v.Number });
                }
            }
            return l;
        }
        SysBankRecord AddSysBankRecord(AuditModel am)
        {
            try
            {
                var aa = db.BankAccounts.Where(a => a.Number == am.SysBank).FirstOrDefault();
                if (aa == null) return null;
                var sa = new SysBankRecord { Account = aa, By = User.Identity.Name, When = DateTime.Now, Delta = am.ActualDelta, ApproveId = am.BrId };
                db.SysBankRecords.Add(sa);
                db.SaveChanges();
                return sa;
            }
            catch (Exception e)
            {
                Singleton<TextLog>.Instance.Error(e, "AddSysBankRecord");
                return null;
            }
        }

        Recharge rech = new Recharge();
        [HttpPost]
        public PartialViewResult AuditIt(AuditModel am)
        {
            ViewData["sysBank"] = GetSysBank();
            if (ModelState.IsValid && IsTranPwd(am.code))
            {
                var r = db.BankRecords.Where(a => a.Id == am.BrId).FirstOrDefault();

                if (r == null)
                {
                    ViewBag.msg = "无此编号的记录";
                    return PartialView(am);
                }
                if (r.IsApproved == true)
                {
                    ViewBag.msg = "已审核";
                    return PartialView(am);
                }
                //
                if (r.BankRecordType == BankRecordType.提现 && r.coin == "BTC")
                {

                }
                else if (r.BankRecordType == BankRecordType.充值 && r.coin == "BTC")
                {
                    if (!rech.BtcRecharge(r.AppUserName, r.Num, r.Address))
                    {
                        ViewBag.msg = "不符合审核条件";
                        return PartialView(am);
                    }

                }
                var sa= AddSysBankRecord(am);
                r.IsApproved = true;
                r.AuditTime = DateTime.Now;
                r.ActualDelta = am.ActualDelta;
                r.ApproveDesc = am.Desc;
                r.ApprovedResult = am.IsApproved;
                r.SysRecord = sa;
                var t = db.SaveChanges() > 0;


                if (!t)
                {
                    ViewBag.msg = "审核失败";
                    return PartialView(am);
                }
                else
                {

                    var u = MvcApplication.OptionService.Model.Traders.Where(a => a.Name == r.AppUserName).FirstOrDefault();
                    if (u != null)
                    {
                        if (r.ApprovedResult.Value)//审核通过
                        {
                            TraderService.OperateAccount(
                                u,
                                r.ActualDelta,
                                r.BankRecordType == BankRecordType.提现 && r.coin == "CNY" ?
                                    AccountChangeType.CNY提现 :
                                        (r.BankRecordType == BankRecordType.充值 && r.coin == "CNY" ?
                                            AccountChangeType.CNY充值 :
                                                (r.BankRecordType == BankRecordType.提现 && r.coin == "BTC" ?
                                                    AccountChangeType.BTC提现 : AccountChangeType.BTC充值)),
                                User.Identity.Name,
                                null
                            );

                            var user = db.Users.Where(a => a.UserName == u.Name).FirstOrDefault();
                            if (user != null)
                            {
                                string str = string.Format("您的比权网账户于{0}发生了一笔{2}操作，目前已到账，到账金额为 {1} 元，请注意查收。【比权网】", r.AuditTime, am.ActualDelta, r.BankRecordType.ToString());
                                qes.SendMassage2(user.PhoneNumber.ToString(), str);
                            }
                        }
                        else //审核不通过
                        {
                            if (r.BankRecordType == BankRecordType.提现 && r.coin == "CNY")
                            {
                                TraderService.OperateAccount(u, r.Delta, AccountChangeType.现金解冻, "system", null);
                            }
                        }
                    }

                    ViewBag.msg = "审核成功";

                    return PartialView();
                }
            }

            return PartialView(am);
        }
        [HttpGet]
        public PartialViewResult Bank()
        {
            return PartialView();
        }

        [HttpPost]
        public PartialViewResult Bank(BankAccount ba)
        {
            if (ModelState.IsValid)
            {
                ba.IsSystem = true;
                db.Set<BankAccount>().Add(ba);
                db.SaveChanges();
                ViewBag.msg = "银行卡添加成功";
            }
            return PartialView(ba);
        }
        QQExMailSender qes = new QQExMailSender();
        public async Task<JsonResult> IsTranPwdTrue(string code)
        {
            if (string.IsNullOrEmpty(code)) return Json(false, JsonRequestBehavior.AllowGet);
            //根据用户名与密码查询是否有该用户,如果不等于null代表登录成功
            var user = db.Users.Where(a => a.UserName == User.Identity.Name).FirstOrDefault();//密码解码查询
            if (user != null)
            {
                if (user.TradePwd == code)
                {
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(false, JsonRequestBehavior.AllowGet);
                }
            }
            else return Json(false, JsonRequestBehavior.AllowGet);
        }
        public bool IsTranPwd(string code)
        {
            if (string.IsNullOrEmpty(code)) return false;
            //根据用户名与密码查询是否有该用户,如果不等于null代表登录成功
            var user = db.Users.Where(a => a.UserName == User.Identity.Name).FirstOrDefault();//密码解码查询
            if (user != null)
            {
                if (user.TradePwd == code)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else return false;
        }
    }
}
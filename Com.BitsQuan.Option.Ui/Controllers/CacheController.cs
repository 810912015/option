using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Imp;
using Com.BitsQuan.Option.Provider;
using Com.BitsQuan.Option.Ui;
using Com.BitsQuan.Option.Ui.Models;
using Com.BitsQuan.Option.Ui.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Com.BitsQuan.Option.Match.Imp;
using Com.BitsQuan.Miscellaneous;
using System.Drawing.Imaging;
using Com.BitsQuan.Option.Ui.Areas.Supervise.Data;
using Com.BitsQuan.Option.Match.Imp.Share;
using System.Configuration;
using System.Linq.Expressions;
using System.Collections;
using System.Text;
using System.Data.Entity.Core.Objects;
using System.Data.Entity;
using Microsoft.AspNet.Identity;
using Com.BitsQuan.Option.Ui.Models.Query;
using Com.BitsQuan.Option.Match;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Com.BitsQuan.Option.Ui.Controllers
{
    [Authorize(Roles = "交易员")]
    public class CacheController : Controller
    {
        UserManager<ApplicationUser> UserManager;
        OptionDbCtx db = new OptionDbCtx();
        ApplicationDbContext adb = new ApplicationDbContext();
        DbBackModel dbm = new DbBackModel();
        public string path = ConfigurationManager.AppSettings["webSite"];
        public static readonly string ServiceQQNumber = ConfigurationManager.AppSettings["serviceQQNumber"];//客服qq
        public CacheController()
        {
            odr = new SpotOrderDealReader(db);
            UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(adb));
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (db != null)
                {
                    db.Dispose();
                    db = null;
                }
                if (adb != null)
                {
                    adb.Dispose();
                    adb = null;
                }
                if (UserManager != null)
                {
                    UserManager.Dispose();
                    UserManager = null;
                }
            }
            base.Dispose(disposing);
        }
        #region primary
        public ActionResult Index(int id = 0)
        {
            if (id != 0)
            {
                ViewData["Id"] = id;
            }
            return View();
        }


        public PartialViewResult general(int id = 0)
        {
            if (id != 0)
            {
                ViewData["handle"] = id;
            }
            var trader = MvcApplication.OptionService.Model.Traders.Where(a => a.Name == User.Identity.Name).FirstOrDefault();
            if (trader != null)
            {
                var snapshot = trader.SnapshotBail(MvcApplication.OptionService.MarketBoard);
                ViewData["snap"] = snapshot;
            }

            var tid = User.GetTraderId();
            var os = MvcApplication.OptionService.Model.Traders.Where(a => a.Id == tid).FirstOrDefault();
            return PartialView(os);
        }
        static int countPerPage = 25;

        readonly AccountChangeType[] cacheFlowChangeTypes = new AccountChangeType[] { 
            AccountChangeType.CNY充值,AccountChangeType.BTC提现,AccountChangeType.BTC充值,
            AccountChangeType.BTC冻结,AccountChangeType.CNY提现,AccountChangeType.现金冻结,
            AccountChangeType.现金解冻,AccountChangeType.现金收款,AccountChangeType.现金付款,
            AccountChangeType.现金转保证金_现金转出,AccountChangeType.保证金转现金_现金转入
        };

        public PartialViewResult CacheFlow(WhereModel model, int pageIndex = 1)
        {
            var tid = User.GetTraderId();
            ParameterExpression exprParam = Expression.Parameter(typeof(AccountTradeRecord));
            Expression exprCriteria = Expression.Equal(Expression.PropertyOrField(exprParam, "WhoId"), Expression.Constant(tid));
            if (model.StartTime.HasValue)
            {
                exprCriteria = Expression.AndAlso(exprCriteria, Expression.GreaterThanOrEqual(Expression.PropertyOrField(exprParam, "When"), Expression.Constant(model.StartTime.Value.Date)));
            }
            if (model.EndTime.HasValue)
            {
                exprCriteria = Expression.AndAlso(exprCriteria, Expression.LessThan(Expression.PropertyOrField(exprParam, "When"), Expression.Constant(model.EndTime.Value.Date.AddDays(1))));
            }
            if (Enum.IsDefined(typeof(AccountChangeType), model.Type))
            {
                exprCriteria = Expression.AndAlso(exprCriteria, Expression.Equal(Expression.PropertyOrField(exprParam, "OperateType"), Expression.Constant((AccountChangeType)model.Type)));
            }
            else
            {
                Expression exprTypes = Expression.Equal(Expression.PropertyOrField(exprParam, "OperateType"), Expression.Constant(cacheFlowChangeTypes[0]));
                for (int i=1; i < cacheFlowChangeTypes.Length; ++i)
                {
                    exprTypes = Expression.OrElse(exprTypes, Expression.Equal(Expression.PropertyOrField(exprParam, "OperateType"), Expression.Constant(cacheFlowChangeTypes[i])));
                }
                exprCriteria = Expression.AndAlso(exprCriteria, exprTypes);
            }

            Expression<Func<AccountTradeRecord,bool>> exprLambda = Expression.Lambda<Func<AccountTradeRecord, bool>>(exprCriteria, exprParam);
            var query = db.Set<AccountTradeRecord>().Where(exprLambda);
            var paged = query.OrderByDescending(a => a.When).Skip((pageIndex - 1) * countPerPage).Take(countPerPage);
            var allCount = query.Count();
            var pager = new Pager
            {
                PageCount = (int)Math.Ceiling((double)allCount / (double)countPerPage),
                PageIndex = pageIndex,
                PostAction = "CacheFlow",
                TargetId = "cache"
            }.SetPagerParam(new FlowQueryParam { StartTime = model.StartTime, EndTime = model.EndTime, Type = model.Type }.SetPageIndex(pageIndex));
            ViewData["TradeFlowType2"] = new[] { new SelectListItem { Text = "全部", Value = "-1" } }
                .Union(cacheFlowChangeTypes.Select(_ => new SelectListItem { Text = _.ToString(), Value = ((int)_).ToString() })).ToList();

            ViewData["cache"] = pager;
            ViewData["CacheFlow"] = paged.ToList();
            return PartialView(model);
        }

        readonly  AccountChangeType[] freezeFlowChangeTypes = new AccountChangeType[] { 
            AccountChangeType.保证金解冻,AccountChangeType.保证金冻结
        };
        public PartialViewResult FreezeFlow(WhereModel model, int pageIndex = 1)
        {
            var tid = User.GetTraderId();
            ParameterExpression exprParam = Expression.Parameter(typeof(AccountTradeRecord));
            Expression exprCriteria = Expression.Equal(Expression.PropertyOrField(exprParam, "WhoId"), Expression.Constant(tid));
            if (model.StartTime.HasValue)
            {
                exprCriteria = Expression.AndAlso(exprCriteria, Expression.GreaterThanOrEqual(Expression.PropertyOrField(exprParam, "When"), Expression.Constant(model.StartTime.Value.Date)));
            }
            if (model.EndTime.HasValue)
            {
                exprCriteria = Expression.AndAlso(exprCriteria, Expression.LessThan(Expression.PropertyOrField(exprParam, "When"), Expression.Constant(model.EndTime.Value.Date.AddDays(1))));
            }
            if (Enum.IsDefined(typeof(AccountChangeType), model.Type))
            {
                exprCriteria = Expression.AndAlso(exprCriteria, Expression.Equal(Expression.PropertyOrField(exprParam, "OperateType"), Expression.Constant((AccountChangeType)model.Type)));
            }
            else
            {
                Expression exprTypes = Expression.Equal(Expression.PropertyOrField(exprParam, "OperateType"), Expression.Constant(freezeFlowChangeTypes[0]));
                for (int i=1; i < freezeFlowChangeTypes.Length; ++i)
                {
                    exprTypes = Expression.OrElse(exprTypes, Expression.Equal(Expression.PropertyOrField(exprParam, "OperateType"), Expression.Constant(freezeFlowChangeTypes[i])));
                }
                exprCriteria = Expression.AndAlso(exprCriteria, exprTypes);
            }

            Expression<Func<AccountTradeRecord,bool>> exprLambda = Expression.Lambda<Func<AccountTradeRecord, bool>>(exprCriteria, exprParam);
            var query = db.Set<AccountTradeRecord>().Where(exprLambda);
            var paged = query.OrderByDescending(a => a.When).Skip((pageIndex - 1) * countPerPage).Take(countPerPage);
            var allCount = query.Count();
            ViewData["freeze"] = new Pager { PageCount = (int)Math.Ceiling((double)allCount / (double)countPerPage), PageIndex = pageIndex, PostAction = "FreezeFlow", TargetId = "freezeTable" }
                .SetPagerParam(new FlowQueryParam { StartTime = model.StartTime, EndTime = model.EndTime, Type = model.Type }.SetPageIndex(pageIndex));
            ViewData["FreezeFlow"] = paged.ToList();
            return PartialView(model);
        }

        public PartialViewResult FreezeFlow_Test(WhereModel model, int pageIndex = 1)
        {
            QueryEngine query = new QueryEngine();
            throw new Exception();
        }

        readonly AccountChangeType[] tradeFlowChangeTypes = new AccountChangeType[]{
                    AccountChangeType.保证金付款,
                    AccountChangeType.保证金收款,
                    AccountChangeType.保证金转现金,
                    AccountChangeType.佣金支付,
                    AccountChangeType.保证金转现金_保证金转出,
                    AccountChangeType.现金转保证金_保证金转入,
                    AccountChangeType.系统还款,
                    AccountChangeType.系统借款,
                    AccountChangeType.行权划出,
                    AccountChangeType.行权划入,
                    AccountChangeType.亏损分摊,
                    AccountChangeType.推荐用户奖金_划入,
                    AccountChangeType.推荐用户交易手续费返还_划入
        };
        public PartialViewResult TradeFlow(WhereModel model, int pageIndex = 1)
        {
            var tid = User.GetTraderId();
            ParameterExpression exprParam = Expression.Parameter(typeof(AccountTradeRecord));
            Expression exprCriteria = Expression.Equal(Expression.PropertyOrField(exprParam, "WhoId"), Expression.Constant(tid));
            if (model.StartTime.HasValue)
            {
                exprCriteria = Expression.AndAlso(exprCriteria, Expression.GreaterThanOrEqual(Expression.PropertyOrField(exprParam, "When"), Expression.Constant(model.StartTime.Value.Date)));
            }
            if (model.EndTime.HasValue)
            {
                exprCriteria = Expression.AndAlso(exprCriteria, Expression.LessThan(Expression.PropertyOrField(exprParam, "When"), Expression.Constant(model.EndTime.Value.Date.AddDays(1))));
            }
            if (Enum.IsDefined(typeof(AccountChangeType), model.Type))
            {
                exprCriteria = Expression.AndAlso(exprCriteria, Expression.Equal(Expression.PropertyOrField(exprParam, "OperateType"), Expression.Constant((AccountChangeType)model.Type)));
            }
            else
            {
                Expression exprTypes = Expression.Equal(Expression.PropertyOrField(exprParam, "OperateType"), Expression.Constant(tradeFlowChangeTypes[0]));
                for (int i=1; i < tradeFlowChangeTypes.Length; ++i)
                {
                    exprTypes = Expression.OrElse(exprTypes, Expression.Equal(Expression.PropertyOrField(exprParam, "OperateType"), Expression.Constant(tradeFlowChangeTypes[i])));
                }
                exprCriteria = Expression.AndAlso(exprCriteria, exprTypes);
            }

            Expression<Func<AccountTradeRecord,bool>> exprLambda = Expression.Lambda<Func<AccountTradeRecord, bool>>(exprCriteria, exprParam);
            var query = db.Set<AccountTradeRecord>().Where(exprLambda);
            var paged = query.OrderByDescending(a => a.When).Skip((pageIndex - 1) * countPerPage).Take(countPerPage);
            var allCount = query.Count();

            ViewData["TradeFlowType"] = new[] { new SelectListItem { Text = "全部", Value = "-1" } }
                .Union(tradeFlowChangeTypes.Select(_ => new SelectListItem { Text = _.ToString(), Value = ((int)_).ToString() })).ToList();
            ViewData["TradeFlow"] = paged.ToList();
            ViewData["trade"] = new Pager { PageCount = (int)Math.Ceiling((double)allCount / (double)countPerPage), PageIndex = pageIndex, PostAction = "TradeFlow", TargetId = "trade" }
                .SetPagerParam(new FlowQueryParam { StartTime = model.StartTime, EndTime = model.EndTime, Type = model.Type }.SetPageIndex(pageIndex));
            return PartialView(model);
        }
        public JsonResult AutoConvert()
        {
            var tid = User.GetTraderId();
            var t = MvcApplication.OptionService.Model.Traders.Where(a => a.Id == tid).First();
            if (t == null) return Json(false, JsonRequestBehavior.AllowGet);
            var result = !t.IsAutoAddBailFromCache;

            MvcApplication.OptionService.esi.UpdateTrader(t.Id, TraderUpdateType.设置保证金自动转入, result);
            return Json(t.IsAutoAddBailFromCache, JsonRequestBehavior.AllowGet);
        }
        public PartialViewResult Invitation()
        {
            var usr = UserManager.FindById<ApplicationUser, string>(User.Identity.GetUserId());
            ViewBag.User = usr;
            ViewBag.InviteCode = usr.CreateInvitationCode();
            ViewBag.Invited = usr.UsersInvitedByMe();
            return PartialView();
        }
        public JsonResult BeInvitor()
        {
            var usr = UserManager.FindById<ApplicationUser, string>(User.Identity.GetUserId());
            if (usr != null)
            {
                usr.IsInvitor = true;
                var r = UserManager.Update(usr);
                return Json(new { result = r.Succeeded ? (String)null: "申请失败" }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { result = "请先登录" }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult CloseRight()
        {
            var tid = User.GetTraderId();
            var t = MvcApplication.OptionService.Model.Traders.Where(a => a.Id == tid).First();
            if (t == null) return Json(false, JsonRequestBehavior.AllowGet);
            t.IsAutoSellRight = !t.IsAutoSellRight;
            return Json(t.IsAutoSellRight, JsonRequestBehavior.AllowGet);
        }
        #endregion


        #region bank
        /// <summary>
        /// 某个用户的银行卡列表
        /// </summary>
        /// <returns></returns>
        public PartialViewResult BankAccounts()
        {
            var q = adb.BankAccounts.Where(a => a.Uname == User.Identity.Name && a.IsDel == false);
            return PartialView(q);
        }
        /// <summary>
        /// 逻辑删除银行卡
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult DelBankAccount(string number)
        {
            var q = adb.BankAccounts.Where(a => a.Uname == User.Identity.Name && a.Number == number).FirstOrDefault();
            if (q == null) return Json(new OperationResult(1, "此银行卡不属于你或已删除"), JsonRequestBehavior.AllowGet);
            q.IsDel = true;
            var r = adb.SaveChanges() > 0;
            if (r) return Json(OperationResult.SuccessResult, JsonRequestBehavior.AllowGet);
            else return Json(new OperationResult(2, "删除失败"), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 逻辑删除提现地址
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        [ValidateAntiForgeryToken]
        [HttpPost]
        public JsonResult DelCurrenAddress(int Id)
        {
            var p = adb.CurrenAddress.Where(a => a.Uname == User.Identity.Name && a.Id == Id).FirstOrDefault();
            if (p == null) return Json(new OperationResult(1, "此提现地址不属于你或已删除"), JsonRequestBehavior.AllowGet);
            p.IsDel = true;
            var r = adb.SaveChanges() > 0;
            if (r) return Json(OperationResult.SuccessResult, JsonRequestBehavior.AllowGet);
            else return Json(new OperationResult(2, "删除失败"), JsonRequestBehavior.AllowGet);
        }
        //[ValidateAntiForgeryToken]
        [HttpPost]
        public JsonResult AddBankAccount(BankModel m)
        {
            string referer = HttpContext.Request.Headers["Referer"];
            if ((referer != null) && (referer.Trim().StartsWith(path)))
            {
                if (ModelState.IsValid)
                {
                    if (BankCityeProvinceTrue(m.Bank, m.City, m.Province))
                    {
                        var count = adb.Set<BankAccount>().Where(a => a.Uname == User.Identity.Name && a.IsDel == false).Count();
                        if (count >= 6) return Json(new OperationResult(1, "最多只能添加6张银行卡"), JsonRequestBehavior.AllowGet);
                        var repeat = adb.Set<BankAccount>().Where(a => a.Number == m.Number && a.IsDel == false).Count();
                        if (repeat > 0) return Json(new OperationResult(3, "此银行卡已在系统中"), JsonRequestBehavior.AllowGet);

                        var up = adb.BankAccounts.Where(a => a.Number == m.Number && a.IsDel == true);//根据地址查询(a.IsDel == true：如果是以前逻辑删除过的,则需要真正删除，再添加)
                        //    var u = adb.Set<ApplicationUser>().Where(a => a.UserName == User.Identity.Name).FirstOrDefault();

                        var r = false;
                        var acount = new BankAccount();
                        if (up.Count() > 0)//修改
                        {
                            var uu = up.FirstOrDefault();
                            uu.IsDel = false;
                            r = dbm.UpdBank(uu);

                            if (r.ToString() == "True" || r == true) return Json(new
                            {
                                ResultCode = 0,
                                BackAccount = new BankModel
                                {
                                    Number = uu.Number,
                                    Name = uu.Name,
                                    Bank = uu.BankName,
                                    Province = m.Province,
                                    City = m.City,
                                    SalesOfficeName = m.SalesOfficeName
                                }
                            }, JsonRequestBehavior.AllowGet);
                            else return Json(new OperationResult(1, "添加失败"), JsonRequestBehavior.AllowGet);

                        }
                        else
                        {


                            var ba = new BankAccount { Number = m.Number, Province = m.Province, City = m.City, BankName = m.Bank, Name = m.Name, Uname = User.Identity.Name, SalesOfficeName = m.SalesOfficeName };
                            acount = adb.BankAccounts.Add(ba);
                            r = adb.SaveChanges() > 0;

                        }
                        if (r) return Json(new
                        {
                            ResultCode = 0,
                            BackAccount = new BankModel
                            {
                                Number = acount.Number,
                                Name = acount.Name,
                                Bank = acount.BankName,
                                Province = m.Province,
                                City = m.City,
                                SalesOfficeName = m.SalesOfficeName
                            }
                        }, JsonRequestBehavior.AllowGet);
                        else return Json(new OperationResult(1, "添加失败"), JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new OperationResult(1, "提交的信息有误"), JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(new { ResultCode = 1, Errors = ModelState.GetErrorString() }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(new OperationResult(12, "Error"), JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        //[ValidateAntiForgeryToken]
        [HttpPost]
        public JsonResult IntoBail(decimal count, string trade)
        {
            if (this.IsTraderLogin())
            {
                var user = adb.Users.Where(a => a.UserName == User.Identity.Name).FirstOrDefault();
                if (trade != user.TradePwd)
                {
                    return Json(new OperationResult(1, "交易密码错误"));
                }
                var trader = MvcApplication.OptionService.Model.Traders.Where(a => a.Name == User.Identity.Name).FirstOrDefault();
                if (trader == null) { return Json(new OperationResult(1, "你没有交易权限,请联系客服")); }
                if (count <= 0) return Json(new OperationResult(2, "不能转入金额为0或负数"));

                if (count > trader.Account.CacheAccount.CnyAccount.Sum)
                    return Json(new OperationResult(3, "现金账户金额不足"));
                var r = TraderService.OperateAccount(trader, count, AccountChangeType.现金转保证金, User.Identity.Name, null);
                trader.UpdateWithdrawSumOfToday(-count);
                return Json(r ? new OperationResult(0, "转入保证金操作成功") : new OperationResult(4, "转入保证金操作失败"));
            }
            else
            {
                return Json(new OperationResult(12, "请先登录"));
            }
        }

        [HttpPost]
        public JsonResult OutFromBail(decimal count, string trade)
        {
            if (this.IsTraderLogin())
            {
                var user = adb.Users.Where(a => a.UserName == User.Identity.Name).FirstOrDefault();
                if (trade != user.TradePwd)
                {
                    return Json(new OperationResult(1, "交易密码错误"));
                }
                var trader = MvcApplication.OptionService.Model.Traders.Where(a => a.Name == User.Identity.Name).FirstOrDefault();
                if (trader == null) { return Json(new OperationResult(1, "你没有交易权限,请联系客服")); }
                if (count <= 0) return Json(new OperationResult(2, "不能转入金额为0或负数"));
                var snapshot = trader.SnapshotBail(MvcApplication.OptionService.MarketBoard);
                if (count > snapshot.Usable)
                    return Json(new OperationResult(5, "保证金不足,不能转出"));
                var last = trader.GetPreBailTotalSnap();
                lock (trader)
                {
                    var upperbound = snapshot.Usable;
                    if (last > 0)
                    {
                        upperbound = Math.Min(last, snapshot.Usable);
                    }
                    var borrow = trader.GetBorrowedSum();
                    var available = upperbound - borrow;
                    if (count > available || count + trader.GetWithdrawSumOfToday() > last)
                        return Json(new OperationResult(3, "不能转出,已大于昨日金额或您有系统借款"));

                    var r = TraderService.OperateAccount(trader, count, AccountChangeType.保证金转现金, User.Identity.Name, null);
                    if (r)
                    {
                        trader.UpdateWithdrawSumOfToday(count);//增加今日提现总额
                    }
                    return Json(r ? new OperationResult(0, "转出保证金操作成功") : new OperationResult(4, "转出保证金操作失败"));
                }
            }
            else
            {
                return Json(new OperationResult(12, "请先登录"));
            }
        }
        void PrepareRefill()
        {
            var notApproved = adb.BankRecords
                .Include("Account")
                .Where(a => a.AppUserName == User.Identity.Name && a.BankRecordType == BankRecordType.充值 && a.coin == "CNY")
                .OrderByDescending(a => a.Id).Take(10)
                .ToList();
            var accounts = adb.BankAccounts.Where(a => a.Uname == User.Identity.Name && a.IsDel == false).ToList();
            ViewData["refills"] = notApproved;
            ViewData["accounts"] = accounts;
        }
        public PartialViewResult RefillCache()
        {
            var uid = adb.Users.Where(a => a.UserName == User.Identity.Name).FirstOrDefault().Uiden;
            ViewData["Uid"] = uid;
            PrepareRefill();
            return PartialView();
        }


        [ValidateAntiForgeryToken]
        [HttpPost]
        public PartialViewResult RefillCache(BankOpModel2 bm)
        {
            string referer = HttpContext.Request.Headers["Referer"];
            if ((referer != null) && (referer.Trim().StartsWith(path)))
            {
                if (ModelState.IsValid && IsBankAccountTrue1(bm.BankAccountId))
                {
                    var uid = adb.Users.Where(a => a.UserName == User.Identity.Name).FirstOrDefault().Uiden;
                    var bo = new BankRecord
                    {
                        Id = 0,
                        AccountNum = bm.BankAccountId,
                        Delta = bm.Delta,
                        AppUserName = User.Identity.Name,
                        BankRecordType = BankRecordType.充值,
                        When = DateTime.Now,
                        coin = "CNY",
                        Uid = uid
                    };
                    adb.BankRecords.Add(bo);
                    var r = adb.SaveChanges() > 0;
                    PrepareRefill();
                    ViewBag.msg = r ? "充值成功!请等待审核" : "充值失败,请稍后重试";
                    var user = adb.Users.Where(a => a.UserName == User.Identity.Name).FirstOrDefault();
                    // qes.SendMassage(user.PhoneNumber.ToString());
                    return PartialView(r ? null : bm);
                }
                return PartialView(bm);
            }
            else
            {

                return PartialView("Error");
            }

        }
        public PartialViewResult RefillBtc()
        {
            return PartialView();
        }
        void PrepareWithDraw()
        {
            var tid = User.GetTraderId();
            var os = MvcApplication.OptionService.Model.Traders.Where(a => a.Id == tid).FirstOrDefault();
            var notApproved = adb.BankRecords
                .Include("Account")
                .Where(a => a.AppUserName == User.Identity.Name && a.BankRecordType == BankRecordType.提现 && a.coin == "CNY")
                .OrderByDescending(a => a.Id).Take(10)
                .ToList();
            var accounts = adb.BankAccounts.Where(a => a.Uname == User.Identity.Name && a.IsDel == false)
                .Select(a => new SelectListItem
                {
                    Text =
                        a.BankName + "-" + a.Number,
                    Value = a.Number
                })
                .ToList();
            BTCWithDraw();
            //查询当前用户的手机号
            var ThisUser = adb.Users.Where(a => a.UserName == User.Identity.Name).FirstOrDefault();
            ViewData["phone"] = ThisUser.PhoneNumber;
            ViewData["CnyUsable"] = os.Account.CacheAccount.CnyAccount.Sum;
            ViewData["withdraw"] = notApproved;
            ViewData["accounts"] = accounts;

        }
        [HttpGet]
        public PartialViewResult WithdrawCny()
        {
            PrepareWithDraw();
            return PartialView();
        }
        //

        public ActionResult Withdraw()
        {
            var ThisUser = adb.Users.Where(a => a.UserName == User.Identity.Name).FirstOrDefault();
            if (ThisUser.IdNumber == "" || ThisUser.IdNumber == null)
            {
                ViewBag.msg11 = "请到安全中心实名认证";
            }
            return PartialView();
        }

        public PartialViewResult Redraw()
        {
            return PartialView();
        }
        [ValidateAntiForgeryToken]
        [HttpPost]
        public PartialViewResult WithdrawCny(BankOpModel bm)
        {

            string referer = HttpContext.Request.Headers["Referer"];
            if ((referer != null) && (referer.Trim().StartsWith(path)))
            {
                if (ModelState.IsValid && (IsMoneyTrue1(bm.Delta) && IsTradeCodeTrue1(bm.Tradepwd) && IsPhoneCodeTrue1(bm.PhoneCode) && IsBankAccountTrue1(bm.BankAccountId)))
                {
                    var ThisUser = adb.Users.Where(a => a.UserName == User.Identity.Name).FirstOrDefault();
                    if (ThisUser.IdNumber == "" || ThisUser.IdNumber == null)
                    {
                        PrepareWithDraw();
                        ViewBag.msg = "请到安全中心实名认证！";

                        return PartialView(null);
                    }
                    var bo = new BankRecord
                    {
                        Id = 0,
                        AccountNum = bm.BankAccountId,
                        Delta = bm.Delta,
                        AppUserName = User.Identity.Name,
                        BankRecordType = BankRecordType.提现,
                        When = DateTime.Now,
                        coin = "CNY",
                        Uid = ThisUser.Uiden,
                    };
                    adb.BankRecords.Add(bo);
                    var r = adb.SaveChanges() > 0;
                    if (r)
                    {
                        var u = MvcApplication.OptionService.Model.Traders.Where(a => a.Name == User.Identity.Name).FirstOrDefault();
                        if (u != null)
                        {
                            var ur = TraderService.OperateAccount(u, bm.Delta, AccountChangeType.现金冻结, User.Identity.Name, null);
                            if (ur)
                            {
                                PrepareWithDraw();
                                ViewBag.msg = "提现成功!请等待审核";

                                string content = string.Format(
                                    "您的比权网账户于{0}提交了一笔{1}元的提现申请，如非本人操作，请尽快联系网站客服。【比权网】",
                                    bo.When.ToString("yyyy-MM-dd HH:mm:ss"),
                                    bo.Delta
                                );
                                qes.SendTo(ThisUser.Email, "比权网账户资产转出通知", content);
                                return PartialView(r ? null : bm);
                            }

                        }
                    }
                    PrepareWithDraw();
                    ViewBag.msg = "提现失败,请稍后重试";
                    return PartialView(r ? null : bm);
                }
                return PartialView(bm);
            }
            else
            {
                return PartialView("Error");
            }

        }

        public PartialViewResult Bail()
        {
            var trader = MvcApplication.OptionService.Model.Traders.Where(a => a.Name == User.Identity.Name).FirstOrDefault();
            if (trader != null)
            {
                var bailSnapshot = trader.SnapshotBail(MvcApplication.OptionService.MarketBoard);
                ViewData["snap"] = bailSnapshot;
                ViewBag.CacheAccount = trader.Account.CacheAccount;
            }

            return PartialView();
        }


        public PartialViewResult StreamShow()
        {
            return PartialView();
        }

        //BTC充值RedrawBTC
        void CurrenPrepareRefill()
        {
            var notApproved = adb.BankRecords
                .Include("Account")
                .Where(a => a.AppUserName == User.Identity.Name && a.BankRecordType == BankRecordType.充值 && a.coin == "BTC")
                .OrderByDescending(a => a.Id).Take(10)
                .ToList();
            var accounts = adb.CurrenAddress.Where(a => a.Uname == User.Identity.Name)
                .Select(a => new SelectListItem
                {
                    Text = a.Name,
                    Value = a.Id.ToString()
                })
                .ToList();
            ViewData["refillsCurren"] = notApproved;
            ViewData["accountsCurren"] = accounts;

        }


        public PartialViewResult RedrawBTC()
        {
            CurrenPrepareRefill();
            return PartialView();
        }

        QQExMailSender qes = new QQExMailSender();
        [HttpPost]
        public PartialViewResult RedrawBTC(OpModelBTCr bm)
        {
            if (ModelState.IsValid)
            {
                var uid = adb.Users.Where(a => a.UserName == User.Identity.Name).FirstOrDefault().Uiden;
                var bo = new BankRecord
                {
                    Id = 0,
                    AddressNum = bm.RBtcAddress,
                    Num = bm.RBtcNum,
                    AppUserName = User.Identity.Name,
                    BankRecordType = BankRecordType.充值,
                    When = DateTime.Now,
                    Address = adb.CurrenAddress.Where(a => a.Id.ToString() == bm.RBtcAddress).FirstOrDefault().Address,
                    coin = "BTC",
                    Uid = uid
                };
                adb.BankRecords.Add(bo);
                var r = adb.SaveChanges() > 0;
                CurrenPrepareRefill();
                ViewBag.msg = r ? "充值成功!请等待审核" : "充值失败,请稍后重试";
                //查询当前用户的手机号
                var user = adb.Users.Where(a => a.UserName == User.Identity.Name).FirstOrDefault();
                if (user.PhoneNumber.ToString() != "" && user.PhoneNumber != null)
                {
                    //string str = "您好，您在" + System.DateTime.Now + "时充值了" + bm.RBtcNum + "个BTC,正在等待系统审核中！！！【比权网】";
                    string str = string.Format("您的比权网账户于{0}进行了一笔转账付款交易，金额为 {1} 元，请注意查收【比权网】", System.DateTime.Now, bm.RBtcNum);
                    qes.SendMassage2(user.PhoneNumber.ToString(), str);
                }
                //qes.SendMassage(user.PhoneNumber.ToString());
                return PartialView(r ? null : bm);
            }
            return PartialView(bm);

        }









        /// <summary>
        /// btc提现
        /// </summary>
        void BTCWithDraw()
        {
            var tid = User.GetTraderId();
            var os = MvcApplication.OptionService.Model.Traders.Where(a => a.Id == tid).FirstOrDefault();
            var notApproved = adb.BankRecords
                .Include("Account")
                .Where(a => a.AppUserName == User.Identity.Name && a.BankRecordType == BankRecordType.提现 && a.coin == "BTC")
                .OrderByDescending(a => a.Id).Take(10)
                .ToList();
            var accounts = adb.CurrenAddress.Where(a => a.Uname == User.Identity.Name && a.IsDel == false)
                .Select(a => new SelectListItem
                {
                    Text = a.Name,
                    Value = a.Id.ToString()
                })
                .ToList();
            //查询当前用户的手机号
            var ThisUser = adb.Users.Where(a => a.UserName == User.Identity.Name).FirstOrDefault();
            ViewData["phone2"] = ThisUser.PhoneNumber;
            ViewData["BtcUsable"] = os.Account.CacheAccount.BtcAccount.Sum;
            ViewData["withdrawBTC"] = notApproved;
            ViewData["accountsBTC"] = accounts;


        }
        public PartialViewResult WithdrawBTC()
        {
            BTCWithDraw();
            return PartialView();
        }
        [ValidateAntiForgeryToken]
        [HttpPost]
        public PartialViewResult WithdrawBTC(OpModelBTCw bm)
        {
            string referer = HttpContext.Request.Headers["Referer"];
            if ((referer != null) && (referer.Trim().StartsWith(path)))
            {
                if (ModelState.IsValid && (IsNumTrue1(bm.Num) && IsTradeCodeTrue1(bm.Tradepwd) && IsPhoneCodeTrue1(bm.PhoneCode) && IsBtcAddressTrue1(bm.AddressNum)))
                {
                    //  var uid = adb.Users.Where(a => a.UserName == User.Identity.Name).FirstOrDefault().Uiden;

                    var ThisUser = adb.Users.Where(a => a.UserName == User.Identity.Name).FirstOrDefault();
                    if (ThisUser.IdNumber == "" || ThisUser.IdNumber == null)
                    {
                        PrepareWithDraw();
                        ViewBag.msg2 = "请到安全中心实名认证！";

                        return PartialView(null);
                    }
                    var bo = new BankRecord
                    {
                        Id = 0,
                        AddressNum = bm.AddressNum,
                        Num = bm.Num,
                        AppUserName = User.Identity.Name,
                        BankRecordType = BankRecordType.提现,
                        When = DateTime.Now,
                        Address = adb.CurrenAddress.Where(a => a.Id.ToString() == bm.AddressNum).FirstOrDefault().Address,
                        coin = "BTC",
                        Uid = ThisUser.Uiden
                    };
                    adb.BankRecords.Add(bo);
                    var r = adb.SaveChanges() > 0;
                    if (r)
                    {
                        var u = MvcApplication.OptionService.Model.Traders.Where(a => a.Name == User.Identity.Name).FirstOrDefault();
                        if (u != null)
                        {

                            var ur = TraderService.OperateAccount(u, bm.Num, AccountChangeType.BTC冻结, User.Identity.Name, null);

                            if (ur)
                            {
                                BTCWithDraw();
                                ViewBag.msg2 = "BTC提现成功!请等待审核";
                                ///////////
                                var user = adb.Users.Where(a => a.UserName == User.Identity.Name).FirstOrDefault();
                                string str = "您好，您在" + System.DateTime.Now + "时提现了" + bm.Num + "个BTC,正在等待系统审核中！！！【比权网】";
                                qes.SendMassage2(user.PhoneNumber.ToString(), str);
                                string msg = string.Format(
                                    "您的比权网账户于{0}提交了一笔{1}BTC的提现申请，如非本人操作，请尽快联系网站客服。【比权网】",
                                    bo.When.ToString(),
                                    bo.Num
                                );
                                qes.SendTo(user.Email, "比权网账户资产转出通知", msg);
                                return PartialView(r ? null : bm);
                            }

                        }
                    }
                    BTCWithDraw();
                    ViewBag.msg2 = "BTC提现失败,请稍后重试";
                    return PartialView(r ? null : bm);
                }
                return PartialView(bm);
            }
            else
            {

                return PartialView("Error");
            }

        }

        //BTC提现地址
        public PartialViewResult CurrenAddress()
        {
            var q = adb.CurrenAddress.Where(a => a.Uname == User.Identity.Name && a.IsDel == false);
            return PartialView(q);
        }
        //获得手机验证码
        [ValidateAntiForgeryToken]
        public void getOldCode(string phone)
        {
            int code = qes.SendMassage(phone);
            Session["Pcode"] = code + "," + DateTime.Now.ToString();
        }

        //验证手机验证码
        public JsonResult IsPhoneCodeTrue(string PhoneCode)
        {
            if (string.IsNullOrEmpty(PhoneCode)) return Json(false, JsonRequestBehavior.AllowGet);

            string gCode = Session["Pcode"].ToString();
            string[] gCodes = gCode.Split(',');
            if (gCodes.Length == 2)
            {
                DateTime d = DateTime.Parse(gCodes[1]);
                if (d.AddMinutes(3) < DateTime.Now)
                {
                    return Json(false, JsonRequestBehavior.AllowGet);
                }
            }
            if (gCodes[0] == PhoneCode) { return Json(true, JsonRequestBehavior.AllowGet); }

            else return Json(false, JsonRequestBehavior.AllowGet);
        }
        public bool IsPhoneCodeTrue1(string PhoneCode)
        {
            if (string.IsNullOrEmpty(PhoneCode)) return false;
            string gCode = Session["Pcode"].ToString();
            string[] gCodes = gCode.Split(',');
            if (gCodes.Length == 2)
            {
                DateTime d = DateTime.Parse(gCodes[1]);
                if (d.AddMinutes(3) < DateTime.Now)
                {
                    return false;
                }
            }
            if (gCodes[0] == PhoneCode) { return true; }
            else return false;
        }
        public bool IsBankAccountTrue1(string BankAccount)
        {
            if (string.IsNullOrEmpty(BankAccount)) return false;
            var accounts = adb.BankAccounts.Where(a => a.Uname == User.Identity.Name && a.IsDel == false && a.Number == BankAccount).ToList();
            if (accounts.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }

        }
        public bool IsBtcAddressTrue1(string BtcAddress)
        {
            if (string.IsNullOrEmpty(BtcAddress)) return false;
            var accounts = adb.CurrenAddress.Where(a => a.Uname == User.Identity.Name && a.IsDel == false && a.Id.ToString() == BtcAddress).ToList();
            if (accounts.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }

        }
        //验证交易密码
        public JsonResult IsTradeCodeTrue(string Tradepwd)
        {
            if (string.IsNullOrEmpty(Tradepwd)) return Json(false, JsonRequestBehavior.AllowGet);
            var q = adb.Users.Where(a => a.UserName == User.Identity.Name).FirstOrDefault().TradePwd;
            if (q == Tradepwd) { return Json(true, JsonRequestBehavior.AllowGet); }
            else return Json(false, JsonRequestBehavior.AllowGet);
        }
        public bool IsTradeCodeTrue1(string Tradepwd)
        {
            if (string.IsNullOrEmpty(Tradepwd)) return false;
            var q = adb.Users.Where(a => a.UserName == User.Identity.Name).FirstOrDefault().TradePwd;
            if (q == Tradepwd) { return true; }
            else return false;
        }
        //BTC是否足够
        public JsonResult IsNumTrue(decimal Num)
        {
            if (string.IsNullOrEmpty(Num.ToString())) return Json(false, JsonRequestBehavior.AllowGet);
            var sum = MvcApplication.OptionService.Model.Traders.Where(a => a.Name == User.Identity.Name).FirstOrDefault().Account.CacheAccount.BtcAccount.Sum;
            if (sum >= Num) { return Json(true, JsonRequestBehavior.AllowGet); }
            else return Json(false, JsonRequestBehavior.AllowGet);
        }
        public bool IsNumTrue1(decimal Num)
        {
            if (string.IsNullOrEmpty(Num.ToString())) return false;
            var sum = MvcApplication.OptionService.Model.Traders.Where(a => a.Name == User.Identity.Name).FirstOrDefault().Account.CacheAccount.BtcAccount.Sum;
            if (sum >= Num) { return true; }
            else return false;
        }
        //RMB是否足够
        public JsonResult IsMoneyTrue(decimal Delta)
        {
            if (string.IsNullOrEmpty(Delta.ToString())) return Json(false, JsonRequestBehavior.AllowGet);
            var max = MvcApplication.OptionService.Model.Traders.Where(a => a.Name == User.Identity.Name).FirstOrDefault().Account.CacheAccount.CnyAccount.Sum;
            if (max >= Delta) { return Json(true, JsonRequestBehavior.AllowGet); }
            else return Json(false, JsonRequestBehavior.AllowGet);
        }
        public bool IsMoneyTrue1(decimal Delta)
        {
            if (string.IsNullOrEmpty(Delta.ToString())) return false;
            var sum = MvcApplication.OptionService.Model.Traders.Where(a => a.Name == User.Identity.Name).FirstOrDefault().Account.CacheAccount.CnyAccount.Sum;
            if (sum >= Delta) { return true; }
            else return false;
        }


        /// <summary>
        /// 添加货币提现地址
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        /// 
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public JsonResult AddCurrenAddress(AddressModel m)
        {
            string referer = HttpContext.Request.Headers["Referer"];
            if ((referer != null) && (referer.Trim().StartsWith(path)))
            {
                if (ModelState.IsValid && m.Coin == "BTC")
                {
                    //   var curren = adb.Set<CurrenAddress>();
                    // var curren = adb.CurrenAddress;
                    var count = adb.CurrenAddress.Where(a => a.Uname == User.Identity.Name && a.IsDel == false).Count();
                    if (count >= 6) return Json(new OperationResult(1, "最多只能添加6个提现地址"), JsonRequestBehavior.AllowGet);
                    var repeat = adb.CurrenAddress.Where(a => a.Address == m.Address && a.IsDel == false).Count();//根据地址查询(a.IsDel == false：未删除)
                    if (repeat > 0) return Json(new OperationResult(3, "此提现地址已在系统中"), JsonRequestBehavior.AllowGet);
                    var up = adb.CurrenAddress.Where(a => a.Address == m.Address && a.IsDel == true);//根据地址查询(a.IsDel == true：如果是以前逻辑删除过的,则需要真正删除，再添加)
                    var r = false;


                    var acount = new CurrenAddress();
                    if (up.Count() > 0)//修改
                    {
                        var uu = up.FirstOrDefault();
                        uu.IsDel = false;
                        r = dbm.UpdCurrenAddress(uu);

                        if (r.ToString() == "True" || r == true) return Json(new
                              {
                                  ResultCode = 0,
                                  CurrenAddress = uu
                              }, JsonRequestBehavior.AllowGet);
                        else return Json(new OperationResult(1, "添加失败"), JsonRequestBehavior.AllowGet);

                    }
                    else
                    {//添加
                        var ba = new CurrenAddress { Id = m.Id, Name = m.Name, Address = m.Address, Uname = User.Identity.Name, Coin = "BTC" };
                        acount = adb.CurrenAddress.Add(ba);
                        r = adb.SaveChanges() > 0;
                    }

                    if (r) return Json(new
                    {

                        ResultCode = 0,
                        CurrenAddress = new AddressModel
                        {
                            Id = acount.Id,
                            Name = acount.Name,
                            Address = acount.Address,
                            Coin = acount.Coin
                        }
                    }, JsonRequestBehavior.AllowGet);
                    else return Json(new OperationResult(1, "添加失败"), JsonRequestBehavior.AllowGet);
                }
                else
                {

                    return Json(new { ResultCode = 1, Errors = ModelState.GetErrorString() }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(new OperationResult(12, "Error"), JsonRequestBehavior.AllowGet);
            }
        }
        /// <summary>
        /// BTC充值地址
        /// </summary>
        /// <returns></returns>
        public Object Image()
        {
            var r = new Com.BitsQuan.Miscellaneous.QrImageMaker();
            var bits = r.Make("");
            Response.ContentType = "image/jpeg";
            bits.Save(Response.OutputStream, ImageFormat.Jpeg);
            return bits;
        }

        SpotOrderDealReader odr;
        public PartialViewResult BtcTradeFlow(WhereModel model, int pageIndex = 1)
        {
            //TODO 未完成
            var p = odr.Query(User.Identity.Name, pageIndex, 5);
            var SelC = odr.Query(User.Identity.Name, 1, 100000);//求出总条数。自定义第一页显示这么多条
            DateTime d = DateTime.Now.AddDays(1);
            List<SpotOrderDeal> dd = new SpotOrderDealModel(p, pageIndex).Deals.OrderByDescending(a => a.MainOrderId).Where(a => a.OrderTime >= model.StartTime && a.OrderTime <= d).OrderByDescending(a => a.MainOrderId).ToList();
            List<SpotOrderDeal> cc = new SpotOrderDealModel(SelC, pageIndex).Deals.OrderByDescending(a => a.MainOrderId).Where(a => a.OrderTime >= model.StartTime && a.OrderTime <= d).OrderByDescending(a => a.MainOrderId).ToList();
            var cfc = cc.Count();
            var pager = new Pager
            {
                PageCount = (int)Math.Ceiling((double)cfc / (double)5),
                PageIndex = pageIndex,
                PostAction = "BtcTradeFlow",
                TargetId = "btctrade"
            };

            ViewData["cache2"] = pager;
            ViewData["BtcTradeFlow"] = dd;
            return PartialView(model);
        }


        //撤销（删除提现记录）
        public bool DeleteWithdarw(int id, decimal Num)
        {
            //      var uid = adb.Users.Where(a => a.UserName == User.Identity.Name).FirstOrDefault().Uiden;
            var bank = adb.BankRecords.Where(a => a.Id == id).FirstOrDefault();
            var r = dbm.DeleteBankRecords(bank);

            if (r)
            {
                var u = MvcApplication.OptionService.Model.Traders.Where(a => a.Name == User.Identity.Name).FirstOrDefault();
                if (u != null)
                {

                    var ur = TraderService.OperateAccount(u, Num, AccountChangeType.BTC冻结, User.Identity.Name, null);

                    if (ur)
                    {
                        BTCWithDraw();

                        var user = adb.Users.Where(a => a.UserName == User.Identity.Name).FirstOrDefault();
                        string str = "您好，您在" + System.DateTime.Now + "时，撤销了提现" + Num + "个BTC的操作！！！【比权网】";
                        qes.SendMassage2(user.PhoneNumber.ToString(), str);
                    }

                }
            }
            return r;

        }

        //根据条件查询
        public JsonResult CachewhereShow(int pageIndex = 1, DateTime? time = null, DateTime? time2 = null)
        {
            if (time == null || time.ToString() == "")
            {
                time = System.DateTime.MinValue;
            }
            if (time2 == null || time2.ToString() == "")
            {
                time2 = System.DateTime.MaxValue;
            }

            string where = "";
            var tid = User.GetTraderId();
            var cf = db.Set<AccountTradeRecord>().Where(a => a.Who.Id == tid
                && (a.OperateType == AccountChangeType.CNY充值 || a.OperateType == AccountChangeType.BTC提现
                || a.OperateType == AccountChangeType.BTC充值 || a.OperateType == AccountChangeType.BTC冻结
                || a.OperateType == AccountChangeType.CNY提现
                || a.OperateType == AccountChangeType.现金冻结
                || a.OperateType == AccountChangeType.现金解冻
                || a.OperateType == AccountChangeType.现金收款
                || a.OperateType == AccountChangeType.现金付款
                || a.OperateType == AccountChangeType.现金转保证金_现金转出
                || a.OperateType == AccountChangeType.保证金转现金_现金转入)).Where(a => a.When >= time && a.When <= time2).ToList();
            var cta = cf.OrderByDescending(a => a.Id).Skip((pageIndex - 1) * countPerPage).Take(countPerPage);
            var cfc = cf.Count();
            var pager = new Pager
            {
                PageCount = (int)Math.Ceiling((double)cfc / (double)countPerPage),
                PageIndex = pageIndex,
                PostAction = "CacheFlow",
                TargetId = "cache"
            };

            ViewData["cache"] = pager;
            return Json(cta, JsonRequestBehavior.AllowGet);
        }

        public PartialViewResult LossRecord()
        {
            //var tid = User.GetTraderId();
            //var list = db.Set<SysAccountRecord>().Where(a => a.Who.Id == tid).ToList();
            //return PartialView(list);
            return PartialView();
        }
        public PartialViewResult UserExecuteRecord(WhereModel mm, int pageIndex = 1)
        {
            var tid = User.GetTraderId();
            var cf = db.Set<ContractExecuteRecord>().Where(a => a.TraderId == tid);
            var cta = cf.OrderByDescending(a => a.Id).Skip((pageIndex - 1) * countPerPage).Take(countPerPage);
            var cfc = cf.Count();
            var pager = new Pager
            {
                PageCount = (int)Math.Ceiling((double)cfc / (double)countPerPage),
                PageIndex = pageIndex,
                PostAction = "UserExecuteRecord",
                TargetId = "cache"
            };

            ViewData["cache"] = pager;
            ViewData["CacheFlow"] = cta.ToList();
            return PartialView();
        }
        public PartialViewResult ExecutedRecord(WhereModel mm, int pageIndex = 1)
        {
            var cs = db.Contracts.Where(a => a.IsNotInUse == true && a.ExcuteTime < DateTime.Now)
                 .OrderByDescending(a => a.Id)
                 .Skip((pageIndex - 1) * countPerPage)
                 .Take(countPerPage).ToList();

            var pager = new Pager
            {
                PageCount = (int)Math.Ceiling((double)db.Contracts.Where(a => a.IsNotInUse == true && a.ExcuteTime < DateTime.Now).Count() / (double)countPerPage),
                PageIndex = pageIndex,
                PostAction = "UserExecuteRecord",
                TargetId = "cache2"
            };

            ViewData["cache"] = pager;
            ViewData["CacheFlow"] = cs.ToList();
            return PartialView();
        }
        public bool BankCityeProvinceTrue(string bank, string city, string province)
        {
            bool r = false;
            Dictionary<string, string> cityProvince = new Dictionary<string, string>();
            cityProvince.Add("北京市", "东城|西城|崇文|宣武|朝阳|丰台|石景山|海淀|门头沟|房山|通州|顺义|昌平|大兴|平谷|怀柔|密云|延庆");
            cityProvince.Add("上海市", "黄浦|卢湾|徐汇|长宁|静安|普陀|闸北|虹口|杨浦|闵行|宝山|嘉定|浦东|金山|松江|青浦|南汇|奉贤|崇明");
            cityProvince.Add("天津市", "和平|东丽|河东|西青|河西|津南|南开|北辰|河北|武清|红挢|塘沽|汉沽|大港|宁河|静海|宝坻|蓟县");
            cityProvince.Add("重庆市", "万州|涪陵|渝中|大渡口|江北|沙坪坝|九龙坡|南岸|北碚|万盛|双挢|渝北|巴南|黔江|长寿|綦江|潼南|铜梁 |大足|荣昌|壁山|梁平|城口|丰都|垫江|武隆|忠县|开县|云阳|奉节|巫山|巫溪|石柱|秀山|酉阳|彭水|江津|合川|永川|南川");
            cityProvince.Add("河北省", "石家庄|邯郸|邢台|保定|张家口|承德|廊坊|唐山|秦皇岛|沧州|衡水");
            cityProvince.Add("山西省", "太原|大同|阳泉|长治|晋城|朔州|吕梁|忻州|晋中|临汾|运城");
            cityProvince.Add("内蒙古自治区", "呼和浩特|包头|乌海|赤峰|呼伦贝尔盟|阿拉善盟|哲里木盟|兴安盟|乌兰察布盟|锡林郭勒盟|巴彦淖尔盟|伊克昭盟");
            cityProvince.Add("辽宁省", "沈阳|大连|鞍山|抚顺|本溪|丹东|锦州|营口|阜新|辽阳|盘锦|铁岭|朝阳|葫芦岛");
            cityProvince.Add("吉林省", "长春|吉林|四平|辽源|通化|白山|松原|白城|延边");
            cityProvince.Add("黑龙江省", "哈尔滨|齐齐哈尔|牡丹江|佳木斯|大庆|绥化|鹤岗|鸡西|黑河|双鸭山|伊春|七台河|大兴安岭");
            cityProvince.Add("江苏省", "南京|镇江|苏州|南通|扬州|盐城|徐州|连云港|常州|无锡|宿迁|泰州|淮安");
            cityProvince.Add("浙江省", "杭州|宁波|温州|嘉兴|湖州|绍兴|金华|衢州|舟山|台州|丽水");
            cityProvince.Add("安徽省", "合肥|芜湖|蚌埠|马鞍山|淮北|铜陵|安庆|黄山|滁州|宿州|池州|淮南|巢湖|阜阳|六安|宣城|亳州");
            cityProvince.Add("福建省", "福州|厦门|莆田|三明|泉州|漳州|南平|龙岩|宁德");
            cityProvince.Add("江西省", "南昌市|景德镇|九江|鹰潭|萍乡|新馀|赣州|吉安|宜春|抚州|上饶");
            cityProvince.Add("山东省", "济南|青岛|淄博|枣庄|东营|烟台|潍坊|济宁|泰安|威海|日照|莱芜|临沂|德州|聊城|滨州|菏泽");
            cityProvince.Add("河南省", "郑州|开封|洛阳|平顶山|安阳|鹤壁|新乡|焦作|濮阳|许昌|漯河|三门峡|南阳|商丘|信阳|周口|驻马店|济源");
            cityProvince.Add("湖北省", "武汉|宜昌|荆州|襄樊|黄石|荆门|黄冈|十堰|恩施|潜江|天门|仙桃|随州|咸宁|孝感|鄂州");
            cityProvince.Add("湖南省", "长沙|常德|株洲|湘潭|衡阳|岳阳|邵阳|益阳|娄底|怀化|郴州|永州|湘西|张家界");
            cityProvince.Add("广东省", "广州|深圳|珠海|汕头|东莞|中山|佛山|韶关|江门|湛江|茂名|肇庆|惠州|梅州|汕尾|河源|阳江|清远|潮州|揭阳|云浮");
            cityProvince.Add("广西壮族自治区", "南宁|柳州|桂林|梧州|北海|防城港|钦州|贵港|玉林|南宁地区|柳州地区|贺州|百色|河池");
            cityProvince.Add("海南省", "海口|三亚");
            cityProvince.Add("四川省", "成都|绵阳|德阳|自贡|攀枝花|广元|内江|乐山|南充|宜宾|广安|达川|雅安|眉山|甘孜|凉山|泸州");
            cityProvince.Add("贵州省", "贵阳|六盘水|遵义|安顺|铜仁|黔西南|毕节|黔东南|黔南");
            cityProvince.Add("云南省", "昆明|大理|曲靖|玉溪|昭通|楚雄|红河|文山|思茅|西双版纳|保山|德宏|丽江|怒江|迪庆|临沧");
            cityProvince.Add("西藏自治区", "拉萨|日喀则|山南|林芝|昌都|阿里|那曲");
            cityProvince.Add("陕西省", "西安|宝鸡|咸阳|铜川|渭南|延安|榆林|汉中|安康|商洛");
            cityProvince.Add("甘肃省", "兰州|嘉峪关|金昌|白银|天水|酒泉|张掖|武威|定西|陇南|平凉|庆阳|临夏|甘南");
            cityProvince.Add("宁夏回族自治区", "银川|石嘴山|吴忠|固原");
            cityProvince.Add("青海省", "西宁|海东|海南|海北|黄南|玉树|果洛|海西");
            cityProvince.Add("新疆维吾尔族自治区", "乌鲁木齐|石河子|克拉玛依|伊犁|巴音郭勒|昌吉|克孜勒苏柯尔克孜|博尔塔拉|吐鲁番|哈密|喀什|和田|阿克苏");
            cityProvince.Add("香港特别行政区", "香港特别行政区");
            cityProvince.Add("澳门特别行政区", "澳门特别行政区");
            cityProvince.Add("台湾省", "台北|高雄|台中|台南|屏东|南投|云林|新竹|彰化|苗栗|嘉义|花莲|桃园|宜兰|基隆|台东|金门|马祖|澎湖");
            cityProvince.Add("其它", "北美洲|南美洲|亚洲|非洲|欧洲|大洋洲");
            if (cityProvince.ContainsKey(province))
            {
                string[] s = cityProvince[province].Split('|');
                if (s.Contains(city))
                {
                    r = true;
                }
            }
            return r;
        }
    }
    public class FlowQueryParam : PagerUrlParam
    {
        public DateTime? EndTime { get; set; }
        public DateTime? StartTime { get; set; }
        public int Type { get; set; }
        public override object GetParams()
        {

            return new
            {
                PageIndex = GetPageIndex(),
                StartTime = StartTime.HasValue ? EncodeUri(StartTime.Value.ToString("yyyy-MM-dd")) : "",
                EndTime = EndTime.HasValue ? EncodeUri(EndTime.Value.ToString("yyyy-MM-dd")) : "",
                Type = Type
            };
        }

        public override PagerUrlParam Clone()
        {
            return new FlowQueryParam { StartTime = StartTime, EndTime = EndTime, Type = Type }.SetPageIndex(GetPageIndex());
        }
    }
}
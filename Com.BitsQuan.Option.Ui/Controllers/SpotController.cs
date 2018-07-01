using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Core.Spot;
using Com.BitsQuan.Option.Imp;
using Com.BitsQuan.Option.Match.Dto;
using Com.BitsQuan.Option.Match.Spot;
using Com.BitsQuan.Option.Provider;
using Com.BitsQuan.Option.Ui.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Com.BitsQuan.Option.Ui.Controllers
{
    public class SpotController : Controller
    {
        OptionDbCtx db;
        ApplicationDbContext adb;
        public UserManager<ApplicationUser> UserManager { get; private set; }
        public SpotController()
        {
            UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));
            db = new OptionDbCtx();
            odr = new SpotOrderDealReader(db);
            adb = new ApplicationDbContext();
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
        SpotService spotService
        {
            get
            {
                return MvcApplication.SpotService;
            }
        }
        // GET: Spot
        public ActionResult Trade()
        {
            if (User.Identity.Name != null && User.Identity.Name != "")
            {
                var os = MvcApplication.OptionService.Model.Traders.Where(a => a.Name == User.Identity.Name).FirstOrDefault();
                if (os != null)
                {
                    ViewData["Cnybalance"] = os.Account.CacheAccount.CnyAccount.Sum;//可用余额
                    ViewData["BTCbalance"] = os.Account.CacheAccount.BtcAccount.Sum;//可用BTC
                }

            }
            return View();
        }
        //判断用户是否登录
        public bool UserIsLogin()
        {
            var u = User.GetTraderId();

            if (u == -1)
            {
                return false;
            }
            return true;
        }

        //获得密码输入次数
        public string GetInputTradeCount()
        {
            var count = adb.Users.Where(a => a.UserName == User.Identity.Name).FirstOrDefault().tradePwdCount;
            if (count == null || count == "0")
            {
                count = "null";
            }
            return count;
        }

        public void UpdateTradeCount()
        {
            var user = adb.Users.Where(a => a.UserName == User.Identity.Name).FirstOrDefault();
            user.tradePwdCount = "11";
            adb.SaveChanges();

        }


        public JsonResult OrderIt(int coinId, Core.TradeDirectType dir, OrderPolicy policy,
            string pwd,
            decimal count, decimal price)
        {
            try
            {
                var au = UserManager.FindByName(User.Identity.Name);
                if (au == null) return Json(new OperationResult(501, "必须登录"), JsonRequestBehavior.AllowGet);
                var u = User.GetTraderId();

                if (u == -1)
                {
                    return Json(new OperationResult(1, "无此用户"), JsonRequestBehavior.AllowGet);
                }

                var ThisUser = adb.Users.Where(a => a.UserName == User.Identity.Name).FirstOrDefault();
                if (ThisUser.PhoneNumber == "" || ThisUser.PhoneNumber == null)
                {
                    return Json(new OperationResult(3, "请进入安全中心绑定手机号"), JsonRequestBehavior.AllowGet);
                }
                string pwdPolicy = GetInputTradeCount();
                if (pwdPolicy == "n" || pwdPolicy == "1")
                {
                    if (pwd != ThisUser.TradePwd)
                    {
                        return Json(new OperationResult(501, "交易密码错误"), JsonRequestBehavior.AllowGet);
                    }
                }
                if (count <= 0)
                {
                    return Json(new OperationResult(2, "数量不能小于等于0"), JsonRequestBehavior.AllowGet);
                }
                if (!policy.IsPriceValid(price))
                {
                    return Json(new OperationResult(4, "非市价策略价格必须有价格"), JsonRequestBehavior.AllowGet);
                }

                var r = spotService.AddOrder(u, coinId, dir, policy, count, price);
                return Json(new { ResultCode = r.ResultCode, Desc = r.Desc, Spot = new SpotOrderDto(r.Spot) }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Singleton<TextLog>.Instance.Error(e, "spotcontroller");
                return Json(new OperationResult(100, "服务器未知错误"), JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult BtcMarketDetail()
        {
            var m = GetBtcMarket();
            var c = GetBtcOrderContainer();
            BtcMarketDetailModel dm = new BtcMarketDetailModel
            {
                SellOrders = new List<SpotOrder>(c.SellOrders),
                BuyOrders = new List<SpotOrder>(c.BuyOrders),
                Deals = new List<SpotDeal>(m.Deals.Items).OrderByDescending(d => d.When).ToList()
            };
            return View(dm);
        }
        public PartialViewResult GetHotDeals()
        {
            var m = GetBtcMarket();
            var md = m.Deals.Items.OrderByDescending(a => a.Id).Take(16);
            return PartialView(md);
        }

        public PartialViewResult GetCurrent()
        {
            var q = MvcApplication.SpotService.Container.Get(
                MvcApplication.SpotService.Model.Model.Coins.Where(a => a.Id == 2).FirstOrDefault()
                );
            var s = new SpotD2Model(q.SellOrders, q.BuyOrders);
            return PartialView(s);
        }
        CoinOrderContainer GetBtcOrderContainer()
        {
            return MvcApplication.SpotService.Container.Get(
               MvcApplication.SpotService.Model.Model.Coins.Where(a => a.Id == 2).FirstOrDefault()
               );
        }
        public PartialViewResult MyCurrent()
        {
            var tid = User.GetTraderId();
            if (tid == -1) return PartialView();
            var q = GetBtcOrderContainer();
            var ms = q.SellOrders.Where(a => a.TraderId == tid).ToList();
            var mb = q.BuyOrders.Where(a => a.TraderId == tid).ToList();
            List<SpotOrderDto> r = new List<SpotOrderDto>();
            if (ms != null)
                foreach (var v in ms)
                    r.Add(new SpotOrderDto(v));
            if (mb != null)
                foreach (var v in mb)
                    r.Add(new SpotOrderDto(v));
            r = r.OrderByDescending(a => a.Id).ToList();
            return PartialView(r);
        }

        public JsonResult GetOrders()
        {
            var tid = User.GetTraderId();
            if (tid == -1) return Json(new { Result = 1, Spot = new List<SpotOrderDto>() }, JsonRequestBehavior.AllowGet);
            var q = GetBtcOrderContainer();
            var ms = q.SellOrders.Where(a => a.TraderId == tid).ToList();
            var mb = q.BuyOrders.Where(a => a.TraderId == tid).ToList();
            List<SpotOrderDto> r = new List<SpotOrderDto>();
            if (ms != null)
                foreach (var v in ms)
                    r.Add(new SpotOrderDto(v));
            if (mb != null)
                foreach (var v in mb)
                    r.Add(new SpotOrderDto(v));
            r = r.OrderByDescending(a => a.Id).ToList();
            return Json(new { Result = 0, Spot = r }, JsonRequestBehavior.AllowGet);
        }

        public PartialViewResult MyHistoryOrder(int pageIndex = 1)
        {
            if (!User.Identity.IsAuthenticated)
            {
                ViewBag.msg = "请先登录"; return PartialView();
            }
            if (pageIndex <= 0) pageIndex = 1;
            var uid = User.GetTraderId();
            var q = db.SpotOrders.Where(a => a.TraderId == uid).OrderByDescending(a => a.Id)
                .Skip((pageIndex - 1) * 10).Take(10).ToList();
            var r = new HisSpotOrderModel(q, pageIndex);
            return PartialView(r);

        }

        SpotOrderDealReader odr;
        public PartialViewResult MySpotOrderDeal(int pageIndex = 1)
        {
            if (!User.Identity.IsAuthenticated)
            {
                ViewBag.msg = "请先登录"; return PartialView();
            }
            if (pageIndex <= 0) pageIndex = 1;
            var r = odr.Query(User.Identity.Name, pageIndex);
            return PartialView(new SpotOrderDealModel(r, pageIndex));
        }

        public JsonResult Redo(int soId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new OperationResult(1, "请先登录"), JsonRequestBehavior.AllowGet);
            }
            var uid = User.GetTrader();
            if (uid == null)
            {
                return Json(new OperationResult(2, "请先登录"), JsonRequestBehavior.AllowGet);
            }
            var so = uid.GetSpotOrders();

            //if (so == null)
            //{
            //    return Json(new OperationResult(3, "您没有此编号的委托"), JsonRequestBehavior.AllowGet);
            //}
            //var rr = so.Where(a => a.Id == soId).FirstOrDefault();

            //if (rr == null)
            //{
            //    return Json(new OperationResult(3, "您没有此编号的委托"), JsonRequestBehavior.AllowGet);
            //}
            var r = MvcApplication.SpotService.Redo(soId);
            var rc = r ? 0 : 4;
            var td = r ? "撤单成功" : "撤单失败";
            return Json(new OperationResult(rc, td), JsonRequestBehavior.AllowGet);
        }
        SpotMarketItem GetBtcMarket()
        {
            return MvcApplication.SpotService.Market.Get(MvcApplication.SpotService.Model.Model.Coins.Where(a => a.Id == 2).FirstOrDefault());
        }
        public JsonResult GetMarket()
        {
            var q = GetBtcOrderContainer();
            var m = GetBtcMarket();
            return Json(new BtcMarket
            {
                NewBtc = m.NewestDealPrice == 0 ? BtcPrice.Current : m.NewestDealPrice,
                S1Price = q.SellOrders.Count == 0 ? 0 : q.SellOrders[0].Price,
                S1Count = q.SellOrders.Count == 0 ? 0 : q.SellOrders[0].Count,
                B1Price = q.BuyOrders.Count == 0 ? 0 : q.BuyOrders[0].Price,
                B1Count = q.BuyOrders.Count == 0 ? 0 : q.BuyOrders[0].Count,
                Total24 = m.TotalCount,
                Min24 = m.Min24,
                Max24 = m.Max24


            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetDeepth(string coinName = "BTC")
        {
            var dd = MvcApplication.SpotService.DeepPool.Get(coinName);
            return Json(dd.List, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetBtcKline()
        {
            var kd1 = MvcApplication.SpotService.GetKlineDataByCoinId(2);
            return Json(kd1, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetBtcKlineNow()
        {
            var kd1 = MvcApplication.SpotService.GetLatestKline();
            return Json(kd1, JsonRequestBehavior.AllowGet);
        }
    }
    public class BtcMarketDetailModel
    {
        public List<SpotOrder> SellOrders { get; set; }
        public List<SpotOrder> BuyOrders { get; set; }
        public List<SpotDeal> Deals { get; set; }
    }
    public class BtcMarket
    {
        public decimal NewBtc { get; set; }
        public decimal S1Price { get; set; }
        public decimal S1Count { get; set; }
        public decimal B1Price { get; set; }
        public decimal B1Count { get; set; }
        public decimal Max24 { get; set; }
        public decimal Min24 { get; set; }
        public decimal Total24 { get; set; }
    }
}
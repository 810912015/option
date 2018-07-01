using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Imp;
using Com.BitsQuan.Option.Match;
using Com.BitsQuan.Option.Match.Dto;
using Com.BitsQuan.Option.Match.Imp;
using Com.BitsQuan.Option.Provider;
using Com.BitsQuan.Option.Ui;
using Com.BitsQuan.Option.Ui.Extensions;
using Com.BitsQuan.Option.Ui.Areas.Supervise.Data;
using Com.BitsQuan.Option.Ui.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Com.BitsQuan.Miscellaneous;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;


namespace System.Web.Mvc
{
    public static class WebMvcHelper
    {
        public static string Render(this Controller controller, string viewName, object model = null, bool partial = false)
        {
            return Render(controller.ControllerContext, viewName, model, partial);
        }

        public static string Render(this Controller controller, ViewResultBase viewResult)
        {
            return Render(controller.ControllerContext, viewResult);
        }

        public static string Render(this ControllerContext context, string viewName, object model = null, bool partial = false)
        {
            if (context == null) throw new ArgumentNullException("context");
            var controller = context.Controller;
            if (model != null) controller.ViewData.Model = model;
            ViewResultBase viewResult;
            if (partial) viewResult = new PartialViewResult();
            else viewResult = new ViewResult { MasterName = null };
            viewResult.ViewData = controller.ViewData;
            viewResult.TempData = controller.TempData;
            viewResult.ViewName = viewName;
            return Render(context, viewResult);
        }

        public static string Render(this ControllerContext context, ViewResultBase viewResult)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (string.IsNullOrEmpty(viewResult.ViewName)) viewResult.ViewName = context.RouteData.GetRequiredString("action");

            ViewEngineResult viewEngineResult = null;
            if (viewResult.View == null)
            {
                viewEngineResult = (viewResult is PartialViewResult) ?
                           ViewEngines.Engines.FindPartialView(context, viewResult.ViewName) :
                           ViewEngines.Engines.FindView(context, viewResult.ViewName, (viewResult as ViewResult).MasterName);
                viewResult.View = viewEngineResult.View;
            }
            try
            {
                var output = new StringWriter(CultureInfo.InvariantCulture);
                var viewContext = new ViewContext(context, viewResult.View, viewResult.ViewData, viewResult.TempData, output);
                viewResult.View.Render(viewContext, output);
                return output.ToString();
            }
            finally
            {
                if (viewEngineResult != null) viewEngineResult.ViewEngine.ReleaseView(context, viewResult.View);
            }
        }
    }
    [System.Runtime.InteropServices.GuidAttribute("4E228994-5CCB-40A2-914D-71DC3FED51BE")]
    public static class IdentityExtension
    {
        public static int GetTraderId(this System.Security.Principal.IPrincipal user)
        {
            try
            {
                if (user.Identity == null || string.IsNullOrEmpty(user.Identity.Name)) return -1;
                var trader = MvcApplication.OptionService.Model.Traders.Where(a => a.Name == user.Identity.Name).FirstOrDefault();
                if (trader == null) return -1;
                return trader.Id;
            }
            catch (Exception e)
            {
                Singleton<TextLog>.Instance.Error(e);
                return -1;
            }
        }
        public static Trader GetTrader(this System.Security.Principal.IPrincipal user)
        {
            try
            {
                if (user.Identity == null || string.IsNullOrEmpty(user.Identity.Name)) return null;
                var trader = MvcApplication.OptionService.Model.Traders.Where(a => a.Name == user.Identity.Name).FirstOrDefault();
                return trader;
            }
            catch (Exception e)
            {
                Singleton<TextLog>.Instance.Error(e, user == null ? "用户为空" : user.Identity == null ? "身份为空" : user.Identity.Name == null ? "姓名为空" : user.Identity.Name);
                return null;
            }
        }
        public static Trader GetTrader(this ApplicationUser self)
        {
            return MvcApplication.OptionService.Model.Traders.Where(a => a.Name == self.UserName).FirstOrDefault();
        }
        public static string GetErrorString(this ModelStateDictionary ms)
        {
            StringBuilder sb = new StringBuilder();
            //获取所有错误的Key
            List<string> Keys = ms.Keys.ToList();
            //获取每一个key对应的ModelStateDictionary
            foreach (var key in Keys)
            {
                if (ms[key].Errors.Count == 0) continue;
                sb.AppendFormat("{0}:", key);
                var errors = ms[key].Errors.ToList();
                //将错误描述添加到sb中
                foreach (var error in errors)
                {
                    sb.AppendFormat("{0},", error);
                }
                sb.Append(";");
            }
            return sb.ToString();
        }

        const string KEY = "88574A699F8D1948";
        const string VEC = "305AE66035564493";
        public const string INVITEBY_KEY = "inviteby";
        static readonly Rijndael Encryptor = Rijndael.Create();

        public static string CreateInvitationCode(this ApplicationUser self)
        {
            return CryptologyUtil.SymmetryEncrypt<Rijndael>(Encryptor, self.UserName, Encoding.UTF8, KEY, VEC);
        }
        public static string UserNameFromInvitationCode(string code)
        {
            return CryptologyUtil.SymmetryDecrypt<Rijndael>(Encryptor, code, Encoding.UTF8, KEY, VEC);
        }

        /// <summary>
        /// 获取推荐人
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static ApplicationUser GetInvitor(this ApplicationUser self)
        {
            var nil = new ApplicationUser { Id = "", UserName = "" };
            if (self.InvitorId != null)
            {
                using (var db = new ApplicationDbContext())
                {
                    return db.Users.FirstOrDefault(_ => _.Id == self.InvitorId) ?? nil;
                }
            }
            return nil;
        }

        /// <summary>
        /// 获取本用户推荐的用户
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static IList<ApplicationUser> UsersInvitedByMe(this ApplicationUser self)
        {
            using (var db = new ApplicationDbContext())
            {
                return db.Users.Where(_ => _.InvitorId == self.Id && _.PhoneNumberConfirmed && _.IdNumber != null && _.IdNumber.Trim().Length > 0).ToList();
            }
        }
    }
}
namespace Com.BitsQuan.Option.Ui.Controllers
{

    public class HomeController : Controller
    {
        OptionDbCtx db;
        ApplicationDbContext adb;
        public UserManager<ApplicationUser> UserManager { get; private set; }
        public HomeController()
        {
            UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));
            db = new OptionDbCtx();
            odr = new OrderDealReader(db);
            adb = new ApplicationDbContext();
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (UserManager != null)
                {
                    UserManager.Dispose();
                    UserManager = null;
                }
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
                if (dbm != null)
                {
                    dbm.Dispose();
                    dbm = null;
                }
            }
            base.Dispose(disposing);
        }
        public ActionResult Index()
        {
            ViewBag.FriendLinks = adb.FriendlyLinks.ToList();
            ViewBag.News = adb.Helps.Where(t => t.type == ContentType.新闻中心).OrderByDescending(_ => _.Id).Take(3).ToList();
            ViewBag.Advs = adb.AdvImgs.OrderBy(f => f.SortId).Take(15).ToList();
            ViewData["ActCapital"] = MvcApplication.OptionService.Model.Traders.Where(a => a.Id == User.GetTraderId()).FirstOrDefault();
            return View();
        }
        TradeManager tradeMgr = new TradeManager();

        /// <summary>
        /// 活动页面
        /// </summary>
        /// <returns></returns>
        public ActionResult Activity()
        {
            return View();
        }

        public ActionResult Simulation()
        {
            return View();
        }


        /// <summary>
        /// 获取某日排名
        /// </summary>
        /// <returns></returns>
        public JsonResult GetTopTradersOfSomeDay(DateTime date)
        {
            var query = db.Database.SqlQuery<AccountTradeRecord>(@"select top 3 * from AccountTradeRecords where Id in(
select MAX(id) from AccountTradeRecords 
where datediff(DAYOFYEAR,[When],@p0)=0 and whoid>0
group by WhoId) order by [Current] desc", date.ToString());
            var data = query.ToList();
            data.ForEach(_ => _.Who = db.Traders.FirstOrDefault(_t => _.WhoId == _t.Id));

            return Json(data.Select(_ => new { name = _.Who.Name.Replace(2, _.Who.Name.Length - 3, '*'), money = _.Current.ToString("F2") }), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Market() { return View(); }
        public ActionResult BtcIndex()
        {
            ViewBag.Max = BtcPrice.ExtremIn5.MaxIn5Min;
            ViewBag.Min = BtcPrice.ExtremIn5.MinIn5Min;
            ViewBag.Current = BtcPrice.Current;
            var markets = new List<BitcoinMarketBase>(5);
            markets.Add(new Btcc(markets));
            markets.Add(new OkCoin(markets));
            markets.Add(new Huobi(markets));
            ViewBag.OtherMarkets = markets;
            return View();
        }

        public JsonResult GetOptionMarket()
        {
            var positionsSum = MvcApplication.OptionService.Model.Traders.Sum(_ => _.GetPositionSummaries().Where(_ps => _ps.PositionType == PositionType.义务仓.ToString()).Sum(_ps => _ps.Count));
            var deals = 0;
            foreach (var c in MvcApplication.OptionService.Model.Contracts)
            {
                var market = MvcApplication.OptionService.MarketBoard.Get(c.Name);
                if (market != null)
                {
                    deals += market.Copis;
                }
            }
            var result = new { btcIndex = BtcPrice.Current, positions = positionsSum, deals = deals };
            return Json(result, JsonRequestBehavior.AllowGet);
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


        /// <summary>
        /// 下单(委托)
        /// </summary>
        /// <param name="code"></param>
        /// <param name="policy"></param>
        /// <param name="price"></param>
        /// <param name="count"></param>
        /// <param name="direct"></param>
        /// <param name="openclose"></param>
        /// <returns></returns>
        public JsonResult OrderIt(string code, OrderPolicy policy,
            decimal price, int count, TradeDirectType direct,
            OrderType openclose, string userOpId, string pwd = "")
        {
            try
            {
                var au = UserManager.FindByName(User.Identity.Name);
                if (au == null) return Json(new OperationResult(501, "必须登录"), JsonRequestBehavior.AllowGet);
                var u = User.GetTraderId();
                if (u <= 0) return Json(new OperationResult { ResultCode = 500, Desc = "必须登录" }, JsonRequestBehavior.AllowGet);
                string pwdPolicy = GetInputTradeCount();
                if (pwdPolicy == "n" || pwdPolicy == "1")
                {
                    if (pwd != au.TradePwd)
                    {
                        return Json(new OperationResult(501, "交易密码错误"), JsonRequestBehavior.AllowGet);
                    }
                }
                var ThisUser = adb.Users.Where(a => a.UserName == User.Identity.Name).FirstOrDefault();
                if (ThisUser.PhoneNumber == "" || ThisUser.PhoneNumber == null)
                {
                    return Json(new OperationResult(1, "请进入安全中心绑定手机号"), JsonRequestBehavior.AllowGet);
                }
                var rr = tradeMgr.OrderIt(u, code, policy, price, count, direct, openclose, userOpId);
                return Json(rr, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Singleton<TextLog>.Instance.Error(e, "order it in home");
                return Json(new OperationResult { ResultCode = 600, Desc = "服务器未知错误" }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetMyInfo()
        {
            var tq = MvcApplication.OptionService.Model.Traders.Where(a => a.Name == User.Identity.Name);
            if (tq == null || tq.Count() == 0)
            {
                return Json(new MyInfo(), JsonRequestBehavior.AllowGet);
            }
            var t = tq.First();


            return Json(new MyInfo(t.Account.BailAccount.Sum, tradeMgr.GetMyOrders(t.Id), tradeMgr.GetMyPositions(t.Id)), JsonRequestBehavior.AllowGet);
        }
        MyBailAccount MakeBail()
        {
            var tq = MvcApplication.OptionService.Model.Traders.Where(a => a.Name == User.Identity.Name);
            if (tq == null || tq.Count() == 0) return new MyBailAccount();
            var t = tq.First();
            return t.SnapshotBail(MvcApplication.OptionService.MarketBoard);
        }
        public JsonResult RefreshMyBail()
        {
            var tq = MvcApplication.OptionService.Model.Traders.Where(a => a.Name == User.Identity.Name);
            if (tq == null || tq.Count() == 0) return Json(new MyBailAccount(), JsonRequestBehavior.AllowGet);
            var t = tq.First();
            return Json(t.SnapshotBail(MvcApplication.OptionService.MarketBoard), JsonRequestBehavior.AllowGet);
        }

        public JsonResult Redo(int orderId, string pwd)
        {
            var tid = User.GetTraderId();
            if (tid == -1) return Json(new { ResultCode = 1, Desc = "你没有交易权限,请登录" }, JsonRequestBehavior.AllowGet);
            var rr = tradeMgr.Redo(User.GetTraderId(), orderId);
            return Json(rr, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Trade()
        {
            return View();
        }
        public ActionResult GetAllOs(string contractCode = "115001")
        {
            var c = MvcApplication.OptionService.Model.Contracts.Where(a => a.Code == contractCode && a.IsDel == false).FirstOrDefault();
            ViewBag.contract = contractCode;
            ViewBag.contractName = c.Name;
            ViewBag.cl = MvcApplication.OptionService.Model.Contracts.Where(a => a.IsDel == false).ToList();
            var m = tradeMgr.GetOrdersInMarket(contractCode, 100);
            return View(m);
        }
        public PartialViewResult GetOs(string contractCode = "115001", double ts = 0)
        {
            var m = tradeMgr.GetOrdersInMarket(contractCode, 100);

            return PartialView(m);
        }

        public PartialViewResult GetCurrentDequeue(string contractCode = "115001", double ts = 0)
        {
            var m = tradeMgr.GetOrdersInMarket(contractCode, 8);

            return PartialView("GetCurrentDequeue", m);
        }
        public PartialViewResult AllDealsPartial(string contractName = "BTC20150101购100", double ts = 0)
        {
            List<Deal> r = MvcApplication.OptionService.MarketBoard.Deals
                .GetDealsByContract(contractName);
            if (r != null)
            {
                r = r.OrderByDescending(a => a.Id).ToList();
            }
            return PartialView(r);
        }

        public PartialViewResult GetDeals(string contractName = "BTC20150101购100", double ts = 0)
        {
            List<Deal> r = MvcApplication.OptionService.MarketBoard.Deals
                .GetDealsByContract(contractName);
            if (r != null)
            {
                r = r.OrderByDescending(a => a.Id).ToList();
                if (r.Count > 10) r = r.Take(16).ToList();
            }
            return PartialView("GetDeals", r);
        }

        public JsonResult Refresh(string code, double ts = 0)
        {
            try
            {
                var q = MvcApplication.OptionService.Model.Contracts.Where(a => a.Code == code).FirstOrDefault();
                if (q == null) q = MvcApplication.OptionService.Model.Contracts.FirstOrDefault();
                if (q == null) { return Json(null, JsonRequestBehavior.AllowGet); };

                RefreshModel rm = new RefreshModel();
                var d = GetDeals(q.Name);
                rm.DealStr = this.Render(d);
                var o = GetCurrentDequeue(q.Code);
                rm.OrderStr = this.Render(o);
                rm.Market = MakeMarket(q.Name);
                //rm.Market.Main.PositionTotal = 0;
                rm.Bail = MakeBail();
                return Json(rm, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Singleton<TextLog>.Instance.Error(e, "refresh in home");
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 获取指定时间之后的交易
        /// </summary>
        /// <param name="cname"></param>
        /// <returns></returns>
        public JsonResult CurDeals(DateTime? after, string cname = "")
        {
            try
            {

                var r = tradeMgr.GetDealsInMarket(cname);

                return Json(new { ResultCode = 0, Deals = r }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new OperationResult(1, e.Message), JsonRequestBehavior.AllowGet);
            }
        }
        /// <summary>
        /// 获取当前合约的k线数据-ohlc
        /// </summary>
        /// <returns></returns>
        public JsonResult FakeKline(string code)
        {
            var kd1 = MvcApplication.OptionService.ks.GetKlineDataByContractCode(code);
            return Json(kd1, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 获取最新的一条5分钟ohlc数据
        /// </summary>
        /// <returns></returns>
        public JsonResult GetNow(string code)
        {
            try
            {
                var kd1 = MvcApplication.OptionService.ks.GetLatestKline(code, OhlcType.M5); //MvcApplication.OptionService.GetKlineDataByContractCode(code);
                if (kd1 == null) kd1 = new List<double>();
                return Json(kd1,
                    //kd1.GetLastByType(OhlcType.M5).List, 
                JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Singleton<TextLog>.Instance.Error(e);
                return Json(new List<double>(), JsonRequestBehavior.AllowGet);
            }
        }

        MyMarket MakeMarket(string cname)
        {
            var tq = MvcApplication.OptionService.Model.Traders.Where(a => a.Name == User.Identity.Name);
            if (tq == null || tq.Count() == 0)
            {
                MyMarket mm = new MyMarket();

                mm.Main = tradeMgr.QueryMarket(cname); mm.Main.GetPositionTotal();
                mm.Related = new List<MarketDto>();
                return mm;
            }
            else
            {
                MyMarket mm = new MyMarket();

                mm.Main = tradeMgr.QueryMarket(cname);

                mm.Main.GetPositionTotal();

                mm.Related = new List<MarketDto>();
                var trader = tq.First();
                var ps = trader.GetPositionSummaries();
                List<string> l = new List<string>();
                foreach (var v in ps)
                {
                    if (!l.Contains(v.CName))
                    {
                        l.Add(v.CName);
                        var tm = tradeMgr.QueryMarket(v.CName);
                        tm.GetPositionTotal();
                        mm.Related.Add(tm);
                    }
                }

                return mm;
            }
        }

        public JsonResult QueryMarket(string cname, double ts = 0)
        {
            var tq = MvcApplication.OptionService.Model.Traders.Where(a => a.Name == User.Identity.Name);
            if (tq == null || tq.Count() == 0)
            {
                MyMarket mm = new MyMarket();

                mm.Main = tradeMgr.QueryMarket(cname); mm.Main.GetPositionTotal();
                mm.Related = new List<MarketDto>();
                return Json(mm, JsonRequestBehavior.AllowGet);
            }
            else
            {
                MyMarket mm = new MyMarket();

                mm.Main = tradeMgr.QueryMarket(cname);
                mm.Main.GetPositionTotal();
                mm.Related = new List<MarketDto>();
                var trader = tq.First();
                var ps = trader.GetPositionSummaries();
                List<string> l = new List<string>();
                foreach (var v in ps)
                {
                    if (!l.Contains(v.CName))
                    {
                        l.Add(v.CName);
                        var tm = tradeMgr.QueryMarket(v.CName); tm.GetPositionTotal();
                        mm.Related.Add(tm);
                    }
                }

                return Json(mm, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetDeepth(string contractName = "", double ts = 0)
        {
            var dd = MvcApplication.OptionService.DeepPool.Get(contractName);
            return Json(dd.List, JsonRequestBehavior.AllowGet);
        }


        public ActionResult GetOption()
        {
            //查询合约（期权）信息
            List<MarketDto> list = new List<MarketDto>();
            MarketDto mk = null;
            var Contracts = MvcApplication.OptionService.Model.Contracts.Where(a => a.IsDel == false);
            foreach (var item in Contracts)
            {
                var mt = MvcApplication.OptionService.MarketBoard.Get(item.Name);
                if (mt == null)
                {
                    mk = new MarketDto
                     {
                         Code = item.Code,
                         Name = item.Name,
                         Newest = 0,
                         HitPrice = 0,
                         LowPrice = 0,
                         PositionTotal = 0,
                         Times = 0
                     };
                }
                else
                {
                    mk = new MarketDto(mt, MvcApplication.OptionService.Matcher.Container.Get1PriceAndCount);
                    var os = MvcApplication.OptionService.Model.Traders.Where(s => s.Positions.Count > 0).ToList();
                    foreach (var o in os)
                    {
                        var oso = o.GetPositionSummaries().Select(a => new PositionSummaryDto(a,
               MvcApplication.OptionService.MarketBoard.GetNewestPrice(a.CName), 0
               )).Where(a => a.Contract.Code == mk.Code && a.PositionType == PositionType.义务仓.ToString()).FirstOrDefault();
                        if (oso != null)
                        {
                            mk.PositionTotal += oso.Count;
                        }

                    }

                }
                mk.GetPositionTotal();
                list.Add(mk);
            }
            return View(list.OrderBy(d => d.Code.Substring(1, d.Code.Length - 1)));
        }
        public ActionResult Broser()
        {
            //查询合约（期权）信息
            return View();

        }
        DbBackModel dbm = new DbBackModel();
        //新闻
        public ActionResult GetNews()
        {
            var news = dbm.Helps.Where(t => t.type == ContentType.期权知识).OrderByDescending(_ => _.Id).Take(6);

            return View(news);
        }

        //公告ViewPage1
        public ActionResult GetNotice()
        {
            var news = dbm.Helps.Where(t => t.type == ContentType.行业新闻).OrderByDescending(_ => _.Id).Take(6);

            return View(news);
        }


        OrderDealReader odr;
        public PartialViewResult GetOrderDeal(int pageIndex = 1)
        {
            if (!User.Identity.IsAuthenticated)
            {
                ViewBag.msg = "请先登录"; return PartialView();
            }
            if (pageIndex <= 0) pageIndex = 1;
            var r = odr.Query(User.Identity.Name, pageIndex);
            return PartialView(new OrderDealModel(r, pageIndex));
        }

        public PartialViewResult GetHistoryOrder(int pageIndex = 1)
        {
            if (!User.Identity.IsAuthenticated)
            {
                ViewBag.msg = "请先登录"; return PartialView();
            }
            if (pageIndex <= 0) pageIndex = 1;
            var uid = User.GetTraderId();
            var q = db.Orders.Where(a => a.TraderId == uid).OrderByDescending(a => a.Id)
                .Skip((pageIndex - 1) * 10).Take(10).ToList();
            var r = new HisOrderModel(q, pageIndex);
            return PartialView(r);

        }
        public Object Image()
        {
            var r = new Com.BitsQuan.Miscellaneous.QrImageMaker();
            var bits = r.Make("");
            Response.ContentType = "image/jpeg";
            bits.Save(Response.OutputStream, ImageFormat.Jpeg);
            return bits;
        }


        //获得密码输入次数
        public string GetInputTradeCount()
        {
            try
            {
                var u = adb.Users.Where(a => a.UserName == User.Identity.Name).FirstOrDefault();
                if (u == null) return "null";
                var count = u.tradePwdCount;
                if (count == null || count == "0")
                {
                    count = "null";
                }
                return count;
            }
            catch (Exception ex)
            {
                Singleton<TextLog>.Instance.Error(ex, "GetInputTradeCount");
                return "null";
            }
        }
        //修改密码输入次数（作用于登录只输入一次）
        public void UpdateTradeCount()
        {
            var user = adb.Users.Where(a => a.UserName == User.Identity.Name).FirstOrDefault();
            user.tradePwdCount = "11";
            adb.SaveChanges();

        }

    }

    public abstract class BitcoinMarketBase
    {
        protected string GetResponse()
        {
            return HttpExecutor.Get(Url);
        }
        protected IEnumerable<BitcoinMarketBase> _collect;
        public BitcoinMarketBase(IEnumerable<BitcoinMarketBase> collect)
        {
            this._collect = collect;
        }

        protected abstract string Url { get; }
        public abstract string Name { get; }
        public decimal NewDealPrice { get; protected set; }
        public decimal MaxPrice { get; protected set; }
        public decimal MinPrice { get; protected set; }
        public double Percent
        {
            get
            {
                return (double)(NewDealPrice / _collect.Sum(_ => _.NewDealPrice));
            }
        }
    }
    public class Btcc : BitcoinMarketBase
    {
        public override string Name
        {
            get { return "btcc"; }
        }

        public Btcc(IEnumerable<BitcoinMarketBase> collect)
            : base(collect)
        {
            try
            {
                var r = GetResponse();
                var info = JObject.Parse(r)["ticker_btccny"];
                NewDealPrice = (decimal)info["vwap"];
                MaxPrice = (decimal)info["high"];
                MinPrice = (decimal)info["low"];

            }
            catch (Exception ex)
            {
                Singleton<TextLog>.Instance.Error(ex);
            }
        }

        protected override string Url
        {
            get { return "https://data.btcchina.com/data/ticker?market=all"; }
        }
    }

    public class OkCoin : BitcoinMarketBase
    {
        public override string Name
        {
            get { return "Okcoin"; }
        }

        public OkCoin(IEnumerable<BitcoinMarketBase> collect)
            : base(collect)
        {
            try
            {
                var r = GetResponse();
                var info = JObject.Parse(r);
                NewDealPrice = (decimal)info["btcLast"];
                MaxPrice = (decimal)info["high"];
                MinPrice = (decimal)info["low"];
            }
            catch (Exception ex)
            {
                Singleton<TextLog>.Instance.Error(ex);
            }
        }

        protected override string Url
        {
            get { return "https://www.okcoin.cn/real/ticker.do"; }
        }
    }
    public class Huobi : BitcoinMarketBase
    {
        public override string Name
        {
            get { return "Huobi"; }
        }

        public Huobi(IEnumerable<BitcoinMarketBase> collect)
            : base(collect)
        {
            try
            {
                var r = GetResponse();
                var start = r.IndexOf('(');
                var end = r.IndexOf(')');
                var str = r.Substring(start + 1, end - start - 1);
                var info = JObject.Parse(str);
                NewDealPrice = (decimal)info["p_new"];
                MaxPrice = (decimal)info["p_high"];
                MinPrice = (decimal)info["p_low"];

            }
            catch (Exception ex)
            {
                Singleton<TextLog>.Instance.Error(ex);
            }
        }

        protected override string Url
        {
            get { return "https://market.huobi.com/staticmarket/detail_btc.js"; }
        }
    }
    public class Chbtc : BitcoinMarketBase
    {
        public override string Name
        {
            get { return "chbtc"; }
        }

        public Chbtc(IEnumerable<BitcoinMarketBase> collect)
            : base(collect)
        {
            try
            {
                var r = GetResponse();
                var start = r.IndexOf('[') + 1;
                var end = r.LastIndexOf(']');
                r = r.Substring(start, end - start);
                var info = JObject.Parse(r);
                var arr = info.Property("btcdefault_hotdata").Value.ToObject<decimal[]>();
                NewDealPrice = arr[0];
                MaxPrice = arr[3];
                MinPrice = arr[4];
            }
            catch (Exception ex)
            {
                Singleton<TextLog>.Instance.Error(ex);
            }
        }

        protected override string Url
        {
            get { return "https://trans.chbtc.com/line/topall?jsoncallback=jsonp"; }
        }
    }
    public class Btctrade : BitcoinMarketBase
    {
        public override string Name
        {
            get { return "btctrade"; }
        }

        public Btctrade(IEnumerable<BitcoinMarketBase> collect)
            : base(collect)
        {
            try
            {
                var r = GetResponse();
                var info = JObject.Parse(r);
                NewDealPrice = (decimal)info["last"];
                MaxPrice = (decimal)info["high"];
                MinPrice = (decimal)info["low"];
            }
            catch (Exception ex)
            {
                Singleton<TextLog>.Instance.Error(ex);
            }
        }

        protected override string Url
        {
            get { return "http://api.btctrade.com/api/ticker?coin=btc"; }
        }
    }

    public class RefreshModel
    {
        public MyMarket Market { get; set; }
        public MyBailAccount Bail { get; set; }
        public string OrderStr { get; set; }
        public string DealStr { get; set; }
    }
}
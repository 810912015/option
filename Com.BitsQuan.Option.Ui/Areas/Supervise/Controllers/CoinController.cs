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
using System.Web;
using System.Web.Mvc;

namespace Com.BitsQuan.Option.Ui.Areas.Supervise.Controllers
{
    [AuthViaMenu(Roles = "网站管理员")]
    [Log]
    public class CoinController : Controller
    {
        static int countPerPage = 10;
        OrderRepo orderRepo;
        OptionDbCtx db;
        //
        // GET: /Supervise/Coin/
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult CoinIndex()
        {
            return View();
        }

        public CoinController()
        {
            db = new OptionDbCtx();
            orderRepo = new OrderRepo(db);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (orderRepo != null)
                {
                    orderRepo.Dispose();
                    orderRepo = null;
                }
            }
            base.Dispose(disposing);
        }
        //修改与添加局部页面
        public PartialViewResult AddAndUpdCoin()
        {
            return PartialView();
        }
        //查询全部货币
        DbBackModel dbm = new DbBackModel();
        public PartialViewResult CoinList(string name, int pageIndex = 1)
        {

            //    name = "qq";
            if (name != null)
            {
                var coin = dbm.Coins.Where(a => a.Name == name);
                var pager = new Pager
                {
                    PageCount = (int)Math.Ceiling((double)dbm.Coins.Count() / (double)countPerPage),
                    PageIndex = pageIndex,
                    PostAction = "CoinList",
                    TargetId = "contractquery"
                };
                ViewData["pager"] = pager;
                return PartialView(coin);
            }
            else
            {
                var coin = dbm.Coins.OrderByDescending(a => a.Id).Skip((pageIndex - 1) * countPerPage)
                     .Take(countPerPage);
                var pager = new Pager
                {
                    PageCount = (int)Math.Ceiling((double)dbm.Coins.Count() / (double)countPerPage),
                    PageIndex = pageIndex,
                    PostAction = "CoinList",
                    TargetId = "contractquery"
                };
                ViewData["pager"] = pager;
                return PartialView(coin);
            }

        }



        //添加功能
        public JsonResult AddMethod(string name, string Code, decimal mainBailRatio, decimal mainBailTimes)
        {
            Coin c = new Coin
            {
                Id = IdService<Coin>.Instance.NewId(),
                Name = name,
                CotractCode = Code,
                MainBailRatio = mainBailRatio,
                MainBailTimes = mainBailTimes
            };

            if (ModelState.IsValid)
            {
                var r = dbm.AddCoin(c);
                return Json(r.Desc);
            }
            else
            {
                return Json("数据有错误,请仔细检查");
            }
        }

        //修改功能
        public JsonResult UpdateMethod(int id, string name, string Code, decimal mainBailRatio, decimal mainBailTimes)
        {
            Coin c = new Coin
            {
                Id = id,
                Name = name,
                CotractCode = Code,
                MainBailRatio = mainBailRatio,
                MainBailTimes = mainBailTimes
            };

            if (ModelState.IsValid)
            {
                var r = dbm.UpdateCoin(c);
                return Json(r.Desc);
            }
            else
            {
                return Json("数据有错误,请仔细检查");
            }
        }
        //根据编号查询对象
        public string GetCoinByID(int id)
        {
            if (id.ToString() != null && id != 0)
            {

                // 根据编号查询对象（在前台显示需要为真）
                var hs = dbm.Coins.Where(a => a.Id == id).FirstOrDefault();

                return hs.Name + "," + hs.CotractCode + "," + hs.MainBailRatio + "," + hs.MainBailTimes;
            }
            else
            {
                return null;
            }
        }

        //删除功能
        public JsonResult DeleteMethod(int id)
        {

            if (ModelState.IsValid)
            {
                var r = dbm.DeleteCoin(id);
                return Json(r.Desc);
            }
            else
            {
                return Json("数据有错误,请仔细检查");
            }
        }
    }
}
using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Match;
using Com.BitsQuan.Option.Provider;
using Com.BitsQuan.Option.Ui.Areas.Supervise.Data;
using Com.BitsQuan.Option.Ui.Areas.Supervise.Models;
using Com.BitsQuan.Option.Ui.Models;
using Com.BitsQuan.Option.Ui.Models.Security;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Com.BitsQuan.Option.Ui.Areas.Supervise.Controllers
{
    [AuthViaMenu(Roles = "网站管理员")]
    [Log]
    public class ContractsController : Controller
    {
        static int countPerPage = 10;

        public ViewResult Index()
        {
            return View();
        }

        public ActionResult Query(int? id, int pageIndex = 1)
        {
            OptionDbCtx db = new OptionDbCtx();
            var cs = db.Contracts
               .OrderByDescending(a => a.Id)
               .Skip((pageIndex - 1) * countPerPage)
               .Take(countPerPage).ToList();

            var pager = new Pager
            {
                PageCount = (int)Math.Ceiling((double)db.Contracts.Count() / (double)countPerPage),
                PageIndex = pageIndex,
                PostAction = "Query",
                TargetId = "contractquery"
            };
            ViewData["pager"] = pager;
            return PartialView(cs);

        }

        public PartialViewResult Create()
        {
            return PartialView();
        }


        [HttpPost]
        public JsonResult Create(ContractModel cm)
        {
            if (ModelState.IsValid)
            {
                cm.ExecuteTime = cm.ExecuteTime.AddHours(OrderExecutor.ExecuteTime);
                var r = MvcApplication.OptionService.esi.AddContract(cm.CoinName, cm.ExecuteTime, cm.ExecutePrice, cm.OptionType, cm.CoinName, cm.CoinCount);
                return Json(r.Desc);
            }
            else
            {
                return Json("数据有错误,请仔细检查");
            }
        }
        [HttpPost]
        public ActionResult IsDel(int id)
        {
            OptionDbCtx dbm = new OptionDbCtx();
            var c = dbm.Contracts.Where(d => d.Id == id).FirstOrDefault();
            var s = MvcApplication.OptionService.Model.Contracts.Where(a => a.Id == id).FirstOrDefault();
            if (s!=null)
            {
                return Content("当前期权正在交易，禁止删除");
            }
            c.IsDel = true;
            dbm.Contracts.Attach(c);
            dbm.Entry(c).State = EntityState.Modified;
            dbm.SaveChanges();
            var Contracts = MvcApplication.OptionService.Model.Contracts.Where(a => a.Id == id).FirstOrDefault();
            Contracts.IsDel = true;
            return Content("修改成功");
        }
    }
}
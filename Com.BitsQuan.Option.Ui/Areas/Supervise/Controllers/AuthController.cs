using Com.BitsQuan.Option.Imp;
using Com.BitsQuan.Option.Provider;
using Com.BitsQuan.Option.Ui.Models;
using Com.BitsQuan.Option.Ui.Models.Security;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
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
    public class AuthController : Controller
    {
        ApplicationDbContext db;
        OptionDbCtx odc;
        public AuthController()
        {
            db = new ApplicationDbContext();
            odc = new OptionDbCtx();
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
                if (odc != null)
                {
                    odc.Dispose();
                    odc = null;
                }
            }
            base.Dispose(disposing);
        }
        // GET: Supervise/Auth
        public ActionResult Index()
        {
            return View();
        }
        static int countPerPage = 10;
        public PartialViewResult Query(int pageIndex = 1)
        {
            var cs = db.Set<ApplicationUser>().OrderByDescending(a => a.RegisterTime)
                .Skip((pageIndex - 1) * countPerPage)
                .Take(countPerPage);
            var show = db.Set<ApplicationUser>().ToList();
            var pager = new Pager
            {
                PageCount = (int)Math.Ceiling((double)db.Set<ApplicationUser>().Count() / (double)countPerPage),
                PageIndex = pageIndex,
                PostAction = "Query",
                TargetId = "userquery"
            };
            ViewData["pager"] = pager;
            ViewData["Count"] = show.Count();
            DateTime end = DateTime.Now.Date.AddDays(1);
            ViewData["todayCount"] = show.Where(a => a.RegisterTime >= DateTime.Now.Date && a.RegisterTime < end).Count();

            // var kk = DateTime.Now.Date.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss.fff");

            //  ViewData["time"] = Convert.ToDateTime(kk) + "///nnn/" + DateTime.Now.Date;
            //   ViewData["time"] = DateTime.Now.Date.AddDays(1);
            //   ViewData["time"] = show.Where(a=>a.RegisterTime>=Convert.ToDateTime(DateTime.Now.Date.ToString("yyyy-MM-dd")));
            return PartialView(cs);
        }
        public JsonResult AllowTrade(string uid)
        {
            if (string.IsNullOrEmpty(uid)) return Json(new OperationResult { ResultCode = 1, Desc = "用户id为空" }, JsonRequestBehavior.AllowGet);
            var u = db.Set<ApplicationUser>().Find(uid);
            if (u == null) return Json(new OperationResult { ResultCode = 2, Desc = "没有此用户" },
                    JsonRequestBehavior.AllowGet);
            var r = MvcApplication.OptionService.esi.CreateTrader(u.UserName, false, false);
            u.IsAllowToTrade = true;
            db.SaveChanges();
            return Json(r,
                 JsonRequestBehavior.AllowGet);
        }
        public PartialViewResult UpdateUser()
        {
            return PartialView();
        }
        public JsonResult GetAuthByID(string id)
        {
            if (string.IsNullOrEmpty(id)) return Json(new OperationResult { ResultCode = 1, Desc = "用户id为空" }, JsonRequestBehavior.AllowGet);
            var u = db.Set<ApplicationUser>().Find(id);
            if (u == null) return Json(new OperationResult { ResultCode = 2, Desc = "没有此用户" },
                    JsonRequestBehavior.AllowGet);
            return Json(u, JsonRequestBehavior.AllowGet);
        }
        public string UpdateMethod(ApplicationUser u)
        {
            IUserStore<ApplicationUser> sdf = new UserStore<ApplicationUser>(new ApplicationDbContext());
            var a = db.Set<ApplicationUser>().Find(u.Id);
            UserManager<ApplicationUser> UserManager = new UserManager<ApplicationUser>(sdf);
            if (a.PasswordHash != u.PasswordHash)
            {
                var s = UserManager.CreateAsync(u, u.PasswordHash);
            }


            a.IdNumber = u.IdNumber;
            a.Email = u.Email;
            a.TradePwd = u.TradePwd;
            a.PhoneNumber = u.PhoneNumber;
            a.PasswordHash = u.PasswordHash;
            db.Set<ApplicationUser>().Attach(a);
            db.Entry(a).State = EntityState.Modified;
            db.SaveChanges();
            return "修改成功";
        }
    }
}
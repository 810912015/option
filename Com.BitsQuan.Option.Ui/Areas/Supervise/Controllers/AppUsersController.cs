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
    public class AppUsersController : Controller
    {
        ApplicationDbContext db;
        BaseRepository<ApplicationUser> auRepo;

        public AppUsersController()
        {
            db = new ApplicationDbContext();
            auRepo = new BaseRepository<ApplicationUser>();
            auRepo.SetCtx(db);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (auRepo != null)
                {
                    auRepo.Dispose();
                    auRepo = null;
                }
                if (db != null)
                {
                    db.Dispose();
                    db = null;
                }
            }
            base.Dispose(disposing);
        }

        // GET: Supervise/AppUsers
        public ActionResult Index()
        {
            return View();
        }
        public PartialViewResult Summary()
        {
            var sm = new SummaryModel(db);
            return PartialView(sm);
        }
        public JsonResult UpdateUser(
            string id,
            string idNumber,
            string identityType,
            string phoneNumber,
            string email,
            string realName,
            decimal ratio,
            string invitor,
            string uid)
        {
            dynamic result=null;
            var user = auRepo.Find(_ => _.Id == id);
            if (user != null)
            {
                if (ratio < 0 || ratio > 1)
                    return Json(new { result = "费率必须大于等于0小于等于1" }, JsonRequestBehavior.AllowGet);

                user.RealityName = realName;
                user.IdNumber = idNumber;
                user.IdNumberType = identityType;
                user.PhoneNumber = phoneNumber;
                user.Email = email;
                user.InvitorFeeRatio = ratio;
                user.Uiden = uid;
                if (string.IsNullOrWhiteSpace(invitor))
                    user.InvitorId = null;
                else
                {
                    var invitorUsr = auRepo.Find(_ => _.UserName == invitor);
                    if (invitorUsr == null)
                    {
                        return Json(new { result = "推荐人不存在" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        user.InvitorId = invitorUsr.Id;
                    }
                }
                if (auRepo.Update(user))
                    result = new { result = "修改成功" };
                else
                    result = new { result = "修改失败" };
            }
            else
                result = new { reuslt = "用户不存在" };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public PartialViewResult Users()
        {
            QueryEngine aqa = null;
            var r = QueryEngine.Query<ApplicationUser, DateTime?>(ref aqa, auRepo, "/AppUsers/UserQuery", a => a.RegisterTime);
            ViewData["args"] = aqa;
            ViewData["action"] = "UserQuery";
            ViewData["controller"] = "AppUsers";
            return PartialView();
        }

        public PartialViewResult UserQuery(QueryEngine aqa = null)
        {
            var r = QueryEngine.Query<ApplicationUser, DateTime?>(ref aqa, auRepo, "/AppUsers/UserQuery", a => a.RegisterTime);
            ViewData["args"] = aqa;
            return PartialView(r);
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
            return Json(r, JsonRequestBehavior.AllowGet);
        }
    }
    public class SummaryModel
    {
        public int Total { get; set; }
        public int Today { get; set; }
        public SummaryModel(ApplicationDbContext db)
        {
            Total = db.Users.Count();
            var yesterday = DateTime.Now.Date;
            var q = db.Users.Where(a => a.RegisterTime != null && ((DateTime)a.RegisterTime) > yesterday);
            if (q != null)
                Today = q.Count<ApplicationUser>();
        }
    }
}
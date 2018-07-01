using Com.BitsQuan.Option.Models.Security;
using Com.BitsQuan.Option.Provider;
using Com.BitsQuan.Option.Ui.Models;
using Com.BitsQuan.Option.Ui.Models.Security;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Com.BitsQuan.Option.Ui.Areas.Supervise.Controllers
{
    public class MenuRepo : BaseRepository<Menu>
    {
        public MenuRepo(ApplicationDbContext db)
        {
            this.dbContext = db;
        }

        //public List<Menu> All { get { return dbContext.Set<Menu>().ToList<Menu>(); } }
        public Menu GetById(int id)
        {
            return Find(a => { return a.Id == id; });
        }

        public static List<Menu> GetChildBySubId(int SubId)
        {
            var db = new ApplicationDbContext();
            var q = db.Menus.Where(c => c.SubId == SubId).ToList();
            return q;


        }
        public static List<Menu> GeTopAll()
        {
            var db = new ApplicationDbContext();
            var q = db.Menus.Where(c => c.SubId == null || c.SubId == 0).ToList();
            return q;


        }
    }
    public class CreateUserModel
    {
        [Required]
        public string RoleId { get; set; }
        [Required]
        [Display(Name = "用户名")]
        [System.Web.Mvc.Remote("IsUserNameUsable", "security", ErrorMessage = "用户名已使用")]
        [RegularExpression("^[(A-Za-z0-9)]{4,30}$", ErrorMessage = "4-30个英文、数字")]

        public string UserName { get; set; }


        [Required]
        [Display(Name = "登录密码")]


        [RegularExpression("^[a-zA-Z]{1}([a-zA-Z0-9]|[._]){0,}$", ErrorMessage = "必须以字母开头字串(可带数字或_或.)")]
        //[StringLength(10, ErrorMessage = "{0} 必须为{2}至{1}个字符。", MinimumLength = 8)]
        [MinLength(8, ErrorMessage = "密码不能小于8位")]
        [MaxLength(20, ErrorMessage = "密码不能大于20位")]

        [DataType(DataType.Password)]
        public string Password { get; set; }


        [DataType(DataType.Password)]
        [Display(Name = "确认密码")]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "登录密码和确认登录密码不匹配。")]
        public string ConfirmPassword { get; set; }

        [RegularExpression("^[a-zA-Z]{1}([a-zA-Z0-9]|[._]){0,}$", ErrorMessage = "必须以字母开头字串(可带数字或_或.)")]
        [MinLength(8, ErrorMessage = "密码不能小于8位")]
        [MaxLength(20, ErrorMessage = "密码不能大于20位")]

        [DataType(DataType.Password)]
        public string TradePassword { get; set; }
        [System.ComponentModel.DataAnnotations.Compare("TradePassword", ErrorMessage = "授权码与确认授权码不匹配。")]
        public string ConfirmTradePassword { get; set; }
    }
    public class UserViewModel
    {
        public ApplicationUser User { get; set; }
        public List<string> RoleNames { get; set; }
    }
    public class UserRoleViewModel
    {
        [Display(Name = "用户名")]
        public string UserName { get; set; }
        [Display(Name = "选择角色")]
        public string RoleId { get; set; }

        public bool IsDelete { get; set; }
    }
    [AuthViaMenu(Roles = "网站管理员")]
    [Log]
    public class SecurityController : Controller
    {
        ApplicationDbContext db;

        UserStore<ApplicationUser> userRepo;
        RoleStore<ApplicationRole> roleRepo;
        public UserManager<ApplicationUser> UserManager { get; private set; }
        RoleManager<ApplicationRole> RoleManager;
        MenuRepo MR;

        public SecurityController()
        {
            db = new ApplicationDbContext();
            MR = new MenuRepo(db);
            userRepo = new UserStore<ApplicationUser>(db);
            UserManager = new UserManager<ApplicationUser>(userRepo);
            roleRepo = new RoleStore<ApplicationRole>(db);
            RoleManager = new RoleManager<ApplicationRole>(roleRepo);

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && UserManager != null)
            {
                UserManager.Dispose();
                UserManager = null;
            }
            base.Dispose(disposing);
        }

        //
        // GET: /Supervise/Security/
        public ActionResult Index()
        {
            return View();
        }
        [AllowAnonymous]
        public JsonResult IsUserNameUsable(string userName)
        {
            if (string.IsNullOrEmpty(userName)) return Json(false, JsonRequestBehavior.AllowGet);
            var q = UserManager.FindByName(userName);
            if (q == null) { return Json(true, JsonRequestBehavior.AllowGet); }
            else return Json(false, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        //[ValidateAntiForgeryToken]
        public ActionResult CreateUser()
        {
            ViewData["roles"] = GetRoleSel();
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateUser(CreateUserModel m)
        {
            if (!ModelState.IsValid)
            {
                ViewData["roles"] = GetRoleSel();
                return View(m);
            }
            var au = new ApplicationUser { UserName = m.UserName, EmailConfirmed = true, TradePwd = m.TradePassword, RegisterTime = DateTime.Now };
            var role = RoleManager.FindById(m.RoleId);

            var r = UserManager.Create(au, m.Password);
            if (!r.Succeeded)
            {
                foreach (var v in r.Errors)
                    ModelState.AddModelError("", v);
                ViewData["roles"] = GetRoleSel();
                return View(m);
            }
            var adr = UserManager.AddToRole(au.Id, role.Name);
            if (!adr.Succeeded)
            {
                foreach (var v in adr.Errors)
                    ModelState.AddModelError("", v);
                ViewData["roles"] = GetRoleSel();
                return View(m);
            }
            MenuManager.Instance.UpdateUserRole();
            return RedirectToAction("users");
        }
        [HttpGet]
        public ActionResult EditUser(string id)
        {
            var u = UserManager.FindById(id);
            return View(u);
        }
        [HttpPost]
        public ActionResult EditUser(ApplicationUser u)
        {
            var au = UserManager.FindById(u.Id);
            var r = UserManager.Update(au);
            return RedirectToAction("users");
        }
        public ActionResult EditUserRole(string id)
        {
            var u = UserManager.FindById(id);
            UserRoleViewModel urv = new UserRoleViewModel { UserName = u.UserName };
            ViewData["roles"] = GetRoleSel();
            return View(urv);
        }
        [HttpPost]
        public ActionResult EditUserRole(UserRoleViewModel urv)
        {
            if (urv.UserName == null) return RedirectToAction("users");// Redirect("/account/users");
            var u = UserManager.FindByName(urv.UserName);
            if (u == null) return RedirectToAction("users");// Redirect("/account/users");
            var r = RoleManager.FindById(urv.RoleId);
            if (r == null) return RedirectToAction("users");// Redirect("/account/users");
            if (!urv.IsDelete)
            {
                var result = UserManager.AddToRole(u.Id, r.Name);
            }
            else UserManager.RemoveFromRole(u.Id, r.Name);

            MenuManager.Instance.UpdateUserRole();
            return RedirectToAction("users");// Redirect("/account/users");
        }

        public ActionResult Users()
        {
            List<UserViewModel> l = new List<UserViewModel>();

            var tr = RoleManager.FindByName("交易员");

            var u = userRepo.Users.ToList<ApplicationUser>();
            foreach (var a in u)
            {
                if (tr!=null&& UserManager.IsInRole(a.Id, tr.Id)) continue;
                var vm = new UserViewModel { User = a, RoleNames = new List<string>(UserManager.GetRoles(a.Id)) };
                l.Add(vm);
            }

            return View(l);
        }
        //[AuthViaMenu(Roles = "组织管理者")]
        public ActionResult Roles()
        {
            return View(roleRepo.Roles);
        }
        List<SelectListItem> GetRoleSel(string id = "")
        {
            var q = from a in roleRepo.Roles
                    where a.Id != id
                    select new SelectListItem { Text = a.Name, Value = a.Id, Selected = a.Id == id };
            List<SelectListItem> l = new List<SelectListItem>();
            l.Add(new SelectListItem { Text = "请选择", Value = "", Selected = string.IsNullOrEmpty(id) });
            l.AddRange(q);
            return l;
        }
        //[AuthViaMenu(Roles = "组织管理者")]
        public ActionResult CreateRole()
        {
            ViewData["roles"] = GetRoleSel();

            ViewData["roseUser"] = new ApplicationRole();
            return View();
        }
        [HttpPost]
        //[AuthViaMenu(Roles = "组织管理者")]
        public ActionResult CreateRole(ApplicationRole r, string selected)
        {
            if (ModelState.IsValid)
            {
                string[] str = selected.Split(',');

                if (str != null && str.Length > 0)
                {
                    r.Menus = new List<Menu>();

                    foreach (var item in str)
                    {
                        if (string.IsNullOrEmpty(item)) continue;
                        int id = int.Parse(item);
                        var m = MR.Find(a => a.Id == id);
                        if (m != null)
                            r.Menus.Add(m);
                    }
                }

                var cr = RoleManager.Create(r);

                if (cr.Succeeded)
                {

                    MenuManager.Instance.UpdateRoleMenu(r.Id, str);
                    return RedirectToAction("roles");// Redirect("/security/roles");
                }
                else
                    foreach (var v in cr.Errors)
                        ModelState.AddModelError("Name", v);

            }
            ViewData["roles"] = GetRoleSel();
            return View(r);
        }
        //[AuthViaMenu(Roles = "组织管理者")]
        public ActionResult EditRole(string id)
        {
            ViewData["roles"] = GetRoleSel(id);
            var v = RoleManager.FindById(id);

            List<int> RoseStr = new List<int>();

            foreach (var m in v.Menus)
            {
                RoseStr.Add(m.Id);

            }
            ViewData["roseUser"] = RoseStr;
            return View(v);
        }
        [HttpPost]
        //[AuthViaMenu(Roles = "组织管理者")]
        public ActionResult EditRole(ApplicationRole r, string selected)
        {
            if (ModelState.IsValid)
            {
                var rr = RoleManager.FindById(r.Id);
                if (rr == null)
                { rr = new ApplicationRole(); }
                string[] str = selected.Split(',');
                rr.Menus.Clear();
                foreach (var item in str)
                {
                    int id = 0;
                    if (!int.TryParse(item, out id)) continue;
                    var m = MR.Find(a => a.Id == id);
                    if (m != null)
                    {
                        rr.Menus.Add(m);
                        m.Roles.Add(rr);
                    }
                }
                var cr = RoleManager.Update(rr);
                MenuManager.Instance.UpdateRoleMenu(r.Id, str);
                return RedirectToAction("roles");// Redirect("/account/roles");
            }
            ViewData["roles"] = GetRoleSel(r.Id);
            return View(r);
        }
        //[AuthViaMenu(Roles = "组织管理者")]
        public ActionResult DeleteRole(string id)
        {
            ViewData["roles"] = GetRoleSel();
            var r = RoleManager.FindById(id);
            r.Users.Clear();
            r.Menus.Clear();
            var cr = RoleManager.Update(r);
            if (cr.Succeeded)
            {
                var rr = RoleManager.FindById(id);

                RoleManager.Delete(rr);
            }


            return RedirectToAction("roles");// Redirect("/account/roles");
        }
        [HttpPost]
        //[AuthViaMenu(Roles = "组织管理者")]
        public ActionResult DeleteRoleConfirmed(string Id)
        {
            var r = RoleManager.FindById(Id);
            if (r != null)
                RoleManager.Delete(r);
            MenuManager.Instance.UpdateUserRole();
            return RedirectToAction("roles");// Redirect("/account/roles");
        }
    }
}